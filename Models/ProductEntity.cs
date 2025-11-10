using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComputerStore.Models
{
    public class ProductEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [StringLength(100)]
        public string Brand { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Component { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }

        [Required]
        [StringLength(50)]
        public string Availability { get; set; } = "Disponible";

        [Required]
        public int Stock { get; set; } = 0;

        [Required]
        public int StockMinimo { get; set; } = 5;

        [StringLength(100)]
        public string? SKU { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? PrecioCompra { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OriginalPrice { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? PorcentajeGanancia { get; set; }

        // ?? NUEVO: Campos de descuento
        public bool TieneDescuento { get; set; } = false;

        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 100)]
        public decimal? PorcentajeDescuento { get; set; }

        public DateTime? FechaInicioDescuento { get; set; }

        public DateTime? FechaFinDescuento { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? FechaActualizacion { get; set; }

        // Especificaciones técnicas como JSON
        [Column(TypeName = "text")]
        public string? DetailsJson { get; set; }

        // Campos específicos para Laptops/Computadores
        [StringLength(200)]
        public string? Procesador { get; set; }

        [StringLength(100)]
        public string? RAM { get; set; }

        [StringLength(200)]
        public string? Almacenamiento { get; set; }

        [StringLength(200)]
        public string? TarjetaGrafica { get; set; }

        [StringLength(200)]
        public string? Pantalla { get; set; }

        [StringLength(100)]
        public string? SistemaOperativo { get; set; }

        // Campos específicos para Periféricos - Mouse
        [StringLength(50)]
        public string? DPI { get; set; }

        [StringLength(50)]
        public string? Botones { get; set; }

        [StringLength(50)]
        public string? Peso { get; set; }

        // Campos específicos para Periféricos - Teclado
        [StringLength(100)]
        public string? Switch { get; set; }

        [StringLength(100)]
        public string? Retroiluminacion { get; set; }

        [StringLength(50)]
        public string? Layout { get; set; }

        // Campos específicos para Monitores
        [StringLength(100)]
        public string? Resolucion { get; set; }

        [StringLength(50)]
        public string? Frecuencia { get; set; }

        [StringLength(100)]
        public string? Panel { get; set; }

        [StringLength(50)]
        public string? HDR { get; set; }

        // Campos comunes
        [Column(TypeName = "text")]
        public string? Conectividad { get; set; }

        [StringLength(100)]
        public string? Garantia { get; set; }

        // Relaciones
        public virtual ICollection<PedidoDetalle> PedidoDetalles { get; set; } = new List<PedidoDetalle>();
        public virtual ICollection<ProductoImagen> Imagenes { get; set; } = new List<ProductoImagen>();

        // ?? NUEVO: Propiedad calculada para precio con descuento
        [NotMapped]
        public decimal PrecioConDescuento
        {
            get
            {
                if (!TieneDescuento || !PorcentajeDescuento.HasValue)
                    return Price;

                // Verificar si el descuento está vigente
                var ahora = DateTime.UtcNow;
                if (FechaInicioDescuento.HasValue && ahora < FechaInicioDescuento.Value)
                    return Price;

                if (FechaFinDescuento.HasValue && ahora > FechaFinDescuento.Value)
                    return Price;

                var descuento = Price * (PorcentajeDescuento.Value / 100);
                return Price - descuento;
            }
        }

        // ?? NUEVO: Verificar si el descuento está activo
        [NotMapped]
        public bool DescuentoActivo
        {
            get
            {
                if (!TieneDescuento || !PorcentajeDescuento.HasValue)
                    return false;

                var ahora = DateTime.UtcNow;
                
                if (FechaInicioDescuento.HasValue && ahora < FechaInicioDescuento.Value)
                    return false;

                if (FechaFinDescuento.HasValue && ahora > FechaFinDescuento.Value)
                    return false;

                return true;
            }
        }

        // Propiedad no mapeada para los detalles (compatibilidad con el sistema anterior)
        [NotMapped]
        public Dictionary<string, string> Details
        {
            get
            {
                var details = new Dictionary<string, string>();

                // Agregar campos específicos según la categoría
                if (!string.IsNullOrEmpty(Category))
                {
                    switch (Category.ToLower())
                    {
                        case "laptops":
                        case "computadores":
                        case "computadoras de escritorio":
                            if (!string.IsNullOrEmpty(Procesador)) details["Procesador"] = Procesador;
                            if (!string.IsNullOrEmpty(RAM)) details["RAM"] = RAM;
                            if (!string.IsNullOrEmpty(Almacenamiento)) details["Almacenamiento"] = Almacenamiento;
                            if (!string.IsNullOrEmpty(TarjetaGrafica)) details["Tarjeta Gráfica"] = TarjetaGrafica;
                            if (!string.IsNullOrEmpty(Pantalla)) details["Pantalla"] = Pantalla;
                            if (!string.IsNullOrEmpty(SistemaOperativo)) details["Sistema Operativo"] = SistemaOperativo;
                            break;

                        case "periféricos":
                            // Para mouse
                            if (!string.IsNullOrEmpty(DPI)) details["DPI"] = DPI;
                            if (!string.IsNullOrEmpty(Botones)) details["Botones"] = Botones;
                            if (!string.IsNullOrEmpty(Peso)) details["Peso"] = Peso;
                            
                            // Para teclado
                            if (!string.IsNullOrEmpty(Switch)) details["Switch"] = Switch;
                            if (!string.IsNullOrEmpty(Retroiluminacion)) details["Retroiluminación"] = Retroiluminacion;
                            if (!string.IsNullOrEmpty(Layout)) details["Layout"] = Layout;
                            break;

                        case "monitores":
                            if (!string.IsNullOrEmpty(Resolucion)) details["Resolución"] = Resolucion;
                            if (!string.IsNullOrEmpty(Frecuencia)) details["Frecuencia"] = Frecuencia;
                            if (!string.IsNullOrEmpty(Panel)) details["Panel"] = Panel;
                            if (!string.IsNullOrEmpty(HDR)) details["HDR"] = HDR;
                            if (!string.IsNullOrEmpty(Pantalla)) details["Tamaño"] = Pantalla;
                            break;
                    }
                }

                // Campos comunes
                if (!string.IsNullOrEmpty(Conectividad)) details["Conectividad"] = Conectividad;
                if (!string.IsNullOrEmpty(Garantia)) details["Garantía"] = Garantia;

                // Si hay JSON adicional, agregarlo
                if (!string.IsNullOrEmpty(DetailsJson))
                {
                    try
                    {
                        var jsonDetails = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(DetailsJson);
                        if (jsonDetails != null)
                        {
                            foreach (var kvp in jsonDetails)
                            {
                                if (!details.ContainsKey(kvp.Key))
                                {
                                    details[kvp.Key] = kvp.Value;
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Ignorar errores de deserialización
                    }
                }

                return details;
            }
            set
            {
                if (value == null)
                {
                    // Limpiar todos los campos específicos
                    Procesador = null;
                    RAM = null;
                    Almacenamiento = null;
                    TarjetaGrafica = null;
                    Pantalla = null;
                    SistemaOperativo = null;
                    DPI = null;
                    Botones = null;
                    Peso = null;
                    Switch = null;
                    Retroiluminacion = null;
                    Layout = null;
                    Resolucion = null;
                    Frecuencia = null;
                    Panel = null;
                    HDR = null;
                    Conectividad = null;
                    Garantia = null;
                    DetailsJson = null;
                    return;
                }

                // Separar campos específicos del JSON general
                var jsonDetails = new Dictionary<string, string>();

                foreach (var kvp in value)
                {
                    switch (kvp.Key.ToLower())
                    {
                        case "procesador":
                            Procesador = kvp.Value;
                            break;
                        case "ram":
                            RAM = kvp.Value;
                            break;
                        case "almacenamiento":
                            Almacenamiento = kvp.Value;
                            break;
                        case "tarjeta gráfica":
                        case "tarjetagrafica":
                            TarjetaGrafica = kvp.Value;
                            break;
                        case "pantalla":
                        case "tamaño":
                            Pantalla = kvp.Value;
                            break;
                        case "sistema operativo":
                        case "sistemaoperativo":
                            SistemaOperativo = kvp.Value;
                            break;
                        case "dpi":
                            DPI = kvp.Value;
                            break;
                        case "botones":
                            Botones = kvp.Value;
                            break;
                        case "peso":
                            Peso = kvp.Value;
                            break;
                        case "switch":
                            Switch = kvp.Value;
                            break;
                        case "retroiluminación":
                        case "retroiluminacion":
                            Retroiluminacion = kvp.Value;
                            break;
                        case "layout":
                            Layout = kvp.Value;
                            break;
                        case "resolución":
                        case "resolucion":
                            Resolucion = kvp.Value;
                            break;
                        case "frecuencia":
                            Frecuencia = kvp.Value;
                            break;
                        case "panel":
                            Panel = kvp.Value;
                            break;
                        case "hdr":
                            HDR = kvp.Value;
                            break;
                        case "conectividad":
                            Conectividad = kvp.Value;
                            break;
                        case "garantía":
                        case "garantia":
                            Garantia = kvp.Value;
                            break;
                        default:
                            // Campos que no tienen columna específica van al JSON
                            jsonDetails[kvp.Key] = kvp.Value;
                            break;
                    }
                }

                // Guardar campos adicionales en JSON
                DetailsJson = jsonDetails.Any() ? 
                    System.Text.Json.JsonSerializer.Serialize(jsonDetails) : 
                    null;
            }
        }

        // Método para convertir a Products (modelo existente)
        public Products ToProducts()
        {
            // Calcular precio final considerando descuentos activos
            var descuentoActivo = this.DescuentoActivo;
            var precioFinal = descuentoActivo ? this.PrecioConDescuento : this.Price;
            var originalParaMostrar = descuentoActivo ? (this.Price) : (this.OriginalPrice);

            return new Products
            {
                Id = this.Id,
                Name = this.Name,
                Category = this.Category,
                Price = precioFinal,
                OriginalPrice = originalParaMostrar,
                ImageUrl = this.ImageUrl ?? "",
                Description = this.Description ?? "",
                Brand = this.Brand,
                Component = this.Component ?? "",
                Color = this.Color ?? "",
                Availability = this.Availability,
                Stock = this.Stock,
                Details = this.Details,
                Imagenes = this.Imagenes?.ToList() ?? new List<ProductoImagen>()
            };
        }
    }
}