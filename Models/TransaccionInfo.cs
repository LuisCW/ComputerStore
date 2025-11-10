namespace ComputerStore.Models
{
    public class TransaccionInfo
    {
        public string TransactionId { get; set; } = string.Empty;
        public long OrderId { get; set; }
        public string ReferenceCode { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string MetodoPago { get; set; } = string.Empty;
        public decimal Monto { get; set; }
        public string Moneda { get; set; } = "COP";
        public DateTime FechaTransaccion { get; set; } = DateTime.UtcNow;
        public string ResponseMessage { get; set; } = string.Empty;
        public string TrazabilityCode { get; set; } = string.Empty;
        public string AuthorizationCode { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty; // ? AGREGADO
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public Dictionary<string, object> ExtraParameters { get; set; } = new Dictionary<string, object>();
    }
}