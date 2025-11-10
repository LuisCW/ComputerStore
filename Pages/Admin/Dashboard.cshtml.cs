using ComputerStore.Data;
using ComputerStore.Models;
using ComputerStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ComputerStore.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPayUService _payUService;
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            IPayUService payUService,
            ILogger<DashboardModel> logger)
        {
            _context = context;
            _userManager = userManager;
            _payUService = payUService;
            _logger = logger;
        }

        public int TotalUsuarios { get; set; }
        public int TotalProductos { get; set; }
        public int TotalPedidos { get; set; }
        public int PedidosPendientes { get; set; }
        public int PedidosCompletados { get; set; }
        public decimal VentasDelMes { get; set; }
        public decimal VentasTotales { get; set; }

        public int TransaccionesAprobadas { get; set; }
        public int TransaccionesPendientes { get; set; }
        public int TransaccionesRechazadas { get; set; }
        public decimal MontoTransaccionesAprobadas { get; set; }

        public int ProductosBajoStock { get; set; }
        public int ProductosAgotados { get; set; }
        public int ProductosInactivos { get; set; }

        public int EnviosEnTransito { get; set; }
        public int EnviosEntregados { get; set; }
        public int EnviosPendientes { get; set; }

        public int NotificacionesSinLeer { get; set; }
        public List<AdminNotification> NotificacionesRecientes { get; set; } = new();

        public List<PedidoEntity> PedidosRecientes { get; set; } = new();
        public List<TransaccionInfo> TransaccionesRecientes { get; set; } = new();
        public List<ApplicationUser> UsuariosRecientes { get; set; } = new();
        public List<EnvioInfo> EnviosRecientes { get; set; } = new();

        public async Task OnGetAsync()
        {
            try
            {
                await CargarEstadisticasGenerales();
                await CargarEstadisticasTransacciones();
                await CargarEstadisticasProductos();
                await CargarEstadisticasEnvios();
                await CargarNotificaciones();
                await CargarDatosRecientes();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el dashboard");
            }
        }

        private async Task CargarEstadisticasGenerales()
        {
            TotalUsuarios = await _context.Users.CountAsync();
            TotalProductos = await _context.Productos.CountAsync(p => p.IsActive);
            TotalPedidos = await _context.Pedidos.CountAsync();
            PedidosPendientes = await _context.Pedidos.CountAsync(p => p.Estado == "Pendiente");
            PedidosCompletados = await _context.Pedidos.CountAsync(p => 
                p.Estado == "Completado" || p.Estado == "Pagado" || p.Estado == "Entregado");

            var inicioMesUtc = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            VentasDelMes = await _context.Pedidos
                .Where(p => p.FechaCreacion >= inicioMesUtc && 
                           (p.Estado == "Completado" || p.Estado == "Pagado" || p.Estado == "Entregado"))
                .SumAsync(p => p.Total);
            VentasTotales = await _context.Pedidos
                .Where(p => p.Estado == "Completado" || p.Estado == "Pagado" || p.Estado == "Entregado")
                .SumAsync(p => p.Total);
        }

        private async Task CargarEstadisticasTransacciones()
        {
            try
            {
                TransaccionesAprobadas = await _context.Transacciones.CountAsync(t => t.Estado == "APPROVED");
                TransaccionesPendientes = await _context.Transacciones.CountAsync(t => t.Estado == "PENDING");
                TransaccionesRechazadas = await _context.Transacciones.CountAsync(t => 
                    t.Estado == "REJECTED" || t.Estado == "DECLINED");
                MontoTransaccionesAprobadas = await _context.Transacciones
                    .Where(t => t.Estado == "APPROVED")
                    .SumAsync(t => t.Monto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar estadísticas de transacciones");
            }
        }

        private async Task CargarEstadisticasProductos()
        {
            try
            {
                ProductosBajoStock = await _context.Productos
                    .CountAsync(p => p.IsActive && p.Stock <= p.StockMinimo && p.Stock > 0);
                ProductosAgotados = await _context.Productos
                    .CountAsync(p => p.IsActive && p.Stock == 0);
                ProductosInactivos = await _context.Productos
                    .CountAsync(p => !p.IsActive);
                if (ProductosAgotados > 0)
                    await AgregarNotificacion($"{ProductosAgotados} productos agotados requieren atención", "warning");
                if (ProductosBajoStock > 0)
                    await AgregarNotificacion($"{ProductosBajoStock} productos con stock bajo", "info");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar estadísticas de productos");
            }
        }

        private async Task CargarEstadisticasEnvios()
        {
            try
            {
                EnviosPendientes = await _context.Envios.CountAsync(e => 
                    e.Estado == "PREPARANDO" || e.Estado == "PENDIENTE");
                EnviosEnTransito = await _context.Envios.CountAsync(e => 
                    e.Estado == "EN_TRANSITO" || e.Estado == "RECOLECTADO" || e.Estado == "EN_RUTA_ENTREGA");
                EnviosEntregados = await _context.Envios.CountAsync(e => e.Estado == "ENTREGADO");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar estadísticas de envíos");
            }
        }

        private async Task CargarNotificaciones()
        {
            try
            {
                NotificacionesRecientes = await _context.AdminNotifications
                    .OrderByDescending(n => n.FechaCreacion)
                    .Take(10)
                    .ToListAsync();
                NotificacionesSinLeer = await _context.AdminNotifications.CountAsync(n => !n.Leida);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar notificaciones");
            }
        }

        private async Task CargarDatosRecientes()
        {
            PedidosRecientes = await _context.Pedidos
                .Include(p => p.User)
                .Include(p => p.Detalles).ThenInclude(d => d.Producto)
                .OrderByDescending(p => p.FechaCreacion)
                .Take(5)
                .ToListAsync();

            var transaccionesBD = await _context.Transacciones
                .Include(t => t.Pedido)
                .ThenInclude(p => p.User)
                .OrderByDescending(t => t.FechaTransaccion)
                .Take(10)
                .ToListAsync();
            TransaccionesRecientes = transaccionesBD.Select(t => t.ToTransaccionInfo()).ToList();

            UsuariosRecientes = await _context.Users
                .OrderByDescending(u => u.FechaRegistro)
                .Take(5)
                .ToListAsync();

            var enviosDb = await _context.Envios
                .Include(e => e.Pedido)
                .OrderByDescending(e => e.FechaCreacion)
                .Take(5)
                .ToListAsync();
            EnviosRecientes = enviosDb.Select(e => e.ToEnvioInfo()).ToList();
        }

        private async Task AgregarNotificacion(string mensaje, string tipo)
        {
            var notif = new AdminNotification
            {
                Mensaje = mensaje,
                Tipo = tipo,
                FechaCreacion = DateTime.UtcNow,
                Leida = false
            };
            _context.AdminNotifications.Add(notif);
            await _context.SaveChangesAsync();
        }

        public string GetEstadoBadgeClass(string estado) => estado.ToLower() switch
        {
            "pendiente" => "bg-warning text-dark",
            "pagado" => "bg-success",
            "completado" => "bg-success",
            "enviado" => "bg-info",
            "entregado" => "bg-primary",
            "cancelado" => "bg-danger",
            _ => "bg-secondary"
        };

        public string GetTransactionBadgeClass(string estado) => estado.ToUpper() switch
        {
            "APPROVED" => "bg-success",
            "PENDING" => "bg-warning text-dark",
            "REJECTED" or "DECLINED" => "bg-danger",
            _ => "bg-secondary"
        };

        public string GetMetodoPagoIcon(string metodoPago) => metodoPago.ToUpper() switch
        {
            "TARJETA" => "fas fa-credit-card",
            "PSE" => "fas fa-university",
            "EFECTY" => "fas fa-money-bill-wave",
            "NEQUI" => "fas fa-mobile-alt",
            _ => "fas fa-payment"
        };

        public string GetEstadoEnvioColor(string estado) => estado?.ToLower() switch
        {
            "preparando" => "text-warning",
            "recolectado" => "text-info",
            "en_transito" => "text-primary",
            "entregado" => "text-success",
            _ => "text-muted"
        };
    }
}