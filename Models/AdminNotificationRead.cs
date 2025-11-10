using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComputerStore.Models
{
    public class AdminNotificationRead
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int NotificationId { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        public DateTime FechaLectura { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(NotificationId))]
        public AdminNotification Notification { get; set; } = null!;
    }
}
