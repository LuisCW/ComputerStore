using System.ComponentModel.DataAnnotations;

namespace ComputerStore.Models
{
    public class MarketingConfiguration
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Canal { get; set; } = string.Empty; // Google Ads, Facebook Ads, Email
        
        [Required]
        [MaxLength(50)]
        public string Plataforma { get; set; } = string.Empty; // google_ads, facebook_ads, mailchimp, etc.
        
        [Required]
        [MaxLength(500)]
        public string ConfiguracionEncriptada { get; set; } = string.Empty; // JSON encriptado con las credenciales
        
        public bool Activo { get; set; } = true;
        
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        
        public DateTime? FechaUltimaActualizacion { get; set; }
        
        public DateTime? UltimaSincronizacion { get; set; }
        
        [MaxLength(20)]
        public string Estado { get; set; } = "conectado"; // conectado, desconectado, error, expirado
        
        [MaxLength(200)]
        public string? Notas { get; set; }
        
        // Información adicional para debugging
        [MaxLength(50)]
        public string? UsuarioCreacion { get; set; }
        
        public int IntentosConexion { get; set; } = 0;
        
        public DateTime? UltimoIntento { get; set; }
    }
}