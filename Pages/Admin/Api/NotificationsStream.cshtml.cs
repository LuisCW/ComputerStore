using ComputerStore.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ComputerStore.Pages.Admin.Api
{
    [Authorize(Roles = "Admin")]
    public class NotificationsStreamModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public NotificationsStreamModel(ApplicationDbContext context) { _context = context; }

        public async Task OnGetAsync(CancellationToken cancellationToken)
        {
            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("X-Accel-Buffering", "no");

            // Track last notification id to push only when a new one is inserted
            int lastId = 0;
            try
            {
                lastId = await _context.AdminNotifications.AsNoTracking().MaxAsync(n => (int?)n.Id) ?? 0;
            }
            catch { lastId = 0; }

            // Heartbeat and new item detection loop
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var currentMax = await _context.AdminNotifications.AsNoTracking().MaxAsync(n => (int?)n.Id) ?? 0;
                    if (currentMax > lastId)
                    {
                        lastId = currentMax;
                        await Response.WriteAsync("event: new\n");
                        await Response.WriteAsync("data: 1\n\n");
                        await Response.Body.FlushAsync();
                    }
                    else
                    {
                        // heartbeat to keep connection alive
                        await Response.WriteAsync(": keepalive\n\n");
                        await Response.Body.FlushAsync();
                    }
                }
                catch
                {
                    // break on errors (client will reconnect and fall back to polling)
                    break;
                }

                try { await Task.Delay(1000, cancellationToken); } catch { break; }
            }
        }
    }
}
