using ComputerStore.Data;
using ComputerStore.Models;
using ComputerStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ComputerStore.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class EditProductModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly IImageUploadService _imageUploadService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EditProductModel> _logger;

        [BindProperty]
        public ProductEditViewModel Product { get; set; } = new ProductEditViewModel();

        public List<ProductoImagen> ProductImages { get; set; } = new List<ProductoImagen>();
        public string ReturnUrl { get; set; } = "/Admin/Products";

        public EditProductModel(IProductService productService, IImageUploadService imageUploadService, 
            ApplicationDbContext context, ILogger<EditProductModel> logger)
        {
            _productService = productService;
            _imageUploadService = imageUploadService;
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(int id, string returnUrl = "/Admin/Products")
        {
            ReturnUrl = returnUrl;

            // Obtener producto con imágenes
            var productEntity = await _context.Productos
                .Include(p => p.Imagenes.OrderBy(i => i.Orden))
                .FirstOrDefaultAsync(p => p.Id == id);

            if (productEntity == null)
            {
                TempData["ErrorMessage"] = "Producto no encontrado.";
                return RedirectToPage("/Admin/Products");
            }

            // Mapear a ViewModel
            Product = new ProductEditViewModel
            {
                Id = productEntity.Id,
                Name = productEntity.Name,
                Category = productEntity.Category,
                Price = productEntity.Price,
                PrecioCompra = productEntity.PrecioCompra,
                OriginalPrice = productEntity.OriginalPrice,
                ImageUrl = productEntity.ImageUrl ?? "",
                Description = productEntity.Description ?? "",
                Brand = productEntity.Brand,
                Component = productEntity.Component ?? "",
                Color = productEntity.Color ?? "",
                Availability = productEntity.Availability ?? "Disponible",
                Stock = productEntity.Stock,
                StockMinimo = productEntity.StockMinimo,
                SKU = productEntity.SKU ?? "",
                
                // ?? CAMPOS DE DESCUENTO
                TieneDescuento = productEntity.TieneDescuento,
                PorcentajeDescuento = productEntity.PorcentajeDescuento,
                FechaInicioDescuento = productEntity.FechaInicioDescuento,
                FechaFinDescuento = productEntity.FechaFinDescuento,
                
                // Especificaciones técnicas específicas
                Procesador = productEntity.Procesador ?? "",
                RAM = productEntity.RAM ?? "",
                Almacenamiento = productEntity.Almacenamiento ?? "",
                TarjetaGrafica = productEntity.TarjetaGrafica ?? "",
                Pantalla = productEntity.Pantalla ?? "",
                SistemaOperativo = productEntity.SistemaOperativo ?? "",
                Conectividad = productEntity.Conectividad ?? "",
                Garantia = productEntity.Garantia ?? "",
                
                // Características adicionales
                DPI = productEntity.DPI ?? "",
                Botones = productEntity.Botones ?? "",
                Peso = productEntity.Peso ?? "",
                Switch = productEntity.Switch ?? "",
                Retroiluminacion = productEntity.Retroiluminacion ?? "",
                Layout = productEntity.Layout ?? "",
                Resolucion = productEntity.Resolucion ?? "",
                Frecuencia = productEntity.Frecuencia ?? "",
                Panel = productEntity.Panel ?? "",
                HDR = productEntity.HDR ?? ""
            };

            // Cargar imágenes del producto
            ProductImages = productEntity.Imagenes?.ToList() ?? new List<ProductoImagen>();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger.LogInformation("[EditProduct] POST recibido para Id={Id}", Product?.Id);
                _logger.LogInformation("[EditProduct] Files(Product.ImageFiles)={Count}", Product?.ImageFiles?.Count ?? 0);
                if (Request.HasFormContentType)
                {
                    _logger.LogInformation("[EditProduct] Request.Form.Files={Count}", Request.Form?.Files?.Count ?? 0);
                }
                 // Validaciones básicas SOLO campos obligatorios
                 if (string.IsNullOrWhiteSpace(Product.Name))
                 {
                     TempData["ErrorMessage"] = "El nombre del producto es requerido";
                     await LoadProductImages();
                     return Page();
                 }

                if (string.IsNullOrWhiteSpace(Product.Brand))
                {
                    TempData["ErrorMessage"] = "La marca es requerida";
                    await LoadProductImages();
                    return Page();
                }

                if (string.IsNullOrWhiteSpace(Product.Category))
                {
                    TempData["ErrorMessage"] = "La categoría es requerida";
                    await LoadProductImages();
                    return Page();
                }

                if (Product.Price <= 0)
                {
                    TempData["ErrorMessage"] = "El precio debe ser mayor a 0";
                    await LoadProductImages();
                    return Page();
                }

                // Obtener el producto existente CON TRACKING EXPLÍCITO
                var existingProduct = await _context.Productos
                    .FirstOrDefaultAsync(p => p.Id == Product.Id);
                
                if (existingProduct == null)
                {
                    TempData["ErrorMessage"] = "Producto no encontrado.";
                    return Redirect(ReturnUrl);
                }

                _logger.LogInformation("Stock ANTES de actualizar: {StockAnterior}", existingProduct.Stock);

                // Actualizar campos básicos
                existingProduct.Name = Product.Name;
                existingProduct.Category = Product.Category;
                existingProduct.Price = Product.Price;
                existingProduct.PrecioCompra = Product.PrecioCompra;
                existingProduct.OriginalPrice = Product.OriginalPrice;
                existingProduct.Description = Product.Description ?? "";
                existingProduct.Brand = Product.Brand;
                existingProduct.Component = Product.Component ?? "";
                existingProduct.Color = Product.Color ?? "";
                existingProduct.Availability = Product.Availability ?? "Disponible";
                existingProduct.Stock = Product.Stock;
                existingProduct.StockMinimo = Product.StockMinimo;
                existingProduct.SKU = Product.SKU ?? "";
                existingProduct.FechaActualizacion = DateTime.UtcNow;

                _logger.LogInformation("Stock DESPUÉS de actualizar en memoria: {StockNuevo}", existingProduct.Stock);

                // Marcar explícitamente como modificado
                _context.Entry(existingProduct).State = EntityState.Modified;

                // Descuentos (UTC)
                existingProduct.TieneDescuento = Product.TieneDescuento;
                existingProduct.PorcentajeDescuento = Product.TieneDescuento ? Product.PorcentajeDescuento : null;
                existingProduct.FechaInicioDescuento = Product.TieneDescuento && Product.FechaInicioDescuento.HasValue
                    ? DateTime.SpecifyKind(Product.FechaInicioDescuento.Value, DateTimeKind.Utc)
                    : null;
                existingProduct.FechaFinDescuento = Product.TieneDescuento && Product.FechaFinDescuento.HasValue
                    ? DateTime.SpecifyKind(Product.FechaFinDescuento.Value, DateTimeKind.Utc)
                    : null;

                // Especificaciones técnicas
                existingProduct.Procesador = Product.Procesador ?? "";
                existingProduct.RAM = Product.RAM ?? "";
                existingProduct.Almacenamiento = Product.Almacenamiento ?? "";
                existingProduct.TarjetaGrafica = Product.TarjetaGrafica ?? "";
                existingProduct.Pantalla = Product.Pantalla ?? "";
                existingProduct.SistemaOperativo = Product.SistemaOperativo ?? "";
                existingProduct.DPI = Product.DPI ?? "";
                existingProduct.Botones = Product.Botones ?? "";
                existingProduct.Peso = Product.Peso ?? "";
                existingProduct.Switch = Product.Switch ?? "";
                existingProduct.Retroiluminacion = Product.Retroiluminacion ?? "";
                existingProduct.Layout = Product.Layout ?? "";
                existingProduct.Resolucion = Product.Resolucion ?? "";
                existingProduct.Frecuencia = Product.Frecuencia ?? "";
                existingProduct.Panel = Product.Panel ?? "";
                existingProduct.HDR = Product.HDR ?? "";
                existingProduct.Conectividad = Product.Conectividad ?? "";
                existingProduct.Garantia = Product.Garantia ?? "";

                // Subir nuevas imágenes: tomar primero los archivos crudos del formulario
                IFormFileCollection? filesToUpload = null;
                if (Request.HasFormContentType && Request.Form.Files?.Count > 0)
                {
                    filesToUpload = Request.Form.Files; // preferir los archivos del formulario
                }
                else if (Product.ImageFiles != null && Product.ImageFiles.Count > 0)
                {
                    filesToUpload = Product.ImageFiles; // respaldo
                }

                if (filesToUpload != null && filesToUpload.Count > 0)
                {
                    try
                    {
                        _logger.LogInformation("[EditProduct] Subiendo {Count} archivo(s) para producto {ProductId}", filesToUpload.Count, existingProduct.Id);
                        await _imageUploadService.UploadImagesAsync(filesToUpload, existingProduct.Id);
                        TempData["SuccessMessage"] = $"Se subieron {filesToUpload.Count} imagen(es)";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error subiendo imágenes para producto {ProductId}", existingProduct.Id);
                        TempData["WarningMessage"] = "Producto actualizado, pero hubo un error al subir algunas imágenes";
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Cambios guardados en base de datos. Verificando...");
                
                // Verificar que se guardó correctamente
                var verificacion = await _context.Productos
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == Product.Id);
                
                _logger.LogInformation("Stock VERIFICADO en BD: {StockVerificado}", verificacion?.Stock ?? -1);

                TempData["SuccessMessage"] = $"Producto '{Product.Name}' actualizado exitosamente. Stock: {verificacion?.Stock ?? Product.Stock}";
                return Redirect(ReturnUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar producto {ProductId}", Product.Id);
                TempData["ErrorMessage"] = $"Error al actualizar producto: {ex.Message}";
                await LoadProductImages();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteImageAsync(int imageId)
        {
            try
            {
                var image = await _context.ProductoImagenes.FindAsync(imageId);
                if (image != null)
                {
                    await _imageUploadService.DeleteImageAsync(image.ImagenUrl);
                    TempData["SuccessMessage"] = "Imagen eliminada exitosamente";
                }
                else
                {
                    TempData["ErrorMessage"] = "Imagen no encontrada";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar imagen {ImageId}", imageId);
                TempData["ErrorMessage"] = "Error al eliminar imagen";
            }

            return RedirectToPage(new { id = Product.Id });
        }

        public async Task<IActionResult> OnPostSetMainImageAsync(int imageId, int productId)
        {
            try
            {
                await _imageUploadService.SetMainImageAsync(productId, imageId);
                TempData["SuccessMessage"] = "Imagen principal actualizada";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al establecer imagen principal");
                TempData["ErrorMessage"] = "Error al actualizar imagen principal";
            }

            return RedirectToPage(new { id = productId });
        }

        private async Task LoadProductImages()
        {
            ProductImages = await _imageUploadService.GetProductImagesAsync(Product.Id);
        }
    }

    public class ProductEditViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? PrecioCompra { get; set; }
        public decimal? OriginalPrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        
        // ?? NUEVAS PROPIEDADES PARA MÚLTIPLES IMÁGENES
        public IFormFileCollection? ImageFiles { get; set; }
        
        public string Description { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Component { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Availability { get; set; } = "Disponible";
        
        // ?? NUEVAS PROPIEDADES PARA STOCK
        public int Stock { get; set; }
        public int StockMinimo { get; set; }
        public string SKU { get; set; } = string.Empty;

        // ?? CAMPOS DE DESCUENTO
        public bool TieneDescuento { get; set; } = false;
        public decimal? PorcentajeDescuento { get; set; }
        public DateTime? FechaInicioDescuento { get; set; }
        public DateTime? FechaFinDescuento { get; set; }

        // Especificaciones para Laptops/Computadores
        public string Procesador { get; set; } = string.Empty;
        public string RAM { get; set; } = string.Empty;
        public string Almacenamiento { get; set; } = string.Empty;
        public string TarjetaGrafica { get; set; } = string.Empty;
        public string Pantalla { get; set; } = string.Empty;
        public string SistemaOperativo { get; set; } = string.Empty;

        // Especificaciones para Mouse
        public string DPI { get; set; } = string.Empty;
        public string Botones { get; set; } = string.Empty;
        public string Peso { get; set; } = string.Empty;

        // Especificaciones para Teclados
        public string Switch { get; set; } = string.Empty;
        public string Retroiluminacion { get; set; } = string.Empty;
        public string Layout { get; set; } = string.Empty;

        // Especificaciones para Monitores
        public string Resolucion { get; set; } = string.Empty;
        public string Frecuencia { get; set; } = string.Empty;
        public string Panel { get; set; } = string.Empty;
        public string HDR { get; set; } = string.Empty;

        // Campos comunes
        public string Conectividad { get; set; } = string.Empty;
        public string Garantia { get; set; } = string.Empty;
    }
}