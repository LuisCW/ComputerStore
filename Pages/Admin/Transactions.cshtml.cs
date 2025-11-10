using ComputerStore.Data;
using ComputerStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ComputerStore.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class TransactionsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IExcelExportService _excelExportService;
        
        public List<ComputerStore.Models.TransaccionEntity> Ultimas { get; set; } = new();
        public decimal TotalVentas { get; set; }
        public decimal CostoEstimadoReposicion { get; set; }
        public decimal GananciaNeta { get; set; }
        public double MargenPorcentaje { get; set; }
        public int TransaccionesAprobadas { get; set; }
        public int TransaccionesPendientes { get; set; }
        public int TransaccionesRechazadas { get; set; }

        public TransactionsModel(ApplicationDbContext context, IExcelExportService excelExportService)
        { 
            _context = context;
            _excelExportService = excelExportService;
        }

        public async Task OnGetAsync()
        {
            // Obtener últimas 100 transacciones
            Ultimas = await _context.Transacciones
                .AsNoTracking()
                .OrderByDescending(t => t.FechaTransaccion)
                .Take(100)
                .ToListAsync();

            // Calcular KPIs
            TransaccionesAprobadas = Ultimas.Count(t => t.Estado == "APPROVED");
            TransaccionesPendientes = Ultimas.Count(t => t.Estado == "PENDING");
            TransaccionesRechazadas = Ultimas.Count(t => t.Estado == "DECLINED" || t.Estado == "REJECTED");

            // Calcular ingresos totales (solo transacciones aprobadas)
            TotalVentas = Ultimas.Where(t => t.Estado == "APPROVED").Sum(t => t.Monto);

            // Calcular análisis de rentabilidad
            await CalcularRentabilidad();
        }

        // ?? NUEVO: Exportar transacciones a Excel
        public async Task<IActionResult> OnGetExportTransactionsAsync()
        {
            try
            {
                await LoadTransactionsData();
                
                var excelBytes = await _excelExportService.ExportTransactionsToExcelAsync(Ultimas);
                
                var fileName = $"Transacciones_ComputerStore_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al exportar transacciones: {ex.Message}";
                return RedirectToPage();
            }
        }

        // ?? NUEVO: Exportar análisis de transacciones a Excel
        public async Task<IActionResult> OnGetExportTransactionsAnalysisAsync()
        {
            try
            {
                await LoadTransactionsData();
                
                var excelBytes = await _excelExportService.ExportTransactionsAnalysisAsync(
                    Ultimas, TotalVentas, CostoEstimadoReposicion, GananciaNeta, MargenPorcentaje);
                
                var fileName = $"Analisis_Transacciones_ComputerStore_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al exportar análisis de transacciones: {ex.Message}";
                return RedirectToPage();
            }
        }

        private async Task LoadTransactionsData()
        {
            // Obtener últimas 100 transacciones
            Ultimas = await _context.Transacciones
                .AsNoTracking()
                .OrderByDescending(t => t.FechaTransaccion)
                .Take(100)
                .ToListAsync();

            // Calcular KPIs
            TransaccionesAprobadas = Ultimas.Count(t => t.Estado == "APPROVED");
            TransaccionesPendientes = Ultimas.Count(t => t.Estado == "PENDING");
            TransaccionesRechazadas = Ultimas.Count(t => t.Estado == "DECLINED" || t.Estado == "REJECTED");

            // Calcular ingresos totales (solo transacciones aprobadas)
            TotalVentas = Ultimas.Where(t => t.Estado == "APPROVED").Sum(t => t.Monto);

            // Calcular análisis de rentabilidad
            await CalcularRentabilidad();
        }

        private async Task CalcularRentabilidad()
        {
            try
            {
                // Obtener pedidos relacionados con transacciones aprobadas
                var transaccionesAprobadas = Ultimas.Where(t => t.Estado == "APPROVED").ToList();
                
                Console.WriteLine($"?? Transacciones aprobadas encontradas: {transaccionesAprobadas.Count}");
                Console.WriteLine($"?? Total ventas: {TotalVentas:C}");
                
                if (!transaccionesAprobadas.Any())
                {
                    CostoEstimadoReposicion = 0;
                    GananciaNeta = 0;
                    MargenPorcentaje = 0;
                    Console.WriteLine("?? No hay transacciones aprobadas");
                    return;
                }

                // Obtener pedidos y sus detalles para calcular costo real
                var pedidosIds = await _context.Pedidos
                    .Where(p => transaccionesAprobadas.Select(t => t.TransactionId).Contains(p.TransactionId))
                    .Select(p => p.Id)
                    .ToListAsync();

                Console.WriteLine($"?? Pedidos relacionados encontrados: {pedidosIds.Count}");

                var detallesPedidos = await _context.PedidoDetalles
                    .Where(d => pedidosIds.Contains(d.PedidoId))
                    .Include(d => d.Producto)
                    .ToListAsync();

                Console.WriteLine($"?? Detalles de pedidos encontrados: {detallesPedidos.Count}");

                decimal costoRealReposicion = 0;

                foreach (var detalle in detallesPedidos.Where(d => d.Producto != null))
                {
                    // Calcular costo de reposición:
                    // - Si el producto tiene un precio de compra específico, usarlo
                    // - Si no, asumir 60% del precio de venta como costo mayorista
                    decimal costoPorUnidad = detalle.Producto.PrecioCompra ?? (detalle.Producto.Price * 0.6m);
                    decimal costoTotalProducto = costoPorUnidad * detalle.Cantidad;
                    costoRealReposicion += costoTotalProducto;
                    
                    Console.WriteLine($"??? Producto: {detalle.Producto.Name}");
                    Console.WriteLine($"   ?? Precio venta: {detalle.Producto.Price:C}");
                    Console.WriteLine($"   ?? Precio compra: {detalle.Producto.PrecioCompra?.ToString("C") ?? "No definido"}");
                    Console.WriteLine($"   ?? Costo usado: {costoPorUnidad:C}");
                    Console.WriteLine($"   ?? Cantidad: {detalle.Cantidad}");
                    Console.WriteLine($"   ?? Costo total: {costoTotalProducto:C}");
                }

                // Si no se pudo calcular costo real, usar estimación del 60%
                CostoEstimadoReposicion = costoRealReposicion > 0 ? costoRealReposicion : TotalVentas * 0.6m;
                GananciaNeta = TotalVentas - CostoEstimadoReposicion;
                MargenPorcentaje = TotalVentas > 0 ? (double)((GananciaNeta / TotalVentas) * 100) : 0;
                
                Console.WriteLine($"?? RESUMEN RENTABILIDAD:");
                Console.WriteLine($"   ?? Total Ventas: {TotalVentas:C}");
                Console.WriteLine($"   ?? Costo Reposición: {CostoEstimadoReposicion:C}");
                Console.WriteLine($"   ?? Ganancia Neta: {GananciaNeta:C}");
                Console.WriteLine($"   ?? Margen: {MargenPorcentaje:F1}%");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error calculando rentabilidad: {ex.Message}");
                
                // En caso de error, usar estimación conservadora
                CostoEstimadoReposicion = TotalVentas * 0.7m; // 70% como costo conservador
                GananciaNeta = TotalVentas - CostoEstimadoReposicion;
                MargenPorcentaje = TotalVentas > 0 ? (double)((GananciaNeta / TotalVentas) * 100) : 0;
                
                Console.WriteLine($"?? Usando estimación conservadora:");
                Console.WriteLine($"   ?? Total Ventas: {TotalVentas:C}");
                Console.WriteLine($"   ?? Costo Estimado (70%): {CostoEstimadoReposicion:C}");
                Console.WriteLine($"   ?? Ganancia Estimada: {GananciaNeta:C}");
                Console.WriteLine($"   ?? Margen Estimado: {MargenPorcentaje:F1}%");
            }
        }
    }
}
