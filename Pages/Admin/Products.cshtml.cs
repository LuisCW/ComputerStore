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
    public class ProductsModel : PageModel
    {
        private readonly IProductService _productService;
        private readonly IImageUploadService _imageUploadService;
        private readonly IExcelExportService _excelExportService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductsModel> _logger;

        public List<ProductEntity> Products { get; set; } = new List<ProductEntity>();
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; } // ?? AGREGADO
        public List<string> Categories { get; set; } = new List<string>();
        public List<string> Brands { get; set; } = new List<string>(); // ?? AGREGADO

        // Filtros mejorados
        public string CurrentCategory { get; set; } = "all";
        public string CurrentBrand { get; set; } = "all"; // ?? AGREGADO
        public string CurrentSearch { get; set; } = "";
        public string CurrentSort { get; set; } = "date";
        public bool ShowInactiveOnly { get; set; } = false;
        public bool ShowLowStockOnly { get; set; } = false; // ?? AGREGADO
        public decimal? MinPrice { get; set; } // ?? AGREGADO
        public decimal? MaxPrice { get; set; } // ?? AGREGADO

        public ProductsModel(IProductService productService, IImageUploadService imageUploadService, 
            IExcelExportService excelExportService, ApplicationDbContext context, ILogger<ProductsModel> logger)
        {
            _productService = productService;
            _imageUploadService = imageUploadService;
            _excelExportService = excelExportService;
            _context = context;
            _logger = logger;
        }

        public async Task OnGetAsync(string? category = null, string? brand = null, string? search = null, 
            string? sortBy = null, bool inactiveOnly = false, bool lowStockOnly = false, 
            decimal? minPrice = null, decimal? maxPrice = null)
        {
            try
            {
                // Obtener todos los productos desde la base de datos
                var query = _context.Productos.AsQueryable();

                // ?? FILTROS MEJORADOS
                if (!string.IsNullOrEmpty(category) && category != "all")
                {
                    query = query.Where(p => p.Category.ToLower() == category.ToLower());
                }

                if (!string.IsNullOrEmpty(brand) && brand != "all")
                {
                    query = query.Where(p => p.Brand.ToLower() == brand.ToLower());
                }

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p => p.Name.Contains(search) || 
                                           p.Brand.Contains(search) ||
                                           p.Description.Contains(search) ||
                                           (p.SKU != null && p.SKU.Contains(search)));
                }

                if (minPrice.HasValue)
                {
                    query = query.Where(p => p.Price >= minPrice.Value);
                }

                if (maxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= maxPrice.Value);
                }

                if (inactiveOnly)
                {
                    query = query.Where(p => !p.IsActive);
                }
                else
                {
                    query = query.Where(p => p.IsActive);
                }

                if (lowStockOnly)
                {
                    query = query.Where(p => p.Stock <= p.StockMinimo);
                }

                // ?? ORDENAMIENTO MEJORADO
                query = sortBy?.ToLower() switch
                {
                    "name" => query.OrderBy(p => p.Name),
                    "name_desc" => query.OrderByDescending(p => p.Name),
                    "price" => query.OrderBy(p => p.Price),
                    "price_desc" => query.OrderByDescending(p => p.Price),
                    "category" => query.OrderBy(p => p.Category),
                    "brand" => query.OrderBy(p => p.Brand),
                    "stock" => query.OrderBy(p => p.Stock),
                    "stock_desc" => query.OrderByDescending(p => p.Stock),
                    "date" => query.OrderByDescending(p => p.CreatedDate),
                    "date_asc" => query.OrderBy(p => p.CreatedDate),
                    _ => query.OrderByDescending(p => p.CreatedDate)
                };

                Products = await query.ToListAsync();

                // ?? ESTADÍSTICAS MEJORADAS
                TotalProducts = await _context.Productos.CountAsync(p => p.IsActive);
                ActiveProducts = TotalProducts;
                LowStockProducts = await _context.Productos.CountAsync(p => p.IsActive && p.Stock <= p.StockMinimo);
                OutOfStockProducts = await _context.Productos.CountAsync(p => p.IsActive && p.Stock == 0);
                
                Categories = await _context.Productos
                    .Where(p => p.IsActive)
                    .Select(p => p.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                Brands = await _context.Productos
                    .Where(p => p.IsActive)
                    .Select(p => p.Brand)
                    .Distinct()
                    .OrderBy(b => b)
                    .ToListAsync();

                // Configurar filtros para la vista
                CurrentCategory = category ?? "all";
                CurrentBrand = brand ?? "all";
                CurrentSearch = search ?? "";
                CurrentSort = sortBy ?? "date";
                ShowInactiveOnly = inactiveOnly;
                ShowLowStockOnly = lowStockOnly;
                MinPrice = minPrice;
                MaxPrice = maxPrice;

                _logger.LogInformation("Cargados {Count} productos con filtros aplicados", Products.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar productos");
                TempData["ErrorMessage"] = $"Error al cargar productos: {ex.Message}";
                Products = new List<ProductEntity>();
            }
        }

        // ? GENERAR SKU AUTOMÁTICO
        private async Task<string> GenerarSKUAutomatico(string categoria, string marca)
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
                var codigoMarca = marca.Length >= 3 ? marca.Substring(0, 3).ToUpper() : marca.ToUpper().PadRight(3, 'X');

                // Número secuencial
                var ultimoNumero = await _context.Productos
                    .Where(p => p.SKU.StartsWith($"{prefijo}-{codigoMarca}-"))
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

        // ? ACTUALIZAR STOCK
        public async Task<IActionResult> OnPostUpdateStockAsync(int id, int newStock)
        {
            try
            {
                var product = await _context.Productos.AsTracking().FirstOrDefaultAsync(p => p.Id == id); // asegurar tracking
                if (product != null)
                {
                    var oldStock = product.Stock;
                    product.Stock = newStock;
                    product.FechaActualizacion = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    // invalidar caché vía servicio (si está disponible en DI)
                    try
                    {
                        var ps = HttpContext.RequestServices.GetService(typeof(IProductService)) as IProductService;
                        if (ps is ProductService concrete)
                        {
                            var mi = typeof(ProductService).GetMethod("InvalidateProductCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            mi?.Invoke(concrete, null);
                        }
                    }
                    catch { }
                    TempData["SuccessMessage"] = $"Stock actualizado de {oldStock} a {newStock} para '{product.Name}'";
                }
                else
                {
                    TempData["ErrorMessage"] = "Producto no encontrado.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al actualizar stock: {ex.Message}";
            }
            return RedirectToPage();
        }

        // ? ELIMINAR PRODUCTO (SOFT DELETE)
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            try
            {
                var product = await _context.Productos.FindAsync(id);
                if (product != null)
                {
                    product.IsActive = false;
                    product.FechaActualizacion = DateTime.UtcNow;
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Producto '{product.Name}' eliminado exitosamente.";
                    _logger.LogInformation("Producto eliminado (soft delete): {ProductId}", id);
                }
                else
                {
                    TempData["ErrorMessage"] = "Producto no encontrado.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar producto {ProductId}", id);
                TempData["ErrorMessage"] = $"Error al eliminar producto: {ex.Message}";
            }

            return RedirectToPage();
        }

        // ? TOGGLE VISIBILIDAD
        public async Task<IActionResult> OnPostToggleStatusAsync(int id)
        {
            try
            {
                var product = await _context.Productos.FindAsync(id);
                if (product != null)
                {
                    product.IsActive = !product.IsActive;
                    product.FechaActualizacion = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    
                    var status = product.IsActive ? "visible" : "oculto";
                    TempData["SuccessMessage"] = $"Producto '{product.Name}' marcado como {status}.";
                    _logger.LogInformation("Estado cambiado para producto {ProductId}: {Status}", id, status);
                }
                else
                {
                    TempData["ErrorMessage"] = "Producto no encontrado.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado del producto {ProductId}", id);
                TempData["ErrorMessage"] = $"Error al cambiar estado: {ex.Message}";
            }

            return RedirectToPage();
        }

        private async Task LoadProductsForDisplay()
        {
            try
            {
                Products = await _context.Productos.Where(p => p.IsActive).ToListAsync();
                Categories = Products.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();
                Brands = Products.Select(p => p.Brand).Distinct().OrderBy(b => b).ToList();
                TotalProducts = Products.Count;
                ActiveProducts = Products.Count;
                LowStockProducts = Products.Count(p => p.Stock <= p.StockMinimo);
                OutOfStockProducts = Products.Count(p => p.Stock == 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar productos para display");
                TempData["ErrorMessage"] = $"Error al cargar productos: {ex.Message}";
                Products = new List<ProductEntity>();
            }
        }

        // ?? EXPORTAR A EXCEL
        public async Task<IActionResult> OnGetExportExcelAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando exportación de productos a Excel");
                
                var allProducts = await _context.Productos
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Category)
                    .ThenBy(p => p.Brand)
                    .ThenBy(p => p.Name)
                    .ToListAsync();

                _logger.LogInformation("Obtenidos {Count} productos para exportar", allProducts.Count);

                var excelData = await _excelExportService.ExportProductsToExcelAsync(allProducts);

                // Detectar si es CSV o Excel basado en contenido
                var isExcel = excelData.Length > 1000 && excelData[0] == 0x50 && excelData[1] == 0x4B; // ZIP signature
                
                string fileName;
                string contentType;
                
                if (isExcel)
                {
                    fileName = $"Productos_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    _logger.LogInformation("Enviando archivo Excel: {FileName}", fileName);
                }
                else
                {
                    fileName = $"Productos_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    contentType = "text/csv";
                    _logger.LogInformation("Enviando archivo CSV alternativo: {FileName}", fileName);
                }
                
                // Headers HTTP seguros
                Response.Clear();
                Response.Headers.Clear();
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                Response.Headers.Add("Content-Type", contentType);
                Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                Response.Headers.Add("Pragma", "no-cache");
                Response.Headers.Add("Expires", "0");
                
                return File(excelData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completo al exportar productos a Excel");
                TempData["ErrorMessage"] = $"Error al exportar: {ex.Message}";
                return RedirectToPage();
            }
        }

        // ?? EXPORTAR TABLA DINÁMICA
        public async Task<IActionResult> OnGetExportPivotAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando exportación de resumen a Excel");
                
                var allProducts = await _context.Productos
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Category)
                    .ThenBy(p => p.Brand)
                    .ThenBy(p => p.Name)
                    .ToListAsync();

                var excelData = await _excelExportService.ExportProductsPivotTableAsync(allProducts);

                // Detectar tipo de archivo
                var isExcel = excelData.Length > 1000 && excelData[0] == 0x50 && excelData[1] == 0x4B;
                
                string fileName;
                string contentType;
                
                if (isExcel)
                {
                    fileName = $"Resumen_Productos_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                }
                else
                {
                    fileName = $"Resumen_Productos_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    contentType = "text/csv";
                }
                
                Response.Clear();
                Response.Headers.Clear();
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                Response.Headers.Add("Content-Type", contentType);
                Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                
                return File(excelData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar tabla dinámica");
                TempData["ErrorMessage"] = $"Error al exportar resumen: {ex.Message}";
                return RedirectToPage();
            }
        }

        // ?? EXPORTAR PRODUCTOS FILTRADOS
        public async Task<IActionResult> OnGetExportFilteredAsync(string? category = null, 
            string? brand = null, string? search = null, bool inactiveOnly = false, bool lowStockOnly = false)
        {
            try
            {
                _logger.LogInformation("Exportando productos filtrados");
                
                // Aplicar los mismos filtros que en OnGetAsync
                var query = _context.Productos.AsQueryable();

                if (!string.IsNullOrEmpty(category) && category != "all")
                {
                    query = query.Where(p => p.Category.ToLower() == category.ToLower());
                }

                if (!string.IsNullOrEmpty(brand) && brand != "all")
                {
                    query = query.Where(p => p.Brand.ToLower() == brand.ToLower());
                }

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(p => p.Name.Contains(search) || 
                                           p.Brand.Contains(search) ||
                                           p.Description.Contains(search) ||
                                           (p.SKU != null && p.SKU.Contains(search)));
                }

                if (inactiveOnly)
                {
                    query = query.Where(p => !p.IsActive);
                }
                else
                {
                    query = query.Where(p => p.IsActive);
                }

                if (lowStockOnly)
                {
                    query = query.Where(p => p.Stock <= p.StockMinimo);
                }

                var filteredProducts = await query
                    .OrderBy(p => p.Category)
                    .ThenBy(p => p.Brand)
                    .ThenBy(p => p.Name)
                    .ToListAsync();

                var excelData = await _excelExportService.ExportProductsToExcelAsync(filteredProducts);

                // Construir nombre de archivo
                var filterSuffix = "";
                if (!string.IsNullOrEmpty(category) && category != "all") filterSuffix += $"_{category}";
                if (!string.IsNullOrEmpty(brand) && brand != "all") filterSuffix += $"_{brand}";
                if (inactiveOnly) filterSuffix += "_Ocultos";
                if (lowStockOnly) filterSuffix += "_BajoStock";
                if (!string.IsNullOrEmpty(search)) filterSuffix += "_Filtrado";

                // Detectar tipo
                var isExcel = excelData.Length > 1000 && excelData[0] == 0x50 && excelData[1] == 0x4B;
                
                string fileName;
                string contentType;
                
                if (isExcel)
                {
                    fileName = $"Productos{filterSuffix}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                }
                else
                {
                    fileName = $"Productos{filterSuffix}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    contentType = "text/csv";
                }
                
                Response.Clear();
                Response.Headers.Clear();
                Response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");
                Response.Headers.Add("Content-Type", contentType);
                
                return File(excelData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al exportar productos filtrados");
                TempData["ErrorMessage"] = $"Error al exportar productos filtrados: {ex.Message}";
                return RedirectToPage();
            }
        }
    }
}