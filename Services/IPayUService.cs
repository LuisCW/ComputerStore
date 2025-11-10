using ComputerStore.Models;
using System.Threading.Tasks;

namespace ComputerStore.Services
{
    public interface IPayUService
    {
        // Métodos para envíos (básicos que necesitamos)
        string CrearEnvio(string userId, string nombreProducto, decimal precioTotal, string ciudadDestino);
        string ConsultarEstadoEnvio(string numeroGuia);
        string ActualizarEstadoEnvio(string numeroGuia);
        List<EnvioInfo> ObtenerTodosLosEnvios();
        EnvioInfo? ObtenerEnvioPorGuia(string numeroGuia);
        EnvioInfo ObtenerEnvioPorOrden(long orderId);
        void EliminarEnvio(string numeroGuia);

        // Métodos para transacciones
        List<TransaccionInfo> ObtenerTodasLasTransacciones();
        TransaccionInfo ObtenerTransaccion(string transactionId);

        // Métodos para pagos
        Task<string> RealizarPago(Pedido pedido,
            string MetodoPago,
            string numeroTarjeta,
            string cvv,
            string expiracionMes,
            string expiracionAnio,
            decimal total,
            string tipoTarjeta,
            string documentoPSE = null,
            string bancoPSE = null,
            string documentoEfecty = null,
            string celularNequi = null,
            string nombreTarjeta = "APRO");

        // Métodos de diagnóstico y conectividad
        Task<(bool Success, string Message, string Details)> ProbarConectividadPayU();

        // Métodos adicionales para tests y administración
        Task<bool> ActualizarEstadoEnvioPublico(long orderId, string nuevoEstado);
        string ObtenerSiguienteEstado(string estadoActual);

        // NUEVO: Configuración de zona horaria
        void ConfigurarZonaHoraria(TimeZoneInfo timeZone);
        DateTime ObtenerFechaColombiaActual();
    }
}
