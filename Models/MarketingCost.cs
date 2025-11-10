using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComputerStore.Models;

/// <summary>
/// Trackea los gastos de marketing por canal para calcular ROI real
/// </summary>
public class MarketingCost
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(100)]
    public string Canal { get; set; } = string.Empty; // SEO, Google Ads, Facebook, Email, etc.

    [Required]
    [StringLength(200)]
    public string Campana { get; set; } = string.Empty; // Nombre de la campaña

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal CostoTotal { get; set; } // Gasto total del día en esta campaña

    [Column(TypeName = "decimal(18,2)")]
    public decimal? CostoPorClick { get; set; } // CPC promedio

    public int? Clicks { get; set; } // Número de clicks

    public int? Impresiones { get; set; } // Número de impresiones

    [StringLength(50)]
    public string? TipoCampana { get; set; } // PPC, Display, Social, Email, etc.

    [StringLength(500)]
    public string? Notas { get; set; } // Notas adicionales

    // Campos para integración con APIs de publicidad
    [StringLength(100)]
    public string? CampanaIdExterna { get; set; } // ID de Google Ads, Facebook Ads, etc.

    [StringLength(100)]
    public string? PlataformaPublicidad { get; set; } // google_ads, facebook_ads, etc.

    // ?? NUEVO: Campos de descuento por categoría
    public bool AplicaDescuento { get; set; } = false;

    [Column(TypeName = "decimal(5,2)")]
    [Range(0, 100)]
    public decimal? PorcentajeDescuento { get; set; }

    [StringLength(100)]
    public string? CategoriaDescuento { get; set; } // "Todos", "Laptops", "Monitores", etc.

    public DateTime? FechaInicioDescuento { get; set; }

    public DateTime? FechaFinDescuento { get; set; }

    public bool DescuentoActivo { get; set; } = false;
}