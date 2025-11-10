using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ComputerStore.Services;
using ComputerStore.Models;
using ComputerStore.Data;
using System.Collections.Generic;
using System.Transactions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Linq; // agregado para GroupBy

namespace ComputerStore.Pages
{
    public class ConfirmacionPagoModel : PageModel
    {
        private readonly LittleCarService _carritoService;
        private readonly IPayUService _payUService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context; // ? persistencia en BD
        private readonly ILogger<ConfirmacionPagoModel> _logger; // ? logging

        public string TransactionState { get; private set; }
        public string ReferenceCode { get; private set; }
        public string ResponseMessage { get; private set; }
        public string OrderId { get; private set; }
        public string TransactionId { get; private set; }
        public string PaymentMethod { get; set; }

        public bool IsPaymentApproved { get; private set; }
        public bool IsPaymentDeclined { get; private set; }
        public bool IsPaymentPending { get; private set; }

        public decimal Total { get; private set; }

        public ConfirmacionPagoModel(LittleCarService carritoService, IPayUService payUService, 
            UserManager<ApplicationUser> userManager, ApplicationDbContext context, 
            ILogger<ConfirmacionPagoModel> logger)
        {
            _carritoService = carritoService;
            _payUService = payUService;
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public async Task OnGetAsync([FromQuery] Dictionary<string, string> queryParameters)
        {
            _logger.LogInformation("=== Parámetros recibidos en Confirmación ===");
            foreach (var param in queryParameters)
                _logger.LogInformation("{Key}: {Value}", param.Key, param.Value);

            TransactionState = ObtenerParametro(queryParameters, "transactionState", "state", "polTransactionState", "lapTransactionState") 
                ?? (TempData["TransactionState"] as string ?? "UNKNOWN");
            ReferenceCode = ObtenerParametro(queryParameters, "referenceCode", "reference", "reference_pol", "trazabilityCode") 
                ?? (TempData["ReferenceCode"] as string ?? "N/A");
            ResponseMessage = ObtenerParametro(queryParameters, "message", "responseMessage", "polResponseMessage") 
                ?? (TempData["ResponseMessage"] as string ?? "Sin mensaje");
            OrderId = ObtenerParametro(queryParameters, "orderId", "order_id", "polOrderId") 
                ?? (TempData["OrderId"] as string);
            TransactionId = ObtenerParametro(queryParameters, "transactionId", "transaction_id", "polTransactionId") 
                ?? (TempData["TransactionId"] as string ?? "N/A");
            PaymentMethod = ObtenerParametro(queryParameters, "paymentMethod", "payment_method", "lapPaymentMethod") 
                ?? (TempData["PaymentMethod"] as string ?? "Desconocido");

            // Resolver usuario
            ApplicationUser usuario = null;
            try
            {
                usuario = await _userManager.GetUserAsync(User);
                if (usuario == null)
                {
                    // Intentar recuperar desde la caché de la tarjeta
                    var info = PayUService.RecuperarDatosTarjetaPorTx(TransactionId);
                    if (info != null && info.TryGetValue("userId", out var uid) && uid is string u && !string.IsNullOrEmpty(u))
                    {
                        usuario = await _userManager.FindByIdAsync(u);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo usuario en confirmación");
            }

            // Caso PSE pendiente/redirect: recuperar datos guardados
            if (PaymentMethod == "PSE" && !string.IsNullOrEmpty(TransactionId))
            {
                try
                {
                    var pseKey = PayUService.RecuperarClaveSegunTransactionId(TransactionId);
                    if (!string.IsNullOrEmpty(pseKey))
                    {
                        var pseData = PayUService.RecuperarDatosPSEPorClave(pseKey);
                        if (pseData != null)
                        {
                            OrderId = pseData["orderId"].ToString();
                            if (pseData.ContainsKey("referenceCode"))
                                ReferenceCode = pseData["referenceCode"].ToString();

                            var normalized = NormalizarEstadoTransaccion(TransactionState);
                            if (normalized == "APPROVED" && pseData.ContainsKey("carrito") && usuario != null)
                            {
                                var carritoGuardado = pseData["carrito"] as List<Products>;
                                if (carritoGuardado != null && carritoGuardado.Any())
                                {
                                    await GuardarPedidoAprobadoEnBD(OrderId, TransactionId, carritoGuardado, usuario, PaymentMethod);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al recuperar datos PSE");
                }
            }

            if (string.IsNullOrEmpty(OrderId) || OrderId == "0") OrderId = "N/A";
            TransactionState = NormalizarEstadoTransaccion(TransactionState);

            IsPaymentApproved = false;
            IsPaymentDeclined = false;
            IsPaymentPending = false;

            switch (TransactionState)
            {
                case "APPROVED":
                case "4":
                case "ACCEPTED":
                    IsPaymentApproved = true;
                    await ProcesarPagoAprobado(usuario);
                    break;
                case "DECLINED":
                case "6":
                case "REJECTED":
                case "CANCELLED":
                    IsPaymentDeclined = true;
                    break;
                case "PENDING":
                case "7":
                case "PROCESSING":
                    IsPaymentPending = true;
                    break;
                default:
                    IsPaymentDeclined = true;
                    if (string.IsNullOrEmpty(ResponseMessage) || ResponseMessage == "Sin mensaje")
                        ResponseMessage = $"Estado desconocido: {TransactionState}";
                    break;
            }
        }

        private async Task GuardarPedidoAprobadoEnBD(string orderId, string transactionId,
            List<Products> carrito, ApplicationUser usuario, string metodoPago)
        {
            try
            {
                var pedidoExistente = await _context.Pedidos.FirstOrDefaultAsync(p => p.TransactionId == transactionId);
                if (pedidoExistente != null) return;

                var utcNow = DateTime.UtcNow; // Persistir SIEMPRE en UTC por Npgsql
                var detalles = new List<PedidoDetalle>();

                // Consolidar cantidades por producto (el carrito puede tener múltiples instancias del mismo Id)
                var grupos = (carrito ?? new List<Products>())
                    .Where(p => p != null && p.Id >0)
                    .GroupBy(p => p.Id)
                    .ToList();

                foreach (var g in grupos)
                {
                    var productoBD = await _context.Productos
                        .AsTracking() // CRITICO: habilita tracking pese al NoTracking global
                        .FirstOrDefaultAsync(p => p.Id == g.Key);

                    if (productoBD == null)
                    {
                        _logger.LogWarning("Producto no encontrado en BD: Id={Id}", g.Key);
                        continue;
                    }

                    int cantidad = g.Count();
                    decimal precioUnitario = productoBD.Price; // usar precio actual
                    decimal subtotal = precioUnitario * cantidad;

                    detalles.Add(new PedidoDetalle
                    {
                        ProductoId = productoBD.Id,
                        Cantidad = cantidad,
                        PrecioUnitario = precioUnitario,
                        Subtotal = subtotal
                    });

                    // Descontar stock de forma segura
                    var stockAnterior = productoBD.Stock;
                    productoBD.Stock = productoBD.Stock - cantidad;
                    if (productoBD.Stock <0) productoBD.Stock =0;
                    if (productoBD.Stock ==0)
                    {
                        // Marcar como agotado para impedir nuevas compras
                        productoBD.Availability = "Agotado";
                    }
                    productoBD.FechaActualizacion = utcNow;

                    _logger.LogInformation("Stock reducido: Producto {ProductoId} {Nombre}, Cantidad={Cantidad}, Stock {Anterior}->{Nuevo}",
      productoBD.Id, productoBD.Name, cantidad, stockAnterior, productoBD.Stock);
                }

                // GUARDAR CAMBIOS DE STOCK ANTES DE CREAR EL PEDIDO
                await _context.SaveChangesAsync();
                _logger.LogInformation("Cambios de stock guardados en base de datos");

                // Calcular total
                var total = detalles.Any() ? detalles.Sum(d => d.Subtotal) : (carrito?.Sum(p => p.Price) ??0);
                if (total <=0) total = _carritoService.ObtenerUltimoTotal();

                var nuevoPedido = new PedidoEntity
                {
                    UserId = usuario?.Id ?? "Sistema",
                    TransactionId = transactionId,
                    ReferenceCode = ReferenceCode ?? ($"ORD-{Guid.NewGuid():N}").Substring(0,16),
                    Total = total,
                    Estado = "Pagado",
                    MetodoPago = metodoPago,
                    FechaCreacion = utcNow,
                    FechaPago = utcNow,
                    DireccionEnvio = usuario?.Direccion ?? "Dirección por defecto",
                    CiudadEnvio = usuario?.Ciudad ?? "Bogotá",
                    DepartamentoEnvio = usuario?.Departamento ?? "Cundinamarca",
                    CodigoPostalEnvio = usuario?.CodigoPostal ?? "000000",
                    TelefonoContacto = usuario?.PhoneNumber,
                    NotasEntrega = usuario?.NotasEntrega,
                    Detalles = detalles
                };

                _context.Pedidos.Add(nuevoPedido);
                await _context.SaveChangesAsync();

                var envio = new EnvioEntity
                {
                    PedidoId = nuevoPedido.Id,
                    NumeroGuia = GenerarNumeroGuia(),
                    Estado = "PREPARANDO",
                    FechaCreacion = utcNow,
                    FechaEstimadaEntrega = utcNow.AddDays(3),
                    DireccionEnvio = nuevoPedido.DireccionEnvio,
                    CiudadEnvio = nuevoPedido.CiudadEnvio,
                    DepartamentoEnvio = nuevoPedido.DepartamentoEnvio,
                    CodigoPostal = nuevoPedido.CodigoPostalEnvio,
                    TelefonoContacto = nuevoPedido.TelefonoContacto,
                    CostoEnvio =0
                };

                _context.Envios.Add(envio);
                await _context.SaveChangesAsync();

                await GuardarTransaccionEnBD(transactionId, nuevoPedido.Id, metodoPago, total, "APPROVED");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar pedido en BD");
            }
        }

        private async Task GuardarTransaccionEnBD(string transactionId, int pedidoId, 
            string metodoPago, decimal monto, string estado)
        {
            try
            {
                var transaccionExistente = await _context.Transacciones
                    .FirstOrDefaultAsync(t => t.TransactionId == transactionId);

                if (transaccionExistente != null)
                {
                    if (transaccionExistente.Estado != estado)
                    {
                        transaccionExistente.Estado = estado;
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    var nuevaTransaccion = new TransaccionEntity
                    {
                        TransactionId = transactionId,
                        PedidoId = pedidoId,
                        Estado = estado,
                        MetodoPago = metodoPago,
                        Monto = monto,
                        Moneda = "COP",
                        ReferenceCode = ReferenceCode,
                        ResponseMessage = ResponseMessage,
                        TrazabilityCode = ReferenceCode,
                        FechaTransaccion = DateTime.UtcNow
                    };

                    _context.Transacciones.Add(nuevaTransaccion);
                    await _context.SaveChangesAsync();
                }

                // Crear notificación admin cuando se aprueba
                if (estado == "APPROVED")
                {
                    try
                    {
                        _context.AdminNotifications.Add(new AdminNotification
                        {
                            UserId = null, // global para admins
                            Mensaje = $"Nueva compra aprobada {metodoPago}: {monto:C0} (TX {transactionId})",
                            Tipo = "success",
                            FechaCreacion = DateTime.UtcNow,
                            Leida = false
                        });
                        await _context.SaveChangesAsync();
                    }
                    catch { /* evitar bloquear flujo por notificación */ }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar transacción en BD");
            }
        }

        private string GenerarNumeroGuia() => $"GU{DateTime.Now:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";

        private string NormalizarEstadoTransaccion(string estado)
        {
            if (string.IsNullOrEmpty(estado)) return "UNKNOWN";
            return estado.ToUpper() switch
            {
                "4" => "APPROVED",
                "6" => "DECLINED", 
                "7" => "PENDING",
                "APROBADA" => "APPROVED",
                "RECHAZADA" => "DECLINED",
                "PENDIENTE" => "PENDING",
                _ => estado.ToUpper()
            };
        }

        private string ObtenerParametro(Dictionary<string, string> queryParameters, params string[] keys)
        {
            foreach (var key in keys)
            {
                if (queryParameters.ContainsKey(key) && !string.IsNullOrEmpty(queryParameters[key]))
                    return queryParameters[key];
            }
            return null;
        }

        private async Task ProcesarPagoAprobado(ApplicationUser usuario)
        {
            try
            {
                _logger.LogInformation("ProcesarPagoAprobado INIT. Tx={Tx} User={User}", TransactionId, usuario?.Id);
                var carritoActual = _carritoService.ObtenerCarrito();
                _logger.LogInformation("Carrito session count={Count}", carritoActual?.Count ?? 0);
                if (carritoActual == null || !carritoActual.Any())
                {
                    var cacheTarjeta = PayUService.RecuperarDatosTarjetaPorTx(TransactionId);
                    _logger.LogInformation("Cache tarjeta encontrado? {Found}", cacheTarjeta != null);
                    if (cacheTarjeta != null && cacheTarjeta.TryGetValue("carrito", out var obj) && obj is List<Products> list)
                    {
                        carritoActual = list;
                    }
                    if ((carritoActual == null || !carritoActual.Any()))
                    {
                        carritoActual = _carritoService.ObtenerUltimaCompra();
                        _logger.LogInformation("Ultima compra count={Count}", carritoActual?.Count ?? 0);
                    }
                }

                if (usuario == null)
                {
                    var cacheTarjeta = PayUService.RecuperarDatosTarjetaPorTx(TransactionId);
                    var uid = cacheTarjeta != null && cacheTarjeta.TryGetValue("userId", out var o) ? o as string : null;
                    if (!string.IsNullOrEmpty(uid))
                    {
                        usuario = await _userManager.FindByIdAsync(uid);
                        _logger.LogInformation("Usuario recuperado desde cache: {User}", uid);
                    }
                }

                if (carritoActual != null && carritoActual.Any() && usuario != null && !string.IsNullOrEmpty(TransactionId))
                {
                    _logger.LogInformation("Persistiendo pedido. Items={Count} User={User}", carritoActual.Count, usuario.Id);
                    await GuardarPedidoAprobadoEnBD(OrderId, TransactionId, carritoActual, usuario, PaymentMethod);
                    _carritoService.GuardarUltimaCompra(carritoActual);
                    _carritoService.VaciarCarrito();
                }
                else
                {
                    _logger.LogWarning("No se pudo persistir: Items={Items} UserNull={UserNull} Tx={Tx}", carritoActual?.Count ?? 0, usuario == null, TransactionId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar carrito aprobado");
            }
        }

        public IActionResult OnPostRedirigirPago(string transactionState, string referenceCode, string responseMessage,
            string orderId = null, string transactionId = null)
        {
            TempData["TransactionState"] = transactionState;
            TempData["ReferenceCode"] = referenceCode;
            TempData["ResponseMessage"] = responseMessage;
            if (!string.IsNullOrEmpty(orderId)) TempData["OrderId"] = orderId;
            if (!string.IsNullOrEmpty(transactionId)) TempData["TransactionId"] = transactionId;
            return RedirectToPage("/ConfirmacionPago");
        }

        public IActionResult OnGetConfirmPayment(string transactionState, string referenceCode, string orderId, string transactionId)
        {
            if (transactionState.Equals("approved", StringComparison.OrdinalIgnoreCase))
            {
                var productos = _carritoService.ObtenerUltimaCompra() ?? _carritoService.ObtenerCarrito();
                _carritoService.VaciarCarrito();
                return RedirectToPage("/Index");
            }
            return RedirectToPage("/Error");
        }

        // Handler para descargar el recibo en PDF
        public async Task<IActionResult> OnGetDownloadReceipt(string transactionState, string referenceCode, string orderId, string transactionId, string paymentMethod)
        {
            try
            {
                // Buscar pedido por referencia o transactionId
                var pedido = await _context.Pedidos
                    .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                    .Include(p => p.User)
                    .Include(p => p.Envio)
                    .FirstOrDefaultAsync(p => p.ReferenceCode == referenceCode || p.TransactionId == transactionId);

                if (pedido == null)
                    return NotFound("Pedido no encontrado");

                // Documento PDF con QuestPDF
                QuestPDF.Settings.License = LicenseType.Community;

                var doc = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(30);
                        page.Size(PageSizes.A4);
                        page.DefaultTextStyle(x => x.FontSize(10));
                        page.Header().Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("CompuHiperMegaRed").Bold().FontSize(18);
                                col.Item().Text("NIT: 900.000.000-1");
                                col.Item().Text("Cra. 15 #78-33, Chapinero - Bogotá, Colombia");
                                col.Item().Text("Tel: +57 323 768 4390 - Email: lugapemu98@gmail.com");
                            });
                            row.ConstantItem(100).Height(60).Background(Colors.Blue.Medium);
                        });

                        page.Content().Column(col =>
                        {
                            // Datos de recibo
                            col.Item().Row(r =>
                            {
                                r.RelativeItem().Column(c =>
                                {
                                    c.Item().Text($"Recibo / Factura: {pedido.ReferenceCode}").Bold().FontSize(14);
                                    c.Item().Text($"Fecha: {pedido.FechaCreacion:dd/MM/yyyy HH:mm} (UTC)");
                                    c.Item().Text($"Estado: {pedido.Estado}");
                                    c.Item().Text($"Método de pago: {pedido.MetodoPago}");
                                    if (!string.IsNullOrWhiteSpace(pedido.TransactionId))
                                        c.Item().Text($"Transacción: {pedido.TransactionId}");
                                });
                                r.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Cliente").Bold();
                                    c.Item().Text($"{pedido.User?.PrimerNombre} {pedido.User?.PrimerApellido}");
                                    c.Item().Text(pedido.User?.Email ?? "");
                                    c.Item().Text($"Documento: {pedido.User?.NumeroDocumento}");
                                });
                            });

                            // Dirección de envío
                            col.Item().Text("Dirección de envío").Bold();
                            col.Item().Text($"{pedido.DireccionEnvio}, {pedido.CiudadEnvio}, {pedido.DepartamentoEnvio}, CP {pedido.CodigoPostalEnvio}");
                            if (!string.IsNullOrWhiteSpace(pedido.TelefonoContacto))
                                col.Item().Text($"Teléfono: {pedido.TelefonoContacto}");

                            col.Item().LineHorizontal(0.5f);

                            // Tabla de items
                            col.Item().Table(t =>
                            {
                                t.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(6);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                t.Header(h =>
                                {
                                    h.Cell().Element(CellHeader).Text("Producto");
                                    h.Cell().Element(CellHeader).Text("Precio");
                                    h.Cell().Element(CellHeader).Text("Cantidad");
                                    h.Cell().Element(CellHeader).Text("Subtotal");
                                    static IContainer CellHeader(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).Padding(4).Background(Colors.Grey.Lighten3);
                                });

                                if (pedido.Detalles != null && pedido.Detalles.Any())
                                {
                                    foreach (var d in pedido.Detalles)
                                    {
                                        t.Cell().Element(Cell).Text(d.Producto?.Name ?? "Producto");
                                        t.Cell().Element(Cell).Text($"{d.PrecioUnitario:C0}");
                                        t.Cell().Element(Cell).Text(d.Cantidad.ToString());
                                        t.Cell().Element(Cell).Text($"{d.Subtotal:C0}");
                                    }
                                }
                                else
                                {
                                    // fallback: un item con total
                                    t.Cell().Element(Cell).Text("Compra procesada");
                                    t.Cell().Element(Cell).Text($"{pedido.Total:C0}");
                                    t.Cell().Element(Cell).Text("1");
                                    t.Cell().Element(Cell).Text($"{pedido.Total:C0}");
                                }

                                static IContainer Cell(IContainer container) => container.Padding(4);
                            });

                            col.Item().AlignRight().Text($"Total: {pedido.Total:C0}").Bold().FontSize(12);

                            col.Item().Text("Gracias por su compra. Este documento sirve como recibo de pago.").Italic().FontColor(Colors.Grey.Darken2);
                        });

                        page.Footer().AlignRight().Text(txt =>
                        {
                            txt.Span("CompuHiperMegaRed").FontSize(9);
                            txt.Span(" • ").FontSize(9);
                            txt.Span("www.compuhipermegared.com").FontSize(9);
                        });
                    });
                });

                var bytes = doc.GeneratePdf();
                var fileName = $"Recibo_{pedido.ReferenceCode}.pdf";
                return File(bytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando recibo PDF");
                return BadRequest("No se pudo generar el recibo");
            }
        }
    }
}