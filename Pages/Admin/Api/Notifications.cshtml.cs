using ComputerStore.Data;
using ComputerStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ComputerStore.Pages.Admin.Api
{
    [Authorize(Roles = "Admin")]
    [IgnoreAntiforgeryToken]
    public class NotificationsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public NotificationsModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context; _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync(int? take)
        {
            var user = await _userManager.GetUserAsync(User);
            int pageSize = take.HasValue && take.Value > 0 ? take.Value : 50;

            var recientes = await _context.AdminNotifications
                .AsNoTracking()
                .OrderByDescending(n => n.FechaCreacion)
                .Take(pageSize)
                .ToListAsync();

            HashSet<int> leidasIds = new();
            if (user != null)
            {
                var ids = await _context.AdminNotificationReads
                    .Where(r => r.UserId == user.Id)
                    .Select(r => r.NotificationId)
                    .ToListAsync();
                leidasIds = new HashSet<int>(ids);
            }

            var unread = recientes.Count(n => !leidasIds.Contains(n.Id));

            var items = recientes.Select(n => new
            {
                id = n.Id,
                mensaje = n.Mensaje,
                tipo = n.Tipo,
                fechaCreacion = n.FechaCreacion,
                leida = leidasIds.Contains(n.Id)
            }).ToList();

            return new JsonResult(new { unreadCount = unread, items });
        }

        public async Task<IActionResult> OnPostMarkReadAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return new JsonResult(new { success = false });

            var exists = await _context.AdminNotificationReads
                .AnyAsync(r => r.UserId == user.Id && r.NotificationId == id);
            if (!exists)
            {
                _context.AdminNotificationReads.Add(new AdminNotificationRead
                {
                    UserId = user.Id,
                    NotificationId = id,
                    FechaLectura = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostMarkAllReadAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return new JsonResult(new { success = false });

            var recentIds = await _context.AdminNotifications
                .OrderByDescending(n => n.FechaCreacion)
                .Take(200)
                .Select(n => n.Id)
                .ToListAsync();

            var existing = await _context.AdminNotificationReads
                .Where(r => r.UserId == user.Id && recentIds.Contains(r.NotificationId))
                .Select(r => r.NotificationId)
                .ToListAsync();

            var toAdd = recentIds.Except(existing).ToList();
            if (toAdd.Any())
            {
                foreach (var id in toAdd)
                {
                    _context.AdminNotificationReads.Add(new AdminNotificationRead
                    {
                        UserId = user.Id,
                        NotificationId = id,
                        FechaLectura = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync();
            }

            return new JsonResult(new { success = true });
        }
    }
}
