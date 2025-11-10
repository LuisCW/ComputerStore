using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ComputerStore.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string PrimerNombre { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string SegundoNombre { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string PrimerApellido { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string SegundoApellido { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string TipoDocumento { get; set; } = string.Empty; // CC, CE, PP, etc.

        [Required]
        [StringLength(20)]
        public string NumeroDocumento { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Direccion { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Ciudad { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Departamento { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string CodigoPostal { get; set; } = string.Empty;

        [StringLength(200)]
        public string? DireccionSecundaria { get; set; }

        [StringLength(500)]
        public string? NotasEntrega { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public DateTime? FechaUltimoAcceso { get; set; }

        public bool IsActive { get; set; } = true;

        // Relaciones
        public virtual ICollection<PedidoEntity> Pedidos { get; set; } = new List<PedidoEntity>();
        public virtual ICollection<EnvioEntity> Envios { get; set; } = new List<EnvioEntity>();
    }
}