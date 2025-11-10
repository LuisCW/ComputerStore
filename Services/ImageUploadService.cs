using ComputerStore.Data;
using ComputerStore.Models;
using Microsoft.EntityFrameworkCore;

namespace ComputerStore.Services
{
    public interface IImageUploadService
    {
        Task<List<string>> UploadImagesAsync(IFormFileCollection files, int productoId);
        Task<string> UploadSingleImageAsync(IFormFile file, int productoId);
        Task<bool> DeleteImageAsync(string imageUrl);
        Task<List<ProductoImagen>> GetProductImagesAsync(int productoId);
        Task<bool> SetMainImageAsync(int productoId, int imagenId);
    }

    public class ImageUploadService : IImageUploadService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ImageUploadService> _logger;

        public ImageUploadService(ApplicationDbContext context, IWebHostEnvironment environment, ILogger<ImageUploadService> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        public async Task<List<string>> UploadImagesAsync(IFormFileCollection files, int productoId)
        {
            var uploadedUrls = new List<string>();
            
            if (files == null || files.Count == 0)
                return uploadedUrls;

            try
            {
                // Crear directorio si no existe
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "products", productoId.ToString());
                Directory.CreateDirectory(uploadsPath);

                // Obtener el orden máximo actual para este producto
                var maxOrden = await _context.ProductoImagenes
                    .Where(pi => pi.ProductoId == productoId)
                    .MaxAsync(pi => (int?)pi.Orden) ?? 0;

                foreach (var file in files)
                {
                    if (file.Length > 0 && IsValidImageFile(file))
                    {
                        var url = await UploadSingleFileAsync(file, productoId, uploadsPath, ++maxOrden);
                        if (!string.IsNullOrEmpty(url))
                        {
                            uploadedUrls.Add(url);
                        }
                    }
                }

                _logger.LogInformation("Subidas {Count} imágenes para producto {ProductoId}", uploadedUrls.Count, productoId);
                return uploadedUrls;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subiendo imágenes para producto {ProductoId}", productoId);
                throw;
            }
        }

        public async Task<string> UploadSingleImageAsync(IFormFile file, int productoId)
        {
            if (file == null || file.Length == 0 || !IsValidImageFile(file))
                return string.Empty;

            try
            {
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "products", productoId.ToString());
                Directory.CreateDirectory(uploadsPath);

                var maxOrden = await _context.ProductoImagenes
                    .Where(pi => pi.ProductoId == productoId)
                    .MaxAsync(pi => (int?)pi.Orden) ?? 0;

                return await UploadSingleFileAsync(file, productoId, uploadsPath, maxOrden + 1);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subiendo imagen individual para producto {ProductoId}", productoId);
                throw;
            }
        }

        private async Task<string> UploadSingleFileAsync(IFormFile file, int productoId, string uploadsPath, int orden)
        {
            try
            {
                // Generar nombre único para el archivo
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Guardar archivo físico
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Generar URL relativa
                var imageUrl = $"/uploads/products/{productoId}/{fileName}";

                // Verificar si es la primera imagen (será principal por defecto)
                var esPrimeraImagen = !await _context.ProductoImagenes.AnyAsync(pi => pi.ProductoId == productoId);

                // Guardar en base de datos
                var productoImagen = new ProductoImagen
                {
                    ProductoId = productoId,
                    ImagenUrl = imageUrl,
                    NombreArchivo = fileName,
                    AltText = Path.GetFileNameWithoutExtension(file.FileName),
                    Orden = orden,
                    EsPrincipal = esPrimeraImagen,
                    FechaCreacion = DateTime.UtcNow
                };

                _context.ProductoImagenes.Add(productoImagen);
                await _context.SaveChangesAsync();

                // Si es la primera imagen, actualizar también el campo ImageUrl del producto para compatibilidad
                if (esPrimeraImagen)
                {
                    var producto = await _context.Productos.FindAsync(productoId);
                    if (producto != null)
                    {
                        producto.ImageUrl = imageUrl;
                        await _context.SaveChangesAsync();
                    }
                }

                return imageUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando archivo {FileName} para producto {ProductoId}", file.FileName, productoId);
                return string.Empty;
            }
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                var imagen = await _context.ProductoImagenes.FirstOrDefaultAsync(pi => pi.ImagenUrl == imageUrl);
                if (imagen == null)
                    return false;

                // Eliminar archivo físico
                var filePath = Path.Combine(_environment.WebRootPath, imageUrl.TrimStart('/'));
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                // Eliminar de base de datos
                _context.ProductoImagenes.Remove(imagen);

                // Si era la imagen principal, establecer otra como principal
                if (imagen.EsPrincipal)
                {
                    var otraImagen = await _context.ProductoImagenes
                        .Where(pi => pi.ProductoId == imagen.ProductoId && pi.Id != imagen.Id)
                        .OrderBy(pi => pi.Orden)
                        .FirstOrDefaultAsync();

                    if (otraImagen != null)
                    {
                        otraImagen.EsPrincipal = true;
                        
                        // Actualizar también el producto
                        var producto = await _context.Productos.FindAsync(imagen.ProductoId);
                        if (producto != null)
                        {
                            producto.ImageUrl = otraImagen.ImagenUrl;
                        }
                    }
                    else
                    {
                        // No quedan imágenes, limpiar el producto
                        var producto = await _context.Productos.FindAsync(imagen.ProductoId);
                        if (producto != null)
                        {
                            producto.ImageUrl = string.Empty;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando imagen {ImageUrl}", imageUrl);
                return false;
            }
        }

        public async Task<List<ProductoImagen>> GetProductImagesAsync(int productoId)
        {
            try
            {
                return await _context.ProductoImagenes
                    .Where(pi => pi.ProductoId == productoId)
                    .OrderBy(pi => pi.Orden)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo imágenes para producto {ProductoId}", productoId);
                return new List<ProductoImagen>();
            }
        }

        public async Task<bool> SetMainImageAsync(int productoId, int imagenId)
        {
            try
            {
                // Quitar principal de todas las imágenes del producto
                var imagenesProducto = await _context.ProductoImagenes
                    .Where(pi => pi.ProductoId == productoId)
                    .ToListAsync();

                foreach (var img in imagenesProducto)
                {
                    img.EsPrincipal = img.Id == imagenId;
                }

                // Actualizar imagen principal en el producto
                var imagenPrincipal = imagenesProducto.FirstOrDefault(i => i.Id == imagenId);
                if (imagenPrincipal != null)
                {
                    var producto = await _context.Productos.FindAsync(productoId);
                    if (producto != null)
                    {
                        producto.ImageUrl = imagenPrincipal.ImagenUrl;
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error estableciendo imagen principal {ImagenId} para producto {ProductoId}", imagenId, productoId);
                return false;
            }
        }

        private static bool IsValidImageFile(IFormFile file)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".avif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            return allowedExtensions.Contains(extension) && 
                   file.Length > 0 && 
                   file.Length <= 10 * 1024 * 1024; // 10MB máximo
        }
    }
}