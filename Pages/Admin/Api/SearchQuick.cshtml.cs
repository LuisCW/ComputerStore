using ComputerStore.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ComputerStore.Pages.Admin.Api
{
    [Authorize(Roles = "Admin")]
    public class SearchQuickModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public SearchQuickModel(ApplicationDbContext context) { _context = context; }

        public async Task<IActionResult> OnGetAsync(string? q)
        {
            var query = (q ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(query))
            {
                return new JsonResult(new { items = Array.Empty<object>() });
            }
            var like = query.ToLower();

            var productsTask = _context.Productos.AsNoTracking()
                .Where(p => p.Name.ToLower().Contains(like) ||
                            (p.SKU != null && p.SKU.ToLower().Contains(like)) ||
                            p.Brand.ToLower().Contains(like))
                .OrderByDescending(p => p.CreatedDate)
                .Take(5)
                .Select(p => new {
                    type = "Producto",
                    id = p.Id,
                    label = p.Name,
                    sub = $"SKU: {p.SKU ?? "N/A"} · {p.Brand}",
                    url = $"/Admin/EditProduct?id={p.Id}"
                })
                .ToListAsync();

            var ordersTask = _context.Pedidos.AsNoTracking()
                .Where(o => o.ReferenceCode.ToLower().Contains(like) ||
                            o.UserId.ToLower().Contains(like) ||
                            (o.TransactionId != null && o.TransactionId.ToLower().Contains(like)))
                .OrderByDescending(o => o.FechaCreacion)
                .Take(5)
                .Select(o => new {
                    type = "Pedido",
                    id = o.Id,
                    label = o.ReferenceCode,
                    sub = $"Usuario: {o.UserId} · Estado: {o.Estado}",
                    url = $"/Admin/Orders?search={o.ReferenceCode}"
                })
                .ToListAsync();

            var usersTask = _context.Users.AsNoTracking()
                .Where(u => u.UserName!.ToLower().Contains(like) ||
                            (u.Email != null && u.Email.ToLower().Contains(like)))
                .OrderByDescending(u => u.FechaRegistro)
                .Take(5)
                .Select(u => new {
                    type = "Usuario",
                    id = u.Id,
                    label = u.UserName,
                    sub = u.Email,
                    url = $"/Admin/Users?search={u.UserName}"
                })
                .ToListAsync();

            var shippingsTask = _context.Envios.AsNoTracking()
                .Where(s => s.NumeroGuia.ToLower().Contains(like) || s.Estado.ToLower().Contains(like))
                .OrderByDescending(s => s.FechaCreacion)
                .Take(5)
                .Select(s => new {
                    type = "Envío",
                    id = s.Id,
                    label = s.NumeroGuia,
                    sub = s.Estado,
                    url = $"/Admin/Shipping?search={s.NumeroGuia}"
                })
                .ToListAsync();

            await Task.WhenAll(productsTask, ordersTask, usersTask, shippingsTask);
            var items = productsTask.Result.Concat<object>(ordersTask.Result).Concat(usersTask.Result).Concat(shippingsTask.Result).ToList();
            return new JsonResult(new { items });
        }
    }
}
