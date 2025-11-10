using ComputerStore.Data;
using ComputerStore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;
using System.Text;

namespace ComputerStore.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductService> _logger;
        private readonly IMemoryCache _cache;
        private readonly IRedisCacheService _redisCache;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);

        public ProductService(ApplicationDbContext context, ILogger<ProductService> logger, IMemoryCache cache, IRedisCacheService redisCache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
            _redisCache = redisCache;
        }

        // Optimización adicional: Usar Redis Cache para productos destacados
        public async Task<List<Products>> GetProductosDestacadosAsync()
        {
            try
            {
                // Deshabilitar Redis Cache temporalmente en GetProductosDestacadosAsync
                var productos = await _context.Productos
                    .AsNoTracking()
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Price,
                        p.Category,
                        p.Brand,
                        p.ImageUrl,
                        p.Stock
                    })
                    .OrderByDescending(p => p.Price)
                    .Take(3)
                    .ToListAsync();

                var mapped = productos.Select(p => new Products
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Category = p.Category,
                    Brand = p.Brand,
                    ImageUrl = p.ImageUrl,
                    Stock = p.Stock
                }).ToList();

                return mapped;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos destacados");
                return GetDefaultProducts().Take(3).ToList();
            }
        }

        // Deshabilitar Redis Cache temporalmente en GetNuevosLanzamientosAsync
        public async Task<List<Products>> GetNuevosLanzamientosAsync()
        {
            try
            {
                var productos = await _context.Productos
                    .AsNoTracking()
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Price,
                        p.Category,
                        p.Brand,
                        p.ImageUrl,
                        p.CreatedDate
                    })
                    .OrderByDescending(p => p.CreatedDate)
                    .Take(3)
                    .ToListAsync();

                var mapped = productos.Select(p => new Products
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Category = p.Category,
                    Brand = p.Brand,
                    ImageUrl = p.ImageUrl
                }).ToList();

                return mapped;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener nuevos lanzamientos");
                return GetDefaultProducts().Take(3).ToList();
            }
        }

        // Deshabilitar Redis Cache temporalmente en GetMasVendidosAsync
        public async Task<List<Products>> GetMasVendidosAsync()
        {
            try
            {
                var productos = await _context.Productos
                    .AsNoTracking()
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Price,
                        p.Category,
                        p.Brand,
                        p.ImageUrl,
                        p.Stock
                    })
                    .OrderByDescending(p => p.Stock)
                    .Take(3)
                    .ToListAsync();

                var mapped = productos.Select(p => new Products
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Category = p.Category,
                    Brand = p.Brand,
                    ImageUrl = p.ImageUrl,
                    Stock = p.Stock
                }).ToList();

                return mapped;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos más vendidos");
                return GetDefaultProducts().Take(3).ToList();
            }
        }

        // Método optimizado con paginación en base de datos
        public async Task<(List<Products> Products, int TotalCount)> GetProductsPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                var query = _context.Productos
                    .AsNoTracking()
                    .Where(p => p.IsActive);

                // Obtener el total de registros (sin cargar los datos)
                var totalCount = await query.CountAsync();

                // Obtener solo la página solicitada con Query Splitting
                var productos = await query
                    .Include(p => p.Imagenes.OrderBy(i => i.Orden))
                    .AsSplitQuery() // Dividir consultas complejas
                    .OrderByDescending(p => p.CreatedDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var mapped = productos.Select(p => p.ToProducts()).ToList();
                await SetCampaignNamesAsync(mapped);

                return (mapped, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos paginados");
                return (new List<Products>(), 0);
            }
        }

        public async Task<(List<Products> Products, int TotalCount)> GetProductsByCategoryPagedAsync(string category, int pageNumber, int pageSize)
        {
            try
            {
                var query = _context.Productos
                    .AsNoTracking()
                    .Where(p => p.IsActive && p.Category.ToLower() == category.ToLower());

                var totalCount = await query.CountAsync();

                var productos = await query
                    .Include(p => p.Imagenes.OrderBy(i => i.Orden))
                    .OrderByDescending(p => p.CreatedDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var mapped = productos.Select(p => p.ToProducts()).ToList();
                await SetCampaignNamesAsync(mapped);

                return (mapped, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos por categoría paginados");
                return (new List<Products>(), 0);
            }
        }

        public async Task<(List<Products> Products, int TotalCount)> SearchProductsPagedAsync(string searchTerm, int pageNumber, int pageSize)
        {
            try
            {
                var query = _context.Productos
                    .AsNoTracking()
                    .Where(p => p.IsActive &&
                           (p.Name.Contains(searchTerm) ||
                            p.Description.Contains(searchTerm) ||
                            p.Brand.Contains(searchTerm) ||
                            p.Category.Contains(searchTerm)));

                var totalCount = await query.CountAsync();

                var productos = await query
                    .Include(p => p.Imagenes.OrderBy(i => i.Orden))
                    .OrderByDescending(p => p.CreatedDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var mapped = productos.Select(p => p.ToProducts()).ToList();
                await SetCampaignNamesAsync(mapped);

                return (mapped, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar productos paginados");
                return (new List<Products>(), 0);
            }
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            try
            {
                return await _cache.GetOrCreateAsync("categorias_lista", async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30); // Aumentar expiración

                    return await _context.Productos
                        .AsNoTracking()
                        .Where(p => p.IsActive)
                        .Select(p => p.Category)
                        .Distinct()
                        .OrderBy(c => c)
                        .ToListAsync();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías");
                return new List<string>();
            }
        }

        public async Task<List<string>> GetBrandsAsync()
        {
            try
            {
                return await _cache.GetOrCreateAsync("marcas_lista", async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);

                    return await _context.Productos
                        .AsNoTracking()
                        .Where(p => p.IsActive)
                        .Select(p => p.Brand)
                        .Distinct()
                        .OrderBy(b => b)
                        .ToListAsync();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener marcas");
                return new List<string>();
            }
        }

        public async Task<List<string>> GetColorsAsync()
        {
            try
            {
                return await _cache.GetOrCreateAsync("colores_lista", async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);

                    return await _context.Productos
                        .AsNoTracking()
                        .Where(p => p.IsActive)
                        .Select(p => p.Color)
                        .Distinct()
                        .OrderBy(c => c)
                        .ToListAsync();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener colores");
                return new List<string>();
            }
        }

        // Mantener método legacy para compatibilidad (pero optimizado)
        public async Task<List<Products>> GetAllProductsAsync()
        {
            try
            {
                // Usar paginación grande pero limitada
                var (products, _) = await GetProductsPagedAsync(1, 100);
                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos de la base de datos");
                return GetDefaultProducts();
            }
        }

        public async Task<Products?> GetProductByIdAsync(int id)
        {
            try
            {
                var producto = await _context.Productos
                    .AsNoTracking()
                    .Include(p => p.Imagenes.OrderBy(i => i.Orden))
                    .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

                var mapped = producto?.ToProducts();
                if (mapped != null)
                {
                    await SetCampaignNameAsync(mapped);
                }
                return mapped;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener producto {ProductId}", id);
                return null;
            }
        }

        public async Task<List<Products>> GetProductsByCategoryAsync(string category)
        {
            try
            {
                var productos = await _context.Productos
                    .AsNoTracking()
                    .Include(p => p.Imagenes.OrderBy(i => i.Orden))
                    .Where(p => p.IsActive && p.Category.ToLower() == category.ToLower())
                    .OrderByDescending(p => p.CreatedDate)
                    .ToListAsync();

                var mapped = productos.Select(p => p.ToProducts()).ToList();
                await SetCampaignNamesAsync(mapped);
                return mapped;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos por categoría {Category}", category);
                return new List<Products>();
            }
        }

        public async Task<List<Products>> SearchProductsAsync(string searchTerm)
        {
            try
            {
                var productos = await _context.Productos
                    .AsNoTracking()
                    .Include(p => p.Imagenes.OrderBy(i => i.Orden))
                    .Where(p => p.IsActive &&
                           (p.Name.Contains(searchTerm) ||
                            p.Description.Contains(searchTerm) ||
                            p.Brand.Contains(searchTerm) ||
                            p.Category.Contains(searchTerm)))
                    .OrderByDescending(p => p.CreatedDate)
                    .ToListAsync();

                var mapped = productos.Select(p => p.ToProducts()).ToList();
                await SetCampaignNamesAsync(mapped);
                return mapped;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar productos con término {SearchTerm}", searchTerm);
                return new List<Products>();
            }
        }

        private static string NormalizeCategory(string? category)
        {
            if (string.IsNullOrWhiteSpace(category)) return string.Empty;
            var text = category.Trim().ToLowerInvariant();
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            var result = sb.ToString().Normalize(NormalizationForm.FormC);
            return result switch
            {
                "computadoras de escritorio" or "computadores" or "computadoras" or "pc" or "escritorio" => "computadores",
                "perifericos" or "periferico" => "perifericos",
                "portatiles" or "laptop" or "laptops" => "laptops",
                "monitores" or "monitor" => "monitores",
                "componentes" or "componente" => "componentes",
                "accesorios" or "accesorio" => "accesorios",
                _ => result
            };
        }

        private async Task SetCampaignNamesAsync(List<Products> products)
        {
            // Implementación del método para asociar nombres de campaña
            if (products.Count ==0) return;

            var now = DateTime.UtcNow;
            var campanasActivas = await _context.MarketingCosts
                .AsNoTracking()
                .Where(c => c.AplicaDescuento &&
                       (c.DescuentoActivo ||
                        ((!c.FechaInicioDescuento.HasValue || now >= c.FechaInicioDescuento.Value) &&
                         (!c.FechaFinDescuento.HasValue || now <= c.FechaFinDescuento.Value))))
                .OrderByDescending(c => c.FechaInicioDescuento)
                .ThenByDescending(c => c.Fecha)
                .ToListAsync();

            foreach (var p in products)
            {
                var catNorm = NormalizeCategory(p.Category);
                var campana = campanasActivas.FirstOrDefault(c =>
                    string.Equals(c.CategoriaDescuento ?? string.Empty, "Todos", StringComparison.OrdinalIgnoreCase) ||
                    NormalizeCategory(c.CategoriaDescuento) == catNorm);

                if (campana != null)
                {
                    p.CampaignName = campana.Campana;
                    // rellenar OriginalPrice si el producto viene con precio reducido y sin original
                    if (!p.OriginalPrice.HasValue)
                    {
                        // buscar el precio normal en DB por si aplica descuento a nivel entidad
                        var db = await _context.Productos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == p.Id);
                        if (db != null && db.TieneDescuento && db.PorcentajeDescuento.HasValue)
                        {
                            p.OriginalPrice = db.Price;
                        }
                    }
                }
            }
        }

        private List<Products> GetDefaultProducts()
        {
            return new List<Products>
            {
                new Products
                {
                    Id =1,
                    Name = "Laptop Gaming MSI",
                    Category = "Laptops",
                    Price =3500000,
                    ImageUrl = "/wwwroot/images/laptop-msi.jpg",
                    Description = "Laptop gaming de alto rendimiento",
                    Brand = "MSI",
                    Component = "Gaming",
                    Color = "Negro",
                    Availability = "Disponible",
                    Details = new Dictionary<string, string>
                    {
                        {"Procesador", "Intel Core i7"},
                        {"RAM", "16GB"},
                        {"Almacenamiento", "1TB SSD"}
                    }
                },
                new Products
                {
                    Id =2,
                    Name = "Mouse Logitech G502",
                    Category = "Periféricos",
                    Price =180000,
                    ImageUrl = "/wwwroot/images/mouse-logitech.jpg",
                    Description = "Mouse gaming de precisión",
                    Brand = "Logitech",
                    Component = "Gaming",
                    Color = "Negro",
                    Availability = "Disponible",
                    Details = new Dictionary<string, string>
                    {
                        {"DPI", "25600"},
                        {"Botones", "11"},
                        {"Peso", "121g"}
                    }
                },
                new Products
                {
                    Id =3,
                    Name = "Teclado Mecánico Corsair",
                    Category = "Periféricos",
                    Price =420000,
                    ImageUrl = "/wwwroot/images/teclado-corsair.jpg",
                    Description = "Teclado mecánico para gaming",
                    Brand = "Corsair",
                    Component = "Gaming",
                    Color = "Negro",
                    Availability = "Disponible",
                    Details = new Dictionary<string, string>
                    {
                        {"Switch", "Cherry MX Red"},
                        {"Retroiluminación", "RGB"},
                        {"Layout", "Español"}
                    }
                }
            };
        }

        // Permitir a otros componentes actualizar stock de forma atómica
        public async Task<bool> AdjustStockAsync(int productId, int delta)
        {
            try
            {
                var entity = await _context.Productos.AsTracking().FirstOrDefaultAsync(p => p.Id == productId);
                if (entity == null) return false;
                var nuevo = entity.Stock + delta;
                if (nuevo <0) nuevo =0;
                entity.Stock = nuevo;
                entity.FechaActualizacion = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                InvalidateProductCache();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ajustando stock {ProductId} delta {Delta}", productId, delta);
                return false;
            }
        }

        // Hacer visible para invocación por reflexión desde Admin/Products
        internal void InvalidateProductCache()
        {
            _cache.Remove("productos_destacados");
            _cache.Remove("productos_nuevos");
            _cache.Remove("productos_mas_vendidos");
            _cache.Remove("categorias_lista");
            _cache.Remove("marcas_lista");
            _cache.Remove("colores_lista");
        }

        private async Task SetCampaignNameAsync(Products product)
        {
            await SetCampaignNamesAsync(new List<Products> { product });
        }

        private int GetNextSequentialNumber(string category)
        {
            try
            {
                var categoryPrefix = GetCategoryPrefix(category);
                var lastProduct = _context.Productos
                    .Where(p => p.SKU != null && p.SKU.StartsWith(categoryPrefix))
                    .OrderByDescending(p => p.Id)
                    .FirstOrDefault();

                if (lastProduct?.SKU != null)
                {
                    var skuParts = lastProduct.SKU.Split('-');
                    if (skuParts.Length >=3 && int.TryParse(skuParts[2], out int lastNumber))
                    {
                        return lastNumber +1;
                    }
                }
                return 1;
            }
            catch
            {
                return 1;
            }
        }

        private string GenerateAutomaticSKU(Products product)
        {
            try
            {
                var categoryPrefix = GetCategoryPrefix(product.Category);
                var brandPrefix = GetBrandPrefix(product.Brand);
                var nextNumber = GetNextSequentialNumber(product.Category);
                return $"{categoryPrefix}-{brandPrefix}-{nextNumber:D6}";
            }
            catch
            {
                return $"SKU-{DateTime.UtcNow.Ticks.ToString().Substring(8)}";
            }
        }

        private string GetCategoryPrefix(string category)
        {
            return category.ToUpper() switch
            {
                "LAPTOPS" => "LAP",
                "COMPUTADORES" => "COM",
                "PERIFÉRICOS" => "PER",
                "MONITORES" => "MON",
                "COMPONENTES" => "CMP",
                "ACCESORIOS" => "ACC",
                _ => "GEN"
            };
        }

        private string GetBrandPrefix(string brand)
        {
            if (string.IsNullOrEmpty(brand) || brand.Length < 3)
                return "GEN";
            return brand.ToUpper().Substring(0, Math.Min(3, brand.Length));
        }

        public async Task<Products> UpdateProductAsync(Products product)
        {
            try
            {
                var existingProduct = await _context.Productos.FindAsync(product.Id);
                if (existingProduct != null)
                {
                    existingProduct.Name = product.Name;
                    existingProduct.Category = product.Category;
                    existingProduct.Price = product.Price;
                    existingProduct.OriginalPrice = product.OriginalPrice; // persistir precio original
                    existingProduct.ImageUrl = product.ImageUrl;
                    existingProduct.Description = product.Description;
                    existingProduct.Brand = product.Brand;
                    existingProduct.Component = product.Component ?? "";
                    existingProduct.Color = product.Color ?? "";
                    existingProduct.Availability = product.Availability ?? "Disponible";

                    // NUEVO: persistir stock
                    existingProduct.Stock = product.Stock;

                    // Generar SKU automático si falta
                    if (string.IsNullOrWhiteSpace(existingProduct.SKU))
                    {
                        existingProduct.SKU = GenerateAutomaticSKU(product);
                    }

                    if (product.Details != null)
                    {
                        existingProduct.Details = product.Details;
                    }

                    existingProduct.FechaActualizacion = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                    InvalidateProductCache();
                    var mapped = existingProduct.ToProducts();
                    await SetCampaignNameAsync(mapped);
                    return mapped;
                }
                throw new Exception("Producto no encontrado");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar producto: {ex.Message}");
            }
        }

        public async Task<Products> CreateProductAsync(Products product)
        {
            try
            {
                var productEntity = new ProductEntity
                {
                    Name = product.Name,
                    Category = product.Category,
                    Price = product.Price,
                    OriginalPrice = product.OriginalPrice,
                    ImageUrl = product.ImageUrl,
                    Description = product.Description,
                    Brand = product.Brand,
                    Component = product.Component,
                    Color = product.Color,
                    Availability = product.Availability,
                    Details = product.Details,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    Stock = product.Stock,
                    StockMinimo =5
                };

                // Generar SKU si no se proporcionó
                productEntity.SKU = GenerateAutomaticSKU(product);

                _context.Productos.Add(productEntity);
                await _context.SaveChangesAsync();

                // Invalidar caché
                InvalidateProductCache();

                var mapped = productEntity.ToProducts();
                await SetCampaignNameAsync(mapped);
                return mapped;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear producto");
                throw new Exception($"Error al crear producto: {ex.Message}");
            }
        }

        public async Task<bool> DeleteProductAsync(Products product)
        {
            return await DeleteProductAsync(product.Id);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            try
            {
                var product = await _context.Productos.FindAsync(id);
                if (product != null)
                {
                    _context.Productos.Remove(product);
                    await _context.SaveChangesAsync();
                    InvalidateProductCache();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar producto: {ex.Message}");
            }
        }
    }
}