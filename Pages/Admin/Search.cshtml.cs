using ComputerStore.Data;
using ComputerStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ComputerStore.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class SearchModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public SearchModel(ApplicationDbContext context) { _context = context; }

        public string Query { get; set; } = string.Empty;
        public List<ProductEntity> Products { get; set; } = new();
        public List<PedidoEntity> Orders { get; set; } = new();
        public List<ApplicationUser> Users { get; set; } = new();
        public List<EnvioEntity> Shippings { get; set; } = new();

        public async Task OnGetAsync(string? q)
        {
            Query = q?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(Query)) return;

            var like = Query.ToLower();
            Products = await _context.Productos
                .Where(p => p.Name.ToLower().Contains(like) ||
                            (p.SKU != null && p.SKU.ToLower().Contains(like)) ||
                            p.Brand.ToLower().Contains(like) ||
                            (p.Description != null && p.Description.ToLower().Contains(like)))
                .OrderByDescending(p => p.CreatedDate)
                .Take(20)
                .ToListAsync();

            Orders = await _context.Pedidos
                .Where(o => o.ReferenceCode.ToLower().Contains(like) ||
                            o.UserId.ToLower().Contains(like) ||
                            (o.TransactionId != null && o.TransactionId.ToLower().Contains(like)))
                .OrderByDescending(o => o.FechaCreacion)
                .Take(20)
                .ToListAsync();

            Users = await _context.Users
                .Where(u => u.UserName!.ToLower().Contains(like) ||
                            (u.Email != null && u.Email.ToLower().Contains(like)))
                .OrderByDescending(u => u.FechaRegistro)
                .Take(20)
                .ToListAsync();

            Shippings = await _context.Envios
                .Where(s => s.NumeroGuia.ToLower().Contains(like) ||
                            s.Estado.ToLower().Contains(like) ||
                            s.DireccionEnvio.ToLower().Contains(like))
                .OrderByDescending(s => s.FechaCreacion)
                .Take(20)
                .ToListAsync();
        }
    }
}
