using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComputerStore.Models
{
    public class EnvioEntity
    {
        [Key]
        public int Id { get; set; }

        public int PedidoId { get; set; }

        [Required]
        [StringLength(50)]
        public string NumeroGuia { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Estado { get; set; } = "PREPARANDO"; // PREPARANDO, RECOLECTADO, EN_CENTRO_DISTRIBUCION, EN_TRANSITO_A_DESTINO, EN_RUTA_ENTREGA, ENTREGADO, FALLIDO

        [StringLength(100)]
        public string? Transportadora { get; set; } = "Servientrega";

        [Required]
        [StringLength(200)]
        public string DireccionEnvio { get; set; } = string.Empty;

        [StringLength(200)]
        public string? DireccionSecundaria { get; set; }

        [Required]
        [StringLength(100)]
        public string CiudadEnvio { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string DepartamentoEnvio { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string CodigoPostal { get; set; } = string.Empty;

        [StringLength(15)]
        public string? TelefonoContacto { get; set; }

        [StringLength(500)]
        public string? NotasEntrega { get; set; }

        // Fechas del proceso de envío
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow; // UTC
        public DateTime? FechaRecoleccion { get; set; }
        public DateTime? FechaLlegadaCentroDistribucion { get; set; }
        public DateTime? FechaSalidaCentroDistribucion { get; set; }
        public DateTime? FechaLlegadaCiudadDestino { get; set; }
        public DateTime? FechaEnRutaEntrega { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public DateTime FechaEstimadaEntrega { get; set; } = DateTime.UtcNow.AddDays(3); // UTC

        // Información adicional del envío
        [StringLength(500)]
        public string? Observaciones { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CostoEnvio { get; set; }

        [StringLength(100)]
        public string? UrlRastreo { get; set; }

        [StringLength(100)]
        public string? CentroDistribucionActual { get; set; }

        [StringLength(100)]
        public string? VehiculoEntrega { get; set; }

        [StringLength(100)]
        public string? RepartidorAsignado { get; set; }

        // Relaciones
        [ForeignKey("PedidoId")]
        public virtual PedidoEntity Pedido { get; set; } = null!;

        // Método para convertir a EnvioInfo (modelo existente)
        public EnvioInfo ToEnvioInfo()
        {
            var productos = this.Pedido?.Detalles?.Select(d => new ProductoEnvio
            {
                Nombre = d.Producto?.Name ?? "Producto",
                Precio = d.PrecioUnitario,
                Cantidad = d.Cantidad
            }).ToList() ?? new List<ProductoEnvio>();

            var precioTotal = productos.Sum(p => p.SubTotal);
            var nombreProducto = productos.Count switch
            {
                0 => "Compra procesada",
                1 => productos.First().Nombre,
                _ => $"Compra múltiple ({productos.Count} productos)"
            };

            return new EnvioInfo
            {
                OrderId = this.PedidoId,
                TransactionId = this.Pedido?.TransactionId ?? "",
                NumeroGuia = this.NumeroGuia,
                UserId = this.Pedido?.UserId ?? string.Empty,
                Estado = this.Estado,
                Transportadora = this.Transportadora ?? "Servientrega",
                DireccionEnvio = this.DireccionEnvio,
                FechaCreacion = this.FechaCreacion,
                FechaEnvio = this.FechaRecoleccion,
                FechaEntrega = this.FechaEntrega,
                FechaEstimadaEntrega = this.FechaEstimadaEntrega,
                Observaciones = this.Observaciones ?? "",
                Productos = productos,
                PrecioTotal = precioTotal,
                NombreProducto = nombreProducto
            };
        }

        // Métodos auxiliares UI
        public string GetEstadoDescripcion()
        {
            return Estado switch
            {
                "PREPARANDO" => "Preparando el pedido",
                "RECOLECTADO" => "Recolectado por la transportadora",
                "EN_CENTRO_DISTRIBUCION" => "En centro de distribución",
                "EN_TRANSITO_A_DESTINO" => "En tránsito hacia destino",
                "EN_RUTA_ENTREGA" => "En ruta de entrega a domicilio",
                "ENTREGADO" => "Entregado exitosamente",
                "FALLIDO" => "Intento de entrega fallido",
                _ => "Estado desconocido"
            };
        }

        public string GetEstadoColor()
        {
            return Estado switch
            {
                "PREPARANDO" => "warning",
                "RECOLECTADO" => "info", 
                "EN_CENTRO_DISTRIBUCION" => "primary",
                "EN_TRANSITO_A_DESTINO" => "info",
                "EN_RUTA_ENTREGA" => "warning",
                "ENTREGADO" => "success",
                "FALLIDO" => "danger",
                _ => "secondary"
            };
        }

        public string GetEstadoIcon()
        {
            return Estado switch
            {
                "PREPARANDO" => "fas fa-box",
                "RECOLECTADO" => "fas fa-hand-paper",
                "EN_CENTRO_DISTRIBUCION" => "fas fa-warehouse",
                "EN_TRANSITO_A_DESTINO" => "fas fa-truck",
                "EN_RUTA_ENTREGA" => "fas fa-map-marker-alt",
                "ENTREGADO" => "fas fa-check-circle",
                "FALLIDO" => "fas fa-exclamation-triangle",
                _ => "fas fa-question"
            };
        }

        public int GetProgresoPorcentaje()
        {
            return Estado switch
            {
                "PREPARANDO" => 10,
                "RECOLECTADO" => 25,
                "EN_CENTRO_DISTRIBUCION" => 40,
                "EN_TRANSITO_A_DESTINO" => 60,
                "EN_RUTA_ENTREGA" => 85,
                "ENTREGADO" => 100,
                "FALLIDO" => 100,
                _ => 0
            };
        }

        public string GetProximoPaso()
        {
            return Estado switch
            {
                "PREPARANDO" => "El pedido será recolectado por la transportadora",
                "RECOLECTADO" => "El pedido se dirigirá al centro de distribución",
                "EN_CENTRO_DISTRIBUCION" => "El pedido será enviado hacia tu ciudad",
                "EN_TRANSITO_A_DESTINO" => "El pedido llegará a tu ciudad y será asignado para entrega",
                "EN_RUTA_ENTREGA" => "El repartidor se dirigirá a tu domicilio",
                "ENTREGADO" => "¡Pedido entregado! Gracias por tu compra",
                "FALLIDO" => "Se programará un nuevo intento de entrega",
                _ => "Información no disponible"
            };
        }
    }
}