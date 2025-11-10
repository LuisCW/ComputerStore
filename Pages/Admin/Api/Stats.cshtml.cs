using ComputerStore.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ComputerStore.Pages.Admin.Api
{
    [Authorize(Roles = "Admin")]
    public class StatsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public StatsModel(ApplicationDbContext context) { _context = context; }

        public async Task<IActionResult> OnGetAsync()
        {
            var hoy = DateTime.UtcNow.Date;
            var ventasHoy = await _context.Pedidos
                .Where(p => p.FechaCreacion >= hoy && (p.Estado == "Pagado" || p.Estado == "Completado" || p.Estado == "Entregado"))
                .SumAsync(p => (decimal?)p.Total) ?? 0m;
            var pedidosHoy = await _context.Pedidos.CountAsync(p => p.FechaCreacion >= hoy);
            var enviosActivos = await _context.Envios.CountAsync(e => e.Estado != "ENTREGADO");
            var productosBajoStock = await _context.Productos.CountAsync(p => p.IsActive && p.Stock <= p.StockMinimo);

            return new JsonResult(new {
                ventasHoy = ventasHoy.ToString("N0"),
                pedidosHoy,
                enviosActivos,
                productosBajoStock
            });
        }
    }
}
