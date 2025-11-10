using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComputerStore.Models;

public class TrafficEvent
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime Fecha { get; set; } = DateTime.UtcNow; // UTC

    [Required]
    [StringLength(300)]
    public string Path { get; set; } = string.Empty;

    [StringLength(100)]
    public string? UserId { get; set; }

    [StringLength(100)]
    public string? SessionId { get; set; }

    [StringLength(500)]
    public string? Referrer { get; set; }

    [StringLength(500)]
    public string? UserAgent { get; set; }

    [StringLength(100)]
    public string? Source { get; set; }

    [StringLength(100)]
    public string? Medium { get; set; }

    [StringLength(100)]
    public string? Campaign { get; set; }

    [StringLength(50)]
    public string? Device { get; set; }

    [StringLength(50)]
    public string? Ip { get; set; }
}