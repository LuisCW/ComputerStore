using ComputerStore.Data;
using ComputerStore.Models;
using ComputerStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ComputerStore.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class CreateProductModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IImageUploadService _imageUploadService;
        private readonly ILogger<CreateProductModel> _logger;

        [BindProperty]
        public CreateProductViewModel Product { get; set; } = new CreateProductViewModel();

        public CreateProductModel(ApplicationDbContext context, IImageUploadService imageUploadService, ILogger<CreateProductModel> logger)
        {
            _context = context;
            _imageUploadService = imageUploadService;
            _logger = logger;
        }

        public void OnGet()
        {
            Product = new CreateProductViewModel
            {
                Stock = 10,
                StockMinimo = 5,
                Availability = "Disponible"
            };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger.LogInformation("CREANDO PRODUCTO: {Name}", Product?.Name ?? "NULL");

                if (Product == null)
                {
                    TempData["ErrorMessage"] = "Error: No se recibieron datos del formulario";
                    return Page();
                }

                // Validaciones básicas
                if (string.IsNullOrWhiteSpace(Product.Name))
                {
                    TempData["ErrorMessage"] = "El nombre del producto es requerido";
                    return Page();
                }

                if (string.IsNullOrWhiteSpace(Product.Brand))
                {
                    TempData["ErrorMessage"] = "La marca es requerida";
                    return Page();
                }

                if (string.IsNullOrWhiteSpace(Product.Category))
                {
                    TempData["ErrorMessage"] = "La categoría es requerida";
                    return Page();
                }

                if (Product.Price <= 0)
                {
                    TempData["ErrorMessage"] = "El precio debe ser mayor a 0";
                    return Page();
                }

                // Generar SKU
                var sku = await GenerarSKUAutomaticoAsync(Product.Category, Product.Brand);

                // Crear ProductEntity SIMPLE
                var productEntity = new ProductEntity
                {
                    Name = Product.Name,
                    Category = Product.Category,
                    Price = Product.Price,
                    PrecioCompra = Product.PrecioCompra ?? 0,
                    OriginalPrice = Product.OriginalPrice ?? Product.Price,
                    ImageUrl = "", // Se actualizará con la primera imagen subida si existe
                    Description = Product.Description ?? "",
                    Brand = Product.Brand,
                    Component = Product.Component ?? "",
                    Color = Product.Color ?? "",
                    Availability = Product.Availability ?? "Disponible",
                    SKU = sku,
                    Stock = Product.Stock,
                    StockMinimo = Product.StockMinimo,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    FechaActualizacion = DateTime.UtcNow,
                    Details = new Dictionary<string, string>(),

                    // ?? CAMPOS DE DESCUENTO - Convertir a UTC
                    TieneDescuento = Product.TieneDescuento,
                    PorcentajeDescuento = Product.TieneDescuento ? Product.PorcentajeDescuento : null,
                    FechaInicioDescuento = Product.TieneDescuento && Product.FechaInicioDescuento.HasValue 
                        ? DateTime.SpecifyKind(Product.FechaInicioDescuento.Value, DateTimeKind.Utc) 
                        : null,
                    FechaFinDescuento = Product.TieneDescuento && Product.FechaFinDescuento.HasValue 
                        ? DateTime.SpecifyKind(Product.FechaFinDescuento.Value, DateTimeKind.Utc) 
                        : null,

                    // Especificaciones técnicas
                    Procesador = Product.Procesador ?? "",
                    RAM = Product.RAM ?? "",
                    Almacenamiento = Product.Almacenamiento ?? "",
                    TarjetaGrafica = Product.TarjetaGrafica ?? "",
                    Pantalla = Product.Pantalla ?? "",
                    SistemaOperativo = Product.SistemaOperativo ?? "",
                    DPI = Product.DPI ?? "",
                    Botones = Product.Botones ?? "",
                    Peso = Product.Peso ?? "",
                    Switch = Product.Switch ?? "",
                    Retroiluminacion = Product.Retroiluminacion ?? "",
                    Layout = Product.Layout ?? "",
                    Resolucion = Product.Resolucion ?? "",
                    Frecuencia = Product.Frecuencia ?? "",
                    Panel = Product.Panel ?? "",
                    HDR = Product.HDR ?? "",
                    Conectividad = Product.Conectividad ?? "",
                    Garantia = Product.Garantia ?? ""
                };

                // GUARDAR EN BASE DE DATOS
                _context.Productos.Add(productEntity);
                await _context.SaveChangesAsync();

                _logger.LogInformation("PRODUCTO CREADO ID: {Id}", productEntity.Id);

                // Subir imágenes si existen
                if (Product.ImageFiles != null && Product.ImageFiles.Count > 0)
                {
                    try
                    {
                        await _imageUploadService.UploadImagesAsync(Product.ImageFiles, productEntity.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error subiendo imágenes");
                    }
                }

                TempData["SuccessMessage"] = $"Producto '{productEntity.Name}' creado exitosamente con SKU: {sku}";
                return RedirectToPage("/Admin/Products");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR CREANDO PRODUCTO");
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return Page();
            }
        }

        private async Task<string> GenerarSKUAutomaticoAsync(string categoria, string marca)
        {
            try
            {
                // Prefijo basado en categoría
                var prefijo = categoria.ToUpper() switch
                {
                    string s when s.Contains("LAPTOP") => "LAP",
                    string s when s.Contains("DESKTOP") || s.Contains("COMPUTADOR") => "DES",
                    string s when s.Contains("MONITOR") => "MON",
                    string s when s.Contains("TECLADO") => "TEC",
                    string s when s.Contains("MOUSE") => "MOU",
                    string s when s.Contains("AUDIFONOS") => "AUD",
                    string s when s.Contains("PARLANTE") => "PAR",
                    string s when s.Contains("CABLE") => "CAB",
                    string s when s.Contains("MEMORIA") => "MEM",
                    string s when s.Contains("DISCO") => "HDD",
                    string s when s.Contains("PROCESADOR") => "CPU",
                    string s when s.Contains("TARJETA") => "GPU",
                    _ => "GEN"
                };

                // Código de marca (3 letras)
                var codigoMarca = string.IsNullOrWhiteSpace(marca) ? "GEN" :
                    (marca.Length >= 3 ? marca.Substring(0, 3).ToUpper() : marca.ToUpper().PadRight(3, 'X'));

                // Número secuencial
                var ultimoNumero = await _context.Productos
                    .Where(p => p.SKU != null && p.SKU.StartsWith($"{prefijo}-{codigoMarca}-"))
                    .CountAsync();

                var numeroSecuencial = (ultimoNumero + 1).ToString("D4");

                return $"{prefijo}-{codigoMarca}-{numeroSecuencial}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando SKU automático");
                return $"GEN-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            }
        }
    }

    public class CreateProductViewModel
    {
        [Required(ErrorMessage = "El nombre del producto es requerido")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "La categoría es requerida")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
        public decimal Price { get; set; }

        public decimal? PrecioCompra { get; set; }
        public decimal? OriginalPrice { get; set; }
        public IFormFileCollection? ImageFiles { get; set; }
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "La marca es requerida")]
        public string Brand { get; set; } = string.Empty;

        // CAMPOS OPCIONALES - SIN VALIDACIONES
        public string Component { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Availability { get; set; } = "Disponible";
        public string Details { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int Stock { get; set; } = 10;

        [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo")]
        public int StockMinimo { get; set; } = 5;

        // ?? CAMPOS DE DESCUENTO
        public bool TieneDescuento { get; set; } = false;

        [Range(0, 100, ErrorMessage = "El porcentaje debe estar entre 0 y 100")]
        public decimal? PorcentajeDescuento { get; set; }

        public DateTime? FechaInicioDescuento { get; set; }

        public DateTime? FechaFinDescuento { get; set; }

        // Especificaciones técnicas OPCIONALES
        public string Procesador { get; set; } = string.Empty;
        public string RAM { get; set; } = string.Empty;
        public string Almacenamiento { get; set; } = string.Empty;
        public string TarjetaGrafica { get; set; } = string.Empty;
        public string Pantalla { get; set; } = string.Empty;
        public string SistemaOperativo { get; set; } = string.Empty;
        public string DPI { get; set; } = string.Empty;
        public string Botones { get; set; } = string.Empty;
        public string Peso { get; set; } = string.Empty;
        public string Switch { get; set; } = string.Empty;
        public string Retroiluminacion { get; set; } = string.Empty;
        public string Layout { get; set; } = string.Empty;
        public string Resolucion { get; set; } = string.Empty;
        public string Frecuencia { get; set; } = string.Empty;
        public string Panel { get; set; } = string.Empty;
        public string HDR { get; set; } = string.Empty;
        public string Conectividad { get; set; } = string.Empty;
        public string Garantia { get; set; } = string.Empty;
    }
}
