using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComputerStore.Models
{
    public class ProductoImagen
    {
        [Key]
        public int Id { get; set; }

        public int ProductoId { get; set; }

        [Required]
        [StringLength(500)]
        public string ImagenUrl { get; set; } = string.Empty;

        [StringLength(255)]
        public string? NombreArchivo { get; set; }

        [StringLength(255)]
        public string? AltText { get; set; }

        public bool EsPrincipal { get; set; } = false;

        public int Orden { get; set; } = 0;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // Relación
        [ForeignKey("ProductoId")]
        public virtual ProductEntity Producto { get; set; } = null!;
    }

    public class TransaccionEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string TransactionId { get; set; } = string.Empty;

        public int? PedidoId { get; set; }

        [Required]
        [StringLength(50)]
        public string Estado { get; set; } = string.Empty; // APPROVED, REJECTED, PENDING

        [Required]
        [StringLength(50)]
        public string MetodoPago { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto { get; set; }

        [Required]
        [StringLength(10)]
        public string Moneda { get; set; } = "COP";

        [StringLength(100)]
        public string? ReferenceCode { get; set; }

        [StringLength(500)]
        public string? ResponseMessage { get; set; }

        [StringLength(100)]
        public string? TrazabilityCode { get; set; }

        [StringLength(100)]
        public string? AuthorizationCode { get; set; }

        public DateTime FechaTransaccion { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "nvarchar(max)")]
        public string? ExtraParametersJson { get; set; }

        // Relación
        [ForeignKey("PedidoId")]
        public virtual PedidoEntity? Pedido { get; set; }

        // Propiedad no mapeada para los parámetros extra
        [NotMapped]
        public Dictionary<string, object> ExtraParameters
        {
            get
            {
                if (string.IsNullOrEmpty(ExtraParametersJson))
                    return new Dictionary<string, object>();
                
                try
                {
                    return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(ExtraParametersJson) 
                           ?? new Dictionary<string, object>();
                }
                catch
                {
                    return new Dictionary<string, object>();
                }
            }
            set
            {
                ExtraParametersJson = System.Text.Json.JsonSerializer.Serialize(value ?? new Dictionary<string, object>());
            }
        }

        // Método para convertir a TransaccionInfo (modelo existente)
        public TransaccionInfo ToTransaccionInfo()
        {
            return new TransaccionInfo
            {
                TransactionId = this.TransactionId,
                OrderId = this.PedidoId ?? 0,
                ReferenceCode = this.ReferenceCode ?? "",
                Estado = this.Estado,
                MetodoPago = this.MetodoPago,
                Monto = this.Monto,
                Moneda = this.Moneda,
                FechaTransaccion = this.FechaTransaccion,
                ResponseMessage = this.ResponseMessage ?? "",
                TrazabilityCode = this.TrazabilityCode ?? "",
                AuthorizationCode = this.AuthorizationCode ?? "",
                ExtraParameters = this.ExtraParameters,
                UserId = this.Pedido?.UserId ?? string.Empty,
                UserName = this.Pedido?.User?.UserName,
                UserEmail = this.Pedido?.User?.Email
            };
        }
    }
}