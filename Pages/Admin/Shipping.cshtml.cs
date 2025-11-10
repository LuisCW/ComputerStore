using ComputerStore.Models;
using ComputerStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ComputerStore.Data;
using Microsoft.EntityFrameworkCore;

namespace ComputerStore.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ShippingModel : PageModel
    {
        private readonly IPayUService _payUService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ShippingModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IExcelExportService _excelExportService;

        public List<EnvioInfo> Envios { get; set; } = new List<EnvioInfo>();
        public List<ApplicationUser> Usuarios { get; set; } = new List<ApplicationUser>();
        public string FiltroUsuario { get; set; } = string.Empty;
        public string FiltroEstado { get; set; } = string.Empty;
        public string FiltroBusqueda { get; set; } = string.Empty;
        public DateTime? FiltroFechaInicio { get; set; }
        public DateTime? FiltroFechaFin { get; set; }

        // Estadísticas
        public int TotalEnvios { get; set; }
        public int EnviosEnPreparacion { get; set; }
        public int EnviosEnTransito { get; set; }
        public int EnviosEntregados { get; set; }
        public int EnviosPendientes { get; set; }

        public ShippingModel(IPayUService payUService, UserManager<ApplicationUser> userManager, 
            ILogger<ShippingModel> logger, ApplicationDbContext context, IExcelExportService excelExportService)
        {
            _payUService = payUService;
            _userManager = userManager;
            _logger = logger;
            _context = context;
            _excelExportService = excelExportService;
        }

        public async Task OnGetAsync(string? filtroUsuario = null, string? filtroEstado = null, 
            string? filtroBusqueda = null, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                FiltroUsuario = filtroUsuario ?? string.Empty;
                FiltroEstado = filtroEstado ?? string.Empty;
                FiltroBusqueda = filtroBusqueda ?? string.Empty;
                FiltroFechaInicio = fechaInicio;
                FiltroFechaFin = fechaFin;

                // Obtener envíos desde la base de datos con su pedido y productos
                var enviosQuery = _context.Envios
                    .Include(e => e.Pedido)
                        .ThenInclude(p => p.Detalles)
                            .ThenInclude(d => d.Producto)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(FiltroUsuario))
                {
                    enviosQuery = enviosQuery.Where(e => e.Pedido.UserId == FiltroUsuario);
                }

                if (!string.IsNullOrEmpty(FiltroEstado))
                {
                    enviosQuery = enviosQuery.Where(e => e.Estado.Contains(FiltroEstado));
                }

                // CORREGIDO: Convertir fechas a UTC antes de la consulta
                if (FiltroFechaInicio.HasValue)
                {
                    var fechaInicioUtc = DateTime.SpecifyKind(FiltroFechaInicio.Value.Date, DateTimeKind.Utc);
                    enviosQuery = enviosQuery.Where(e => e.FechaCreacion.Date >= fechaInicioUtc.Date);
                }

                if (FiltroFechaFin.HasValue)
                {
                    var fechaFinUtc = DateTime.SpecifyKind(FiltroFechaFin.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                    enviosQuery = enviosQuery.Where(e => e.FechaCreacion <= fechaFinUtc);
                }

                var enviosDb = await enviosQuery.OrderByDescending(e => e.FechaCreacion).ToListAsync();
                var todosEnvios = enviosDb.Select(e => e.ToEnvioInfo()).ToList();

                if (!string.IsNullOrEmpty(FiltroBusqueda))
                {
                    todosEnvios = todosEnvios.Where(e =>
                        (e.NumeroGuia?.Contains(FiltroBusqueda, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (e.TransactionId?.Contains(FiltroBusqueda, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (e.NombreProducto?.Contains(FiltroBusqueda, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (e.DireccionEnvio?.Contains(FiltroBusqueda, StringComparison.OrdinalIgnoreCase) ?? false)
                    ).ToList();
                }

                Envios = todosEnvios;

                // Estadísticas
                TotalEnvios = Envios.Count;
                EnviosEnPreparacion = Envios.Count(e => e.Estado.Contains("PREPARANDO") || e.Estado.Contains("Proceso", StringComparison.OrdinalIgnoreCase));
                EnviosEnTransito = Envios.Count(e => e.Estado.Contains("TRANSITO") || e.Estado.Contains("ENVIADO", StringComparison.OrdinalIgnoreCase));
                EnviosEntregados = Envios.Count(e => e.Estado.Contains("ENTREGADO"));
                EnviosPendientes = Envios.Count(e => e.Estado.Contains("PENDIENTE"));

                // Obtener lista de usuarios para el filtro
                Usuarios = _userManager.Users.OrderBy(u => u.UserName).ToList();

                _logger.LogInformation("Cargados {Count} envíos para administración con filtros aplicados", Envios.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar envíos para administración");
                TempData["ErrorMessage"] = $"Error al cargar envíos: {ex.Message}";
                Envios = new List<EnvioInfo>();
            }
        }

        public async Task<IActionResult> OnPostActualizarEstadoAsync(string numeroGuia)
        {
            try
            {
                var envio = await _context.Envios
                    .AsTracking() // CRITICO: habilitar tracking para persistir cambios
                    .FirstOrDefaultAsync(e => e.NumeroGuia == numeroGuia);
                if (envio == null)
                {
                    TempData["ErrorMessage"] = "Envío no encontrado";
                    return RedirectToPage();
                }

                // Progresión de estados
                envio.Estado = _payUService.ObtenerSiguienteEstado(envio.Estado);

                var ahora = DateTime.UtcNow;
                switch (envio.Estado)
                {
                    case "RECOLECTADO":
                        envio.FechaRecoleccion = ahora; break;
                    case "EN_CENTRO_DISTRIBUCION":
                        envio.FechaLlegadaCentroDistribucion = ahora; break;
                    case "EN_TRANSITO_A_DESTINO":
                        envio.FechaSalidaCentroDistribucion = ahora; break;
                    case "EN_RUTA_ENTREGA":
                        envio.FechaEnRutaEntrega = ahora; break;
                    case "ENTREGADO":
                        envio.FechaEntrega = ahora; break;
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Estado actualizado a: {envio.Estado} para envío {numeroGuia}";
                _logger.LogInformation("Admin actualizó estado de envío {NumeroGuia} a {Estado}", numeroGuia, envio.Estado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar estado del envío {NumeroGuia}", numeroGuia);
                TempData["ErrorMessage"] = $"Error al actualizar estado: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEliminarEnvioAsync(string numeroGuia)
        {
            try
            {
                var envio = await _context.Envios
                    .AsTracking() // asegurar tracking para eliminar
                    .FirstOrDefaultAsync(e => e.NumeroGuia == numeroGuia);
                if (envio == null)
                {
                    TempData["ErrorMessage"] = "Envío no encontrado";
                    return RedirectToPage();
                }

                _context.Envios.Remove(envio);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Envío {numeroGuia} eliminado correctamente";
                _logger.LogInformation("Admin eliminó envío {NumeroGuia}", numeroGuia);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar envío {NumeroGuia}", numeroGuia);
                TempData["ErrorMessage"] = $"Error al eliminar envío: {ex.Message}";
            }

            return RedirectToPage();
        }

        // ?? EXPORTAR ENVÍOS A EXCEL
        public async Task<IActionResult> OnGetExportExcelAsync(string? filtroUsuario = null, string? filtroEstado = null, 
            string? filtroBusqueda = null, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            try
            {
                _logger.LogInformation("Iniciando exportación de envíos a Excel");

                // Aplicar los mismos filtros que en OnGetAsync
                var enviosQuery = _context.Envios
                    .Include(e => e.Pedido)
                        .ThenInclude(p => p.Detalles)
                            .ThenInclude(d => d.Producto)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(filtroUsuario))
                {
                    enviosQuery = enviosQuery.Where(e => e.Pedido.UserId == filtroUsuario);
                }

                if (!string.IsNullOrEmpty(filtroEstado))
                {
                    enviosQuery = enviosQuery.Where(e => e.Estado.Contains(filtroEstado));
                }

                // Filtros de fecha con conversión UTC
                if (fechaInicio.HasValue)
                {
                    var fechaInicioUtc = DateTime.SpecifyKind(fechaInicio.Value.Date, DateTimeKind.Utc);
                    enviosQuery = enviosQuery.Where(e => e.FechaCreacion.Date >= fechaInicioUtc.Date);
                }

                if (fechaFin.HasValue)
                {
                    var fechaFinUtc = DateTime.SpecifyKind(fechaFin.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                    enviosQuery = enviosQuery.Where(e => e.FechaCreacion <= fechaFinUtc);
                }

                var enviosDb = await enviosQuery.OrderByDescending(e => e.FechaCreacion).ToListAsync();
                var todosEnvios = enviosDb.Select(e => e.ToEnvioInfo()).ToList();

                // Filtro de búsqueda en memoria
                if (!string.IsNullOrEmpty(filtroBusqueda))
                {
                    todosEnvios = todosEnvios.Where(e =>
                        (e.NumeroGuia?.Contains(filtroBusqueda, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (e.TransactionId?.Contains(filtroBusqueda, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (e.NombreProducto?.Contains(filtroBusqueda, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (e.DireccionEnvio?.Contains(filtroBusqueda, StringComparison.OrdinalIgnoreCase) ?? false)
                    ).ToList();
                }

                _logger.LogInformation("Obtenidos {Count} envíos para exportar", todosEnvios.Count);

                var excelData = await _excelExportService.ExportShipmentsToExcelAsync(todosEnvios);

                // Detectar tipo de archivo
                var isExcel = excelData.Length > 1000 && excelData[0] == 0x50 && excelData[1] == 0x4B;
                
                // Construir nombre de archivo con filtros
                var filterSuffix = "";
                if (!string.IsNullOrEmpty(filtroUsuario)) filterSuffix += "_Usuario";
                if (!string.IsNullOrEmpty(filtroEstado)) filterSuffix += $"_{filtroEstado}";
                if (fechaInicio.HasValue) filterSuffix += $"_Desde{fechaInicio.Value:yyyyMMdd}";
                if (fechaFin.HasValue) filterSuffix += $"_Hasta{fechaFin.Value:yyyyMMdd}";
                if (!string.IsNullOrEmpty(filtroBusqueda)) filterSuffix += "_Filtrado";

                string fileName;
                string contentType;
                
                if (isExcel)
                {
                    fileName = $"Envios{filterSuffix}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    _logger.LogInformation("Enviando archivo Excel: {FileName}", fileName);
                }
                else
                {
                    fileName = $"Envios{filterSuffix}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    contentType = "text/csv";
                    _logger.LogInformation("Enviando archivo CSV alternativo: {FileName}", fileName);
                }
                
                Response.Clear();
                Response.Headers.Clear();
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                Response.Headers.Add("Content-Type", contentType);
                Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                
                return File(excelData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completo al exportar envíos a Excel");
                TempData["ErrorMessage"] = $"Error al exportar envíos: {ex.Message}";
                return RedirectToPage();
            }
        }

        // ?? EXPORTAR ANÁLISIS DE ENVÍOS (TABLA DINÁMICA)
        public async Task<IActionResult> OnGetExportAnalysisAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando exportación de análisis de envíos");
                
                var allShipments = await _context.Envios
                    .Include(e => e.Pedido)
                        .ThenInclude(p => p.Detalles)
                            .ThenInclude(d => d.Producto)
                    .OrderByDescending(e => e.FechaCreacion)
                    .ToListAsync();

                var enviosInfo = allShipments.Select(e => e.ToEnvioInfo()).ToList();

                var excelData = await _excelExportService.ExportShipmentsAnalysisAsync(enviosInfo);

                // Detectar tipo de archivo
                var isExcel = excelData.Length > 1000 && excelData[0] == 0x50 && excelData[1] == 0x4B;
                
                string fileName;
                string contentType;
                
                if (isExcel)
                {
                    fileName = $"Analisis_Envios_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                }
                else
                {
                    fileName = $"Analisis_Envios_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    contentType = "text/csv";
                }
                
                Response.Clear();
                Response.Headers.Clear();
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                Response.Headers.Add("Content-Type", contentType);
                Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                
                return File(excelData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar análisis de envíos");
                TempData["ErrorMessage"] = $"Error al exportar análisis: {ex.Message}";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnGetDetalleEnvioAsync(string numeroGuia)
        {
            try
            {
                var envioEntity = await _context.Envios
                    .Include(e => e.Pedido)
                        .ThenInclude(p => p.Detalles)
                            .ThenInclude(d => d.Producto)
                    .FirstOrDefaultAsync(e => e.NumeroGuia == numeroGuia);
                
                if (envioEntity == null)
                {
                    return new JsonResult(new { success = false, message = "Envío no encontrado" });
                }

                var envio = envioEntity.ToEnvioInfo();

                // Obtener información del usuario
                ApplicationUser? usuario = null;
                if (!string.IsNullOrEmpty(envioEntity.Pedido?.UserId))
                {
                    usuario = await _userManager.FindByIdAsync(envioEntity.Pedido.UserId);
                }

                // Generar timeline completo
                var timeline = GenerarTimelineCompleto(envio);

                return new JsonResult(new
                {
                    success = true,
                    envio = new
                    {
                        numeroGuia = envio.NumeroGuia,
                        orderId = envio.OrderId,
                        transactionId = envio.TransactionId,
                        estado = envio.Estado,
                        estadoDisplay = envio.EstadoDisplay,
                        colorEstado = envio.ColorEstado,
                        nombreProducto = envio.NombreProducto,
                        precioTotal = envio.PrecioTotal,
                        fechaCreacion = envio.FechaCreacion.ToString("dd/MM/yyyy HH:mm"),
                        fechaEstimadaEntrega = envio.FechaEstimadaEntrega.ToString("dd/MM/yyyy"),
                        direccionEnvio = envio.DireccionEnvio,
                        transportadora = envio.Transportadora,
                        pesoKg = envio.PesoKg,
                        observaciones = envio.Observaciones,
                        usuario = usuario != null ? new
                        {
                            id = usuario.Id,
                            userName = usuario.UserName,
                            email = usuario.Email,
                            telefono = usuario.PhoneNumber,
                            direccion = usuario.Direccion,
                            ciudad = usuario.Ciudad,
                            departamento = usuario.Departamento
                        } : null,
                        productos = envio.Productos?.Select(p => new
                        {
                            nombre = p.Nombre,
                            precio = p.Precio,
                            cantidad = p.Cantidad,
                            subTotal = p.SubTotal,
                            precioFormateado = p.PrecioFormateado,
                            subTotalFormateado = p.SubTotalFormateado
                        }).Cast<object>().ToList() ?? new List<object>(),
                        timeline = timeline
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo detalles del envío {NumeroGuia}", numeroGuia);
                return new JsonResult(new { success = false, message = "Error al obtener detalles del envío" });
            }
        }

        private List<object> GenerarTimelineCompleto(EnvioInfo envio)
        {
            var timeline = new List<object>();

            var estadosPosibles = new[]
            {
                new { Codigo = "PREPARANDO", Nombre = "Paquete preparado", Icono = "fas fa-box", Descripcion = "Su pedido ha sido empacado y está listo para ser recogido por la transportadora" },
                new { Codigo = "RECOLECTADO", Nombre = "Recogido por transportadora", Icono = "fas fa-truck-pickup", Descripcion = "El paquete ha sido recogido y registrado en el sistema de la transportadora" },
                new { Codigo = "EN_CENTRO_DISTRIBUCION", Nombre = "En centro de distribución", Icono = "fas fa-warehouse", Descripcion = "El paquete está siendo procesado y clasificado para su envío" },
                new { Codigo = "EN_TRANSITO_A_DESTINO", Nombre = "En tránsito hacia destino", Icono = "fas fa-shipping-fast", Descripcion = "El paquete viaja hacia su ciudad de destino" },
                new { Codigo = "LLEGADA_CIUDAD_DESTINO", Nombre = "Llegada a ciudad destino", Icono = "fas fa-map-marker-alt", Descripcion = "El paquete ha llegado a su ciudad y está siendo preparado para entrega" },
                new { Codigo = "EN_RUTA_ENTREGA", Nombre = "En ruta de entrega", Icono = "fas fa-route", Descripcion = "El repartidor está en camino a su dirección registrada" },
                new { Codigo = "ENTREGADO", Nombre = "Entregado", Icono = "fas fa-check-circle", Descripcion = "Paquete entregado exitosamente" }
            };

            var estadoActualIndex = Array.FindIndex(estadosPosibles, e => e.Codigo == envio.Estado);
            if (estadoActualIndex == -1) estadoActualIndex = 0;

            for (int i = 0; i < estadosPosibles.Length; i++)
            {
                var estado = estadosPosibles[i];
                var esCompletado = i <= estadoActualIndex;
                var esActual = i == estadoActualIndex;
                var esPendiente = i > estadoActualIndex;

                DateTime? fechaEstado = null;
                if (esCompletado)
                {
                    fechaEstado = estado.Codigo switch
                    {
                        "PREPARANDO" => envio.FechaCreacion,
                        "RECOLECTADO" => envio.FechaRecoleccion ?? envio.FechaCreacion.AddHours(6),
                        "EN_CENTRO_DISTRIBUCION" => envio.FechaLlegadaCentroDistribucion ?? envio.FechaCreacion.AddDays(1),
                        "EN_TRANSITO_A_DESTINO" => envio.FechaSalidaCentroDistribucion ?? envio.FechaCreacion.AddDays(1.5),
                        "LLEGADA_CIUDAD_DESTINO" => envio.FechaLlegadaCiudadDestino ?? envio.FechaCreacion.AddDays(2),
                        "EN_RUTA_ENTREGA" => envio.FechaEnRutaEntrega ?? envio.FechaCreacion.AddDays(2.5),
                        "ENTREGADO" => envio.FechaEntrega,
                        _ => null
                    };
                }

                timeline.Add(new
                {
                    codigo = estado.Codigo,
                    nombre = estado.Nombre,
                    icono = estado.Icono,
                    descripcion = estado.Descripcion,
                    fecha = fechaEstado?.ToString("dd/MM/yyyy HH:mm") ?? (esPendiente ? "Pendiente" : "Procesando"),
                    esCompletado = esCompletado,
                    esActual = esActual,
                    esPendiente = esPendiente,
                    ubicacion = estado.Codigo switch
                    {
                        "PREPARANDO" => "Centro de distribución CompuHiperMegaRed - Bogotá",
                        "RECOLECTADO" => "Servientrega - Country 2, Cl. 79 #15-22, Bogotá",
                        "EN_CENTRO_DISTRIBUCION" => "Hub de distribución regional - Bogotá",
                        "EN_TRANSITO_A_DESTINO" => "Transporte terrestre - Ruta Bogotá-Destino",
                        "LLEGADA_CIUDAD_DESTINO" => "Centro de distribución local",
                        "EN_RUTA_ENTREGA" => "Vehículo de entrega - Repartidor asignado",
                        "ENTREGADO" => envio.DireccionEnvio,
                        _ => "En proceso"
                    }
                });
            }

            return timeline;
        }
    }
}