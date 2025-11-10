using ComputerStore.Data;
using ComputerStore.Models;
using ComputerStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ComputerStore.Pages.Account
{
    [Authorize]
    public class OrdersModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<OrdersModel> _logger;
        private readonly IPayUService _payUService; // Conservamos referencia si es necesaria

        public OrdersModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ILogger<OrdersModel> logger,
            IPayUService payUService)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
            _payUService = payUService;
        }

        public List<PedidoEntity> Pedidos { get; set; } = new();
        public List<TransaccionInfo> Transacciones { get; set; } = new();
        public List<EnvioInfo> Envios { get; set; } = new();
        public ApplicationUser? CurrentUser { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                CurrentUser = await _userManager.GetUserAsync(User);
                if (CurrentUser == null)
                {
                    _logger.LogWarning("Usuario no autenticado en Orders");
                    return;
                }

                // Pedidos del usuario desde BD
                Pedidos = await _context.Pedidos
                    .Include(p => p.Detalles)
                        .ThenInclude(d => d.Producto)
                    .Include(p => p.Envio)
                    .Where(p => p.UserId == CurrentUser.Id)
                    .OrderByDescending(p => p.FechaCreacion)
                    .ToListAsync();

                // Transacciones del usuario desde BD (no desde cache en memoria)
                var transaccionesDb = await _context.Transacciones
                    .Include(t => t.Pedido)
                    .Where(t => t.Pedido != null && t.Pedido.UserId == CurrentUser.Id)
                    .OrderByDescending(t => t.FechaTransaccion)
                    .ToListAsync();
                Transacciones = transaccionesDb.Select(t => t.ToTransaccionInfo()).ToList();

                // Envios del usuario desde BD
                var enviosDb = await _context.Envios
                    .Include(e => e.Pedido)
                        .ThenInclude(p => p.Detalles)
                            .ThenInclude(d => d.Producto)
                    .Where(e => e.Pedido.UserId == CurrentUser.Id)
                    .OrderByDescending(e => e.FechaCreacion)
                    .ToListAsync();
                Envios = enviosDb.Select(e => e.ToEnvioInfo()).ToList();

                _logger.LogInformation("Cargados {PedidosCount} pedidos, {TransaccionesCount} transacciones y {EnviosCount} envíos para el usuario {UserId}",
                    Pedidos.Count, Transacciones.Count, Envios.Count, CurrentUser.Id);

                // Sincronizar: si hay transacción APPROVED y pedido Pendiente, actualizar
                await SincronizarTransaccionesConPedidos();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar pedidos del usuario");
                Pedidos = new List<PedidoEntity>();
                Transacciones = new List<TransaccionInfo>();
                Envios = new List<EnvioInfo>();
            }
        }

        // Sincronizar transacciones con pedidos del usuario
        private async Task SincronizarTransaccionesConPedidos()
        {
            try
            {
                // Tomar transacciones aprobadas del usuario
                var txAprobadas = await _context.Transacciones
                    .Include(t => t.Pedido)
                    .Where(t => t.Pedido != null && t.Pedido.UserId == CurrentUser!.Id && t.Estado == "APPROVED")
                    .ToListAsync();

                foreach (var tx in txAprobadas)
                {
                    // Buscar pedido por TransactionId
                    var pedido = await _context.Pedidos.FirstOrDefaultAsync(p => p.TransactionId == tx.TransactionId);
                    if (pedido != null && pedido.Estado == "Pendiente")
                    {
                        pedido.Estado = "Pagado";
                        pedido.FechaPago = tx.FechaTransaccion;
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al sincronizar transacciones con pedidos (usuario)");
            }
        }

        public string GetEstadoBadgeClass(string estado)
        {
            return estado.ToLower() switch
            {
                "pendiente" => "bg-warning text-dark",
                "pagado" => "bg-success",
                "enviado" => "bg-info",
                "entregado" => "bg-primary",
                "cancelado" => "bg-danger",
                _ => "bg-secondary"
            };
        }

        public string GetMetodoPagoIcon(string metodoPago)
        {
            return metodoPago.ToUpper() switch
            {
                "TARJETA" => "fas fa-credit-card",
                "PSE" => "fas fa-university",
                "EFECTY" => "fas fa-money-bill-wave",
                "NEQUI" => "fas fa-mobile-alt",
                _ => "fas fa-payment"
            };
        }

        public string GetEstadoEnvioColor(string? estado)
        {
            if (string.IsNullOrEmpty(estado)) return "text-muted";
            return estado.ToLower() switch
            {
                "preparando" => "text-warning",
                "enviado" => "text-info",
                "en_transito" => "text-primary",
                "entregado" => "text-success",
                _ => "text-muted"
            };
        }

        public string GetEstadoTransaccion(long orderId)
        {
            var transaccion = Transacciones.FirstOrDefault(t => t.OrderId == orderId);
            return transaccion?.Estado ?? "Desconocido";
        }

        public EnvioInfo? GetEnvioPorTransaccion(string transactionId)
        {
            return Envios.FirstOrDefault(e => e.TransactionId == transactionId);
            }
    }
}