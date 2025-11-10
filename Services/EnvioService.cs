using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ComputerStore.Data;
using ComputerStore.Models;

namespace ComputerStore.Services
{
    public interface IEnvioService
    {
        Task<EnvioInfo> CrearEnvioAsync(long orderId, string transactionId, string userId);
        Task<EnvioInfo?> ObtenerEnvioPorOrderIdAsync(long orderId);
        Task<List<EnvioInfo>> ObtenerEnviosUsuarioAsync(string userId);
        Task<List<EnvioInfo>> ObtenerTodosLosEnviosAsync();
        Task<bool> ActualizarEstadoEnvioAsync(long orderId, string nuevoEstado);
        Task<List<RastreoDetalle>> GenerarRastreoDetalladoAsync(string numeroGuia, string estadoActual);
    }

    public class EnvioService : IEnvioService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly LittleCarService _carritoService;

        public EnvioService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, LittleCarService carritoService)
        {
            _context = context;
            _userManager = userManager;
            _carritoService = carritoService;
        }

        public async Task<EnvioInfo> CrearEnvioAsync(long orderId, string transactionId, string userId)
        {
            // Obtener información del usuario
            var usuario = await _userManager.FindByIdAsync(userId);
            if (usuario == null)
            {
                throw new ArgumentException("Usuario no encontrado");
            }

            // Crear dirección completa
            var direccionCompleta = $"{usuario.Direccion}";
            if (!string.IsNullOrEmpty(usuario.DireccionSecundaria))
            {
                direccionCompleta += $", {usuario.DireccionSecundaria}";
            }
            direccionCompleta += $", {usuario.Ciudad}, {usuario.Departamento}, Colombia";

            // Obtener productos del carrito
            var productos = _carritoService.ObtenerCarrito() ?? _carritoService.ObtenerUltimaCompra() ?? new List<Products>();

            var envioInfo = new EnvioInfo
            {
                OrderId = orderId,
                TransactionId = transactionId,
                NumeroGuia = $"GU{orderId:D8}",
                Estado = "PREPARANDO",
                Transportadora = "Servientrega",
                DireccionEnvio = direccionCompleta,
                FechaCreacion = DateTime.UtcNow,
                FechaEstimadaEntrega = DateTime.UtcNow.AddDays(3),
                Observaciones = usuario.NotasEntrega ?? "",
                Productos = productos.Select(p => new ProductoEnvio
                {
                    Nombre = p.Name,
                    Precio = p.Price,
                    Cantidad = 1
                }).ToList()
            };

            // Aquí podrías guardar en base de datos si tienes la entidad EnvioEntity configurada
            // await GuardarEnBaseDeDatos(envioInfo, usuario);

            return envioInfo;
        }

        public async Task<EnvioInfo?> ObtenerEnvioPorOrderIdAsync(long orderId)
        {
            // Implementar lógica para obtener de base de datos
            // Por ahora retornamos null
            return null;
        }

        public async Task<List<EnvioInfo>> ObtenerEnviosUsuarioAsync(string userId)
        {
            // Implementar lógica para obtener envíos del usuario
            return new List<EnvioInfo>();
        }

        public async Task<List<EnvioInfo>> ObtenerTodosLosEnviosAsync()
        {
            // Implementar lógica para obtener todos los envíos
            return new List<EnvioInfo>();
        }

        public async Task<bool> ActualizarEstadoEnvioAsync(long orderId, string nuevoEstado)
        {
            // Implementar lógica para actualizar estado en base de datos
            return true;
        }

        public async Task<List<RastreoDetalle>> GenerarRastreoDetalladoAsync(string numeroGuia, string estadoActual)
        {
            var today = DateTime.UtcNow;
            var rastreo = new List<RastreoDetalle>();

            // Generar rastreo basado en el estado actual
            var estados = new[]
            {
                new { Estado = "PREPARANDO", Dias = -4, Hora = "08:30", Ubicacion = "Centro de distribución CompuHiperMegaRed - Bogotá", Descripcion = "Su pedido ha sido empacado y está listo para ser recogido por la transportadora" },
                new { Estado = "RECOLECTADO", Dias = -3, Hora = "14:45", Ubicacion = "Servientrega - Sucursal Principal Bogotá", Descripcion = "El paquete ha sido recogido y registrado en el sistema de la transportadora" },
                new { Estado = "EN_CENTRO_DISTRIBUCION", Dias = -3, Hora = "18:20", Ubicacion = "Hub de distribución regional - Bogotá", Descripcion = "El paquete está siendo procesado y clasificado para su envío" },
                new { Estado = "EN_TRANSITO_A_DESTINO", Dias = -2, Hora = "06:15", Ubicacion = "Transporte terrestre - Ruta Bogotá-Destino", Descripcion = "El paquete viaja hacia su ciudad de destino" },
                new { Estado = "LLEGADA_CIUDAD_DESTINO", Dias = -1, Hora = "10:30", Ubicacion = "Centro de distribución local", Descripcion = "El paquete ha llegado a su ciudad y está siendo preparado para entrega" },
                new { Estado = "EN_RUTA_ENTREGA", Dias = 0, Hora = "09:15", Ubicacion = "Vehículo de entrega - Zona local", Descripcion = "El repartidor está en camino a su dirección registrada" },
                new { Estado = "ENTREGADO", Dias = 0, Hora = "Pendiente", Ubicacion = "Dirección del cliente", Descripcion = "Entrega programada en horario de 8:00 AM - 6:00 PM" }
            };

            var estadoIndex = Array.FindIndex(estados, e => e.Estado == estadoActual);
            if (estadoIndex == -1) estadoIndex = 0;

            for (int i = 0; i <= Math.Min(estadoIndex + 1, estados.Length - 1); i++)
            {
                var estado = estados[i];
                var fecha = estado.Hora == "Pendiente" ? "Pendiente" : 
                           today.AddDays(estado.Dias).ToString("dd/MM/yyyy") + $", {estado.Hora}:00";

                rastreo.Add(new RastreoDetalle
                {
                    Fecha = fecha,
                    Estado = GetEstadoDescripcion(estado.Estado),
                    Ubicacion = estado.Ubicacion,
                    Descripcion = estado.Descripcion,
                    EsCompletado = estado.Hora != "Pendiente",
                    EsActual = i == estadoIndex
                });
            }

            return rastreo;
        }

        private string GetEstadoDescripcion(string estado)
        {
            return estado switch
            {
                "PREPARANDO" => "Paquete preparado",
                "RECOLECTADO" => "Recogido por transportadora",
                "EN_CENTRO_DISTRIBUCION" => "En centro de distribución",
                "EN_TRANSITO_A_DESTINO" => "En tránsito hacia destino",
                "LLEGADA_CIUDAD_DESTINO" => "Llegada a ciudad destino",
                "EN_RUTA_ENTREGA" => "En ruta de entrega",
                "ENTREGADO" => "Entregado",
                _ => estado
            };
        }
    }

    public class RastreoDetalle
    {
        public string Fecha { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool EsCompletado { get; set; }
        public bool EsActual { get; set; }
    }
}