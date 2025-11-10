using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComputerStore.Models
{
    public class PedidoEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ReferenceCode { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [Required]
        [StringLength(50)]
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Pagado, Cancelado, Entregado

        [Required]
        [StringLength(50)]
        public string MetodoPago { get; set; } = string.Empty; // TARJETA, PSE, EFECTY, NEQUI

        [StringLength(100)]
        public string? TransactionId { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow; // UTC

        public DateTime? FechaPago { get; set; }

        public DateTime? FechaEntrega { get; set; }

        [StringLength(500)]
        public string? Observaciones { get; set; }

        // Datos de envío
        [Required]
        [StringLength(200)]
        public string DireccionEnvio { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string CiudadEnvio { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string DepartamentoEnvio { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string CodigoPostalEnvio { get; set; } = string.Empty;

        [StringLength(15)]
        public string? TelefonoContacto { get; set; }

        [StringLength(500)]
        public string? NotasEntrega { get; set; }

        // Relaciones
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        public virtual ICollection<PedidoDetalle> Detalles { get; set; } = new List<PedidoDetalle>();
        public virtual EnvioEntity? Envio { get; set; }
    }

    public class PedidoDetalle
    {
        [Key]
        public int Id { get; set; }

        public int PedidoId { get; set; }

        public int ProductoId { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecioUnitario { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        // Relaciones
        [ForeignKey("PedidoId")]
        public virtual PedidoEntity Pedido { get; set; } = null!;

        [ForeignKey("ProductoId")]
        public virtual ProductEntity Producto { get; set; } = null!;
    }
}