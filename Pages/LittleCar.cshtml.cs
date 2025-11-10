using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Globalization;
using ComputerStore.Models;
using ComputerStore.Services;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace ComputerStore.Pages
{
    // [Authorize] // Se quita para permitir ver el carrito sin iniciar sesión
    public class LittleCarModel : PageModel
    {
        private readonly LittleCarService _carritoService;
        private readonly IPayUService _payUService;
        private readonly ILogger<LittleCarModel> _logger;

        public List<Products> ProductosCarrito { get; set; }
        public decimal Total { get; set; }

        [BindProperty]
        public string MetodoPago { get; set; }

        // Propiedades para Tarjeta de Crédito
        [BindProperty]
        public string NumeroTarjeta { get; set; }
        [BindProperty]
        public string CVV { get; set; }
        [BindProperty]
        public string ExpiracionMes { get; set; }
        [BindProperty]
        public string ExpiracionAnio { get; set; }
        [BindProperty]
        public string TipoTarjeta { get; set; }

        // Propiedades para PSE
        [BindProperty]
        public string DocumentoPSE { get; set; }
        [BindProperty]
        public string BancoPSE { get; set; }

        // Propiedades para Efecty
        [BindProperty]
        public string DocumentoEfecty { get; set; }

        // Propiedades para Nequi
        [BindProperty]
        public string CelularNequi { get; set; }

        public LittleCarModel(LittleCarService carritoService, IPayUService payUService, ILogger<LittleCarModel> logger)
        {
            _carritoService = carritoService;
            _payUService = payUService;
            _logger = logger;
        }

        public void OnGet()
        {
            try
            {
                _logger.LogInformation("=== INICIANDO CARGA DE CARRITO ===");
                
                // Cargar carrito desde sesión (funciona con o sin sesión iniciada)
                ProductosCarrito = _carritoService.ObtenerCarrito();
                Total = _carritoService.ObtenerTotal();
                
                _logger.LogInformation("Carrito cargado: {Count} productos, Total: {Total:C}", 
                    ProductosCarrito?.Count ?? 0, Total);
                
                // Log detallado de cada producto
                if (ProductosCarrito?.Any() == true)
                {
                    foreach (var producto in ProductosCarrito)
                    {
                        _logger.LogInformation("Producto en carrito: ID={Id}, Nombre={Name}, Precio={Price:C}", 
                            producto.Id, producto.Name, producto.Price);
                    }
                }
                else
                {
                    _logger.LogWarning("No hay productos en el carrito");
                }
                
                // Validar integridad del carrito
                if (!_carritoService.ValidarCarrito())
                {
                    _logger.LogWarning("Carrito inválido detectado, recargando...");
                    ProductosCarrito = _carritoService.ObtenerCarrito();
                    Total = _carritoService.ObtenerTotal();
                }
                
                _logger.LogInformation("=== CARGA DE CARRITO COMPLETADA ===");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error crítico al cargar el carrito");
                ProductosCarrito = new List<Products>();
                Total = 0;
            }
        }

        public JsonResult OnPostEliminarProducto(int id)
        {
            // Permitir eliminar con o sin autenticación, ya que el carrito es por sesión
            int nuevoConteo = _carritoService.EliminarDelCarrito(id);
            return new JsonResult(new { success = true, cartCount = nuevoConteo });
        }

        public JsonResult OnPostVaciarCarrito()
        {
            // Permitir vaciar con o sin autenticación
            _carritoService.VaciarCarrito();
            return new JsonResult(new { success = true, cartCount = 0 });
        }

        public async Task<IActionResult> OnPostRealizarPago()
        {
            try
            {
                // Requerir inicio de sesión para proceder al pago
                if (!User.Identity.IsAuthenticated)
                {
                    var returnUrl = Url.Page("/LittleCar");
                    return Redirect($"/Identity/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl)}");
                }

                var total = _carritoService.ObtenerTotal();
                var pedido = new Pedido
                {
                    ReferenceCode = Guid.NewGuid().ToString(),
                    Description = "Compra de productos",
                    Amount = total,
                    BuyerName = "Nombre del Cliente",
                    BuyerEmail = "email@ejemplo.com"
                };

                // Obtener la respuesta JSON de PayU
                string respuestaPayU = await ProcesarPago(pedido, total);
                _logger.LogInformation($"Respuesta PayU original: {respuestaPayU}");

                if (string.IsNullOrEmpty(respuestaPayU))
                {
                    ModelState.AddModelError(string.Empty, "Error al procesar el pago.");
                    return Page();
                }

                // Handle specific payment method redirects
                if (respuestaPayU.StartsWith("https://") || respuestaPayU.StartsWith("/"))
                {
                    return Redirect(respuestaPayU);
                }

                try
                {
                    var payUResponse = JsonSerializer.Deserialize<PayUResponse>(respuestaPayU);

                    if (payUResponse?.TransactionResponse == null)
                    {
                        _logger.LogError("Respuesta de PayU no contiene TransactionResponse");
                        ModelState.AddModelError(string.Empty, "Error al procesar el pago.");
                        return Page();
                    }

                    // Construir los parámetros de redirección
                    var redirectParams = new
                    {
                        transactionState = payUResponse.TransactionResponse.State,
                        referenceCode = payUResponse.TransactionResponse.TrazabilityCode,
                        message = payUResponse.TransactionResponse.PaymentNetworkResponseErrorMessage
             ?? payUResponse.TransactionResponse.ResponseMessage
             ?? "Sin mensaje disponible",
                        orderId = payUResponse.TransactionResponse.OrderId.ToString(),
                        transactionId = payUResponse.TransactionResponse.TransactionId,
                        paymentMethod = MetodoPago,
                        bankUrl = payUResponse.TransactionResponse.ExtraParameters?.ContainsKey("BANK_URL") == true
        ? payUResponse.TransactionResponse.ExtraParameters["BANK_URL"]?.ToString()
        : null
                    };

                    // Manejar redirección para métodos de pago específicos
                    switch (MetodoPago)
                    {
                        case "PSE" when payUResponse.TransactionResponse.State.Equals("PENDING", StringComparison.OrdinalIgnoreCase):
                            return redirectParams.bankUrl != null
                                ? Redirect(redirectParams.bankUrl)
                                : RedirectToPage("/ConfirmacionPago", redirectParams);

                        case "NEQUI" when payUResponse.TransactionResponse.State.Equals("PENDING", StringComparison.OrdinalIgnoreCase):
                            // Agregar lógica específica para Nequi si es necesario
                            return RedirectToPage("/ConfirmacionPago", redirectParams);

                        case "EFECTY" when payUResponse.TransactionResponse.State.Equals("PENDING", StringComparison.OrdinalIgnoreCase):
                            // Agregar lógica específica para Efecty si es necesario
                            // Puede incluir la generación de código de pago o redirección
                            return RedirectToPage("/ConfirmacionPago", redirectParams);
                    }

                    // Si el pago fue aprobado, vaciar el carrito
                    if (payUResponse.TransactionResponse.State.Equals("APPROVED", StringComparison.OrdinalIgnoreCase))
                    {
                        _carritoService.VaciarCarrito();
                    }

                    // Redireccionar a la página de confirmación
                    return RedirectToPage("/ConfirmacionPago", redirectParams);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error al parsear respuesta de PayU: {Response}", respuestaPayU);
                    ModelState.AddModelError(string.Empty, "Error al procesar la respuesta del pago.");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar el pago");
                ModelState.AddModelError(string.Empty, "Error inesperado al procesar el pago.");
                return Page();
            }
        }

        private async Task<string> ProcesarPago(Pedido pedido, decimal total)
        {
            return MetodoPago?.ToUpper() switch
            {
                "TARJETA" => await _payUService.RealizarPago(pedido, MetodoPago, NumeroTarjeta, CVV, ExpiracionMes, ExpiracionAnio, total, TipoTarjeta),
                "PSE" => await _payUService.RealizarPago(pedido, MetodoPago, DocumentoPSE, null, null, null, total, BancoPSE),
                "EFECTY" => await _payUService.RealizarPago(pedido, MetodoPago, DocumentoEfecty, null, null, null, total, null),
                "NEQUI" => await _payUService.RealizarPago(pedido, MetodoPago, CelularNequi, null, null, null, total, null),
                _ => string.Empty
            };
        }

        private class RespuestaPago
        {
            public string Estado { get; set; }
            public string ReferenceCode { get; set; }
            public string Message { get; set; }
            public string OrderId { get; set; }
            public string TransactionId { get; set; }
        }

        private async Task<RespuestaPago> ParsearRespuestaPago(string respuestaPayU)
        {
            try
            {
                // Deserializar la respuesta JSON de PayU
                var respuesta = JsonSerializer.Deserialize<PayUResponse>(respuestaPayU);

                if (respuesta?.TransactionResponse == null)
                {
                    _logger.LogError("Respuesta de PayU inválida o nula");
                    throw new Exception("Respuesta de PayU inválida");
                }

                return new RespuestaPago
                {
                    Estado = respuesta.TransactionResponse.State,
                    ReferenceCode = respuesta.TransactionResponse.TrazabilityCode,
                    Message = respuesta.TransactionResponse.ResponseMessage ??
                             respuesta.TransactionResponse.PaymentNetworkResponseErrorMessage ??
                             "Sin mensaje disponible",
                    OrderId = respuesta.TransactionResponse.OrderId.ToString(),
                    TransactionId = respuesta.TransactionResponse.TransactionId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parseando respuesta de PayU");
                throw;
            }
        }

        // Clases para deserializar la respuesta de PayU
        public class PayUResponse
        {
            [JsonPropertyName("code")]
            public string Code { get; set; }

            [JsonPropertyName("error")]
            public string Error { get; set; }

            [JsonPropertyName("transactionResponse")]
            public TransactionResponse TransactionResponse { get; set; }
        }

        public class TransactionResponse
        {
            [JsonPropertyName("state")]
            public string State { get; set; }

            [JsonPropertyName("paymentNetworkResponseErrorMessage")]
            public string PaymentNetworkResponseErrorMessage { get; set; }

            [JsonPropertyName("trazabilityCode")]
            public string TrazabilityCode { get; set; }

            [JsonPropertyName("orderId")]
            public long OrderId { get; set; }

            [JsonPropertyName("transactionId")]
            public string TransactionId { get; set; }

            [JsonPropertyName("responseMessage")]
            public string ResponseMessage { get; set; }

            [JsonPropertyName("responseCode")]
            public string ResponseCode { get; set; }

            [JsonPropertyName("extraParameters")]
            public Dictionary<string, object> ExtraParameters { get; set; }

            [JsonPropertyName("additionalInfo")]
            public Dictionary<string, object> AdditionalInfo { get; set; }
        }
    }
}