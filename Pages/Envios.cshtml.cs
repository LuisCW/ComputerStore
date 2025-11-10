using ComputerStore.Models;
using ComputerStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ComputerStore.Data;
using Microsoft.EntityFrameworkCore;

namespace ComputerStore.Pages
{
    [Authorize]
    public class EnviosModel : PageModel
    {
        private readonly IPayUService _payUService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public List<EnvioInfo> Envios { get; set; } = new List<EnvioInfo>();
        public string MensajeError { get; set; } = string.Empty;
        public string MensajeExito { get; set; } = string.Empty;
        
        // ? AGREGADO: Propiedades que faltaban en el modelo
        public string Mensaje { get; set; } = string.Empty;
        
        [BindProperty]
        public string NumeroGuia { get; set; } = string.Empty;

        public EnviosModel(IPayUService payUService, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _payUService = payUService;
            _userManager = userManager;
            _context = context;
        }

        public async Task OnGetAsync()
        {
            try
            {
                // ? CRÍTICO: Obtener el usuario actual para filtrar envíos
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    MensajeError = "Usuario no encontrado. Por favor, inicia sesión nuevamente.";
                    Mensaje = MensajeError; // ? Duplicar en Mensaje para compatibilidad
                    return;
                }

                Console.WriteLine($"?? Buscando envíos para usuario: {user.Id} ({user.UserName})");

                // Cargar envíos desde la base de datos para el usuario
                var enviosDb = await _context.Envios
                    .Include(e => e.Pedido)
                        .ThenInclude(p => p.Detalles)
                            .ThenInclude(d => d.Producto)
                    .Where(e => e.Pedido.UserId == user.Id)
                    .OrderByDescending(e => e.FechaCreacion)
                    .ToListAsync();

                Envios = enviosDb.Select(e => e.ToEnvioInfo()).ToList();

                Console.WriteLine($"?? Envíos encontrados para {user.UserName}: {Envios.Count}");

                if (!Envios.Any())
                {
                    MensajeError = "No tienes envíos registrados aún. Realiza una compra para ver tus envíos aquí.";
                    Mensaje = MensajeError; // ? Duplicar en Mensaje para compatibilidad
                }
                else
                {
                    Mensaje = $"Se encontraron {Envios.Count} envío(s) registrados.";
                    // Log de envíos encontrados
                    foreach (var envio in Envios)
                    {
                        Console.WriteLine($"   ?? Guía: {envio.NumeroGuia} | Estado: {envio.Estado} | Producto: {envio.NombreProducto}");
                    }
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al cargar envíos: {ex.Message}";
                Mensaje = MensajeError; // ? Duplicar en Mensaje para compatibilidad
                Console.WriteLine($"? Error en OnGetAsync: {ex.Message}");
            }
        }

        public async Task<IActionResult> OnPostConsultarEstadoAsync(string numeroGuia)
        {
            try
            {
                // ? VERIFICAR que el envío pertenece al usuario actual
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Usuario no encontrado. Por favor, inicia sesión nuevamente.";
                    return RedirectToPage();
                }

                Console.WriteLine($"?? Actualizando estado de envío {numeroGuia} para usuario {user.UserName}");

                // Validar y actualizar en BD
                var envio = await _context.Envios
                    .Include(e => e.Pedido)
                    .FirstOrDefaultAsync(e => e.NumeroGuia == numeroGuia && e.Pedido.UserId == user.Id);

                if (envio == null)
                {
                    TempData["ErrorMessage"] = "Envío no encontrado o no tienes permisos para consultarlo.";
                    Console.WriteLine($"?? Intento de acceso no autorizado al envío {numeroGuia} por usuario {user.UserName}");
                    return RedirectToPage();
                }

                var estadoAnterior = envio.Estado;
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

                if (envio.Estado != estadoAnterior)
                {
                    TempData["SuccessMessage"] = $"Estado actualizado de '{estadoAnterior}' a '{envio.Estado}' para envío {numeroGuia}";
                }
                else
                {
                    TempData["InfoMessage"] = $"El envío {numeroGuia} ya está en el estado más reciente: {envio.Estado}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al consultar estado: {ex.Message}";
                Console.WriteLine($"? Error en OnPostConsultarEstadoAsync: {ex.Message}");
            }

            return RedirectToPage();
        }

        // ? NUEVO: Método para buscar envío por número de guía
        public async Task<IActionResult> OnPostBuscarEnvioAsync()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Usuario no encontrado. Por favor, inicia sesión nuevamente.";
                    return RedirectToPage();
                }

                if (string.IsNullOrEmpty(NumeroGuia))
                {
                    TempData["ErrorMessage"] = "Por favor, ingresa un número de guía válido.";
                    return RedirectToPage();
                }

                Console.WriteLine($"?? Buscando envío {NumeroGuia} para usuario {user.UserName}");

                var envio = await _context.Envios
                    .Include(e => e.Pedido)
                    .FirstOrDefaultAsync(e => e.NumeroGuia == NumeroGuia && e.Pedido.UserId == user.Id);
                
                if (envio == null)
                {
                    TempData["ErrorMessage"] = "No se encontró ningún envío con ese número de guía.";
                    return RedirectToPage();
                }

                TempData["SuccessMessage"] = $"Envío {NumeroGuia} encontrado - Estado: {envio.Estado}";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al buscar envío: {ex.Message}";
                Console.WriteLine($"? Error en OnPostBuscarEnvioAsync: {ex.Message}");
                return RedirectToPage();
            }
        }

        // ? NUEVO: Método para mostrar detalles del envío (solo del usuario actual)
        public async Task<IActionResult> OnGetDetalleEnvioAsync(string numeroGuia)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToPage("/Account/Login");
                }

                var envio = await _context.Envios
                    .Include(e => e.Pedido)
                        .ThenInclude(p => p.Detalles)
                            .ThenInclude(d => d.Producto)
                    .FirstOrDefaultAsync(e => e.NumeroGuia == numeroGuia && e.Pedido.UserId == user.Id);

                if (envio == null)
                {
                    TempData["ErrorMessage"] = "Envío no encontrado o no tienes permisos para verlo.";
                    return RedirectToPage();
                }

                var info = envio.ToEnvioInfo();

                // Retornar JSON con detalles del envío para mostrar en modal
                return new JsonResult(new
                {
                    success = true,
                    envio = new
                    {
                        numeroGuia = info.NumeroGuia,
                        estado = info.Estado,
                        nombreProducto = info.NombreProducto,
                        precioTotal = info.PrecioTotal,
                        fechaCreacion = info.FechaCreacion.ToString("dd/MM/yyyy HH:mm"),
                        fechaEstimadaEntrega = info.FechaEstimadaEntrega.ToString("dd/MM/yyyy"),
                        direccionEnvio = info.DireccionEnvio,
                        transportadora = info.Transportadora,
                        productos = info.Productos?.Select(p => new
                        {
                            nombre = p.Nombre,
                            precio = p.Precio,
                            cantidad = p.Cantidad
                        }).Cast<object>().ToList() ?? new List<object>()
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error obteniendo detalle de envío: {ex.Message}");
                return new JsonResult(new { success = false, message = "Error al obtener detalles del envío" });
            }
        }
    }
}