using ComputerStore.Data;
using ComputerStore.Models;
using ComputerStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ComputerStore.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class OrdersModel : PageModel
    {
        private readonly IPayUService _payUService;
        private readonly ApplicationDbContext _context;
        private readonly IExcelExportService _excelExportService;
        private readonly ILogger<OrdersModel> _logger;
        
        public List<TransaccionInfo> Pedidos { get; set; } = new List<TransaccionInfo>();
        public string SearchTerm { get; set; } = string.Empty;
        public string FilterStatus { get; set; } = string.Empty;
        public DateTime? FilterDateFrom { get; set; }
        public DateTime? FilterDateTo { get; set; }
        public decimal TotalVentas { get; set; }
        public int TotalPedidos { get; set; }
        public int PedidosAprobados { get; set; }
        public int PedidosRechazados { get; set; }
        public int PedidosPendientes { get; set; }

        [BindProperty]
        public long SelectedOrderId { get; set; }

        [BindProperty]
        public string NewStatus { get; set; } = string.Empty;

        public OrdersModel(IPayUService payUService, ApplicationDbContext context, 
            IExcelExportService excelExportService, ILogger<OrdersModel> logger)
        {
            _payUService = payUService;
            _context = context;
            _excelExportService = excelExportService;
            _logger = logger;
        }

        public async Task OnGetAsync(string search = "", string status = "", 
            DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            SearchTerm = search;
            FilterStatus = status;
            FilterDateFrom = dateFrom;
            FilterDateTo = dateTo;
            await LoadOrdersAsync();
        }

        public async Task<IActionResult> OnPostUpdateStatus()
        {
            try
            {
                _logger.LogInformation("Actualizando estado de pedido: {OrderId} a {NewStatus}", SelectedOrderId, NewStatus);

                var pedido = await _context.Pedidos
                    .Include(p => p.Detalles)
                    .ThenInclude(d => d.Producto)
                    .Include(p => p.Envio)
                    .FirstOrDefaultAsync(p => p.Id == SelectedOrderId);

                if (pedido == null)
                {
                    TempData["ErrorMessage"] = $"No se encontró el pedido #{SelectedOrderId}";
                    _logger.LogWarning("Pedido no encontrado: {OrderId}", SelectedOrderId);
                    return RedirectToPage();
                }

                var estadoAnterior = pedido.Estado;
                pedido.Estado = NewStatus switch
                {
                    "APPROVED" or "Pagado" => "Pagado",
                    "REJECTED" or "DECLINED" or "Cancelado" => "Cancelado",
                    "PENDING" or "Pendiente" => "Pendiente",
                    "EN_PREPARACION" or "Preparando" or "En Preparación" => "En Preparación",
                    "ENVIADO" or "Enviado" => "Enviado",
                    "ENTREGADO" or "Entregado" => "Entregado",
                    _ => NewStatus
                };

                // Devolución de stock
                if (estadoAnterior == "Pagado" && pedido.Estado == "Cancelado" && pedido.Detalles != null)
                {
                    foreach (var d in pedido.Detalles)
                        if (d.Producto != null) d.Producto.Stock += d.Cantidad;
                }

                // Envío
                var envio = pedido.Envio ?? await _context.Envios.FirstOrDefaultAsync(e => e.PedidoId == pedido.Id);
                if (envio != null)
                {
                    switch (pedido.Estado)
                    {
                        case "Pagado":
                        case "En Preparación":
                            envio.Estado = "PREPARANDO";
                            envio.FechaRecoleccion = null;
                            envio.FechaEntrega = null;
                            break;
                        case "Enviado":
                            envio.Estado = "EN_TRANSITO_A_DESTINO";
                            envio.FechaSalidaCentroDistribucion ??= DateTime.UtcNow;
                            break;
                        case "Entregado":
                            envio.Estado = "ENTREGADO";
                            envio.FechaEntrega ??= DateTime.UtcNow;
                            break;
                        case "Cancelado":
                            envio.Estado = "FALLIDO";
                            break;
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Estado del pedido #{SelectedOrderId} actualizado de '{estadoAnterior}' a '{pedido.Estado}'";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar pedido {OrderId}", SelectedOrderId);
                TempData["ErrorMessage"] = $"Error al actualizar pedido: {ex.Message}";
                return RedirectToPage();
            }
        }

        // Nuevo m?todo para marcar como entregado
        public async Task<IActionResult> OnPostMarkAsDelivered(long orderId)
        {
            try
            {
                var pedido = await _context.Pedidos
                    .Include(p => p.Envio)
                    .FirstOrDefaultAsync(p => p.Id == orderId);
                
                if (pedido != null)
                {
                    pedido.Estado = "Entregado";
                        
                    if (pedido.Envio != null)
                    {
                        pedido.Envio.Estado = "ENTREGADO";
                        pedido.Envio.FechaEntrega = DateTime.UtcNow;
                    }
                    
                    await _context.SaveChangesAsync();
                        
                    TempData["SuccessMessage"] = $"Pedido #{orderId} marcado como entregado";
                    _logger.LogInformation("Pedido marcado como entregado: {OrderId}", orderId);
                }
                else
                {
                    TempData["ErrorMessage"] = $"No se encontr? el pedido #{orderId}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al marcar como entregado pedido {OrderId}", orderId);
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
            }
        
            return RedirectToPage();
        }

        // ?? EXPORTAR PEDIDOS A EXCEL
        public async Task<IActionResult> OnGetExportExcelAsync(string? search = null, string? status = null, 
            DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            try
            {
                _logger.LogInformation("Iniciando exportación de pedidos a Excel");

                // Aplicar los mismos filtros que en OnGetAsync
                var query = _context.Transacciones
                    .Include(t => t.Pedido)
                    .ThenInclude(p => p.User)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(o => o.TransactionId.Contains(search) ||
                                             (o.PedidoId != null && o.PedidoId.ToString()!.Contains(search)) ||
                                             (o.Pedido != null && o.Pedido.User != null && 
                                              (o.Pedido.User.UserName!.Contains(search) || 
                                               o.Pedido.User.Email!.Contains(search))));
                }

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(o => o.Estado == status);
                }

                if (dateFrom.HasValue)
                {
                    var fechaInicioUtc = DateTime.SpecifyKind(dateFrom.Value.Date, DateTimeKind.Utc);
                    query = query.Where(o => o.FechaTransaccion.Date >= fechaInicioUtc.Date);
                }

                if (dateTo.HasValue)
                {
                    var fechaFinUtc = DateTime.SpecifyKind(dateTo.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                    query = query.Where(o => o.FechaTransaccion <= fechaFinUtc);
                }

                var transacciones = await query
                    .OrderByDescending(o => o.FechaTransaccion)
                    .ToListAsync();

                var pedidosInfo = transacciones.Select(t => t.ToTransaccionInfo()).ToList();

                _logger.LogInformation("Obtenidos {Count} pedidos para exportar", pedidosInfo.Count);

                var excelData = await _excelExportService.ExportOrdersToExcelAsync(pedidosInfo);

                // Detectar tipo de archivo
                var isExcel = excelData.Length > 1000 && excelData[0] == 0x50 && excelData[1] == 0x4B;

                // Construir nombre de archivo con filtros
                var filterSuffix = "";
                if (!string.IsNullOrEmpty(search)) filterSuffix += "_Filtrado";
                if (!string.IsNullOrEmpty(status)) filterSuffix += $"_{status}";
                if (dateFrom.HasValue) filterSuffix += $"_Desde{dateFrom.Value:yyyyMMdd}";
                if (dateTo.HasValue) filterSuffix += $"_Hasta{dateTo.Value:yyyyMMdd}";

                string fileName;
                string contentType;

                if (isExcel)
                {
                    fileName = $"Pedidos{filterSuffix}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    _logger.LogInformation("Enviando archivo Excel: {FileName}", fileName);
                }
                else
                {
                    fileName = $"Pedidos{filterSuffix}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
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
                _logger.LogError(ex, "Error completo al exportar pedidos a Excel");
                TempData["ErrorMessage"] = $"Error al exportar pedidos: {ex.Message}";
                return RedirectToPage();
            }
        }

        // ?? EXPORTAR ANÁLISIS DE PEDIDOS (TABLA DINÁMICA)
        public async Task<IActionResult> OnGetExportAnalysisAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando exportación de análisis de pedidos");

                var allTransactions = await _context.Transacciones
                    .Include(t => t.Pedido)
                    .ThenInclude(p => p.User)
                    .OrderByDescending(t => t.FechaTransaccion)
                    .ToListAsync();

                var pedidosInfo = allTransactions.Select(t => t.ToTransaccionInfo()).ToList();

                var excelData = await _excelExportService.ExportOrdersAnalysisAsync(pedidosInfo);

                // Detectar tipo de archivo
                var isExcel = excelData.Length > 1000 && excelData[0] == 0x50 && excelData[1] == 0x4B;

                string fileName;
                string contentType;

                if (isExcel)
                {
                    fileName = $"Analisis_Pedidos_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                }
                else
                {
                    fileName = $"Analisis_Pedidos_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
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
                _logger.LogError(ex, "Error al exportar análisis de pedidos");
                TempData["ErrorMessage"] = $"Error al exportar análisis: {ex.Message}";
                return RedirectToPage();
            }
        }

        private async Task LoadOrdersAsync()
        {
            try
            {
                // Cargar transacciones desde la BD para persistencia real
                var query = _context.Transacciones
                    .Include(t => t.Pedido)
                    .ThenInclude(p => p.User)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    query = query.Where(o => o.TransactionId.Contains(SearchTerm) ||
                                             (o.PedidoId != null && o.PedidoId.ToString()!.Contains(SearchTerm)) ||
                                             (o.Pedido != null && o.Pedido.User != null && 
                                              (o.Pedido.User.UserName!.Contains(SearchTerm) || 
                                               o.Pedido.User.Email!.Contains(SearchTerm))));
                }

                if (!string.IsNullOrEmpty(FilterStatus))
                {
                    query = query.Where(o => o.Estado == FilterStatus);
                }

                // Filtros de fecha con conversión UTC
                if (FilterDateFrom.HasValue)
                {
                    var fechaInicioUtc = DateTime.SpecifyKind(FilterDateFrom.Value.Date, DateTimeKind.Utc);
                    query = query.Where(o => o.FechaTransaccion.Date >= fechaInicioUtc.Date);
                }

                if (FilterDateTo.HasValue)
                {
                    var fechaFinUtc = DateTime.SpecifyKind(FilterDateTo.Value.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
                    query = query.Where(o => o.FechaTransaccion <= fechaFinUtc);
                }

                var list = await query
                    .OrderByDescending(o => o.FechaTransaccion)
                    .ToListAsync();

                Pedidos = list.Select(t => t.ToTransaccionInfo()).ToList();
                TotalPedidos = Pedidos.Count;
                TotalVentas = Pedidos.Where(p => p.Estado == "APPROVED").Sum(p => p.Monto);
                PedidosAprobados = Pedidos.Count(p => p.Estado == "APPROVED");
                PedidosRechazados = Pedidos.Count(p => p.Estado == "DECLINED" || p.Estado == "REJECTED");
                PedidosPendientes = Pedidos.Count(p => p.Estado == "PENDING");

                _logger.LogInformation("Cargados {Count} pedidos con filtros aplicados", Pedidos.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar pedidos");
                TempData["ErrorMessage"] = $"Error al cargar pedidos: {ex.Message}";
                Pedidos = new List<TransaccionInfo>();
            }
        }
    }
}