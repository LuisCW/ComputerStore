using ComputerStore.Models;
using ComputerStore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComputerStore.Pages
{
    public class ProductsModel : BasePageModel
    {
        private readonly LittleCarService _carritoService;
        private readonly IProductService _productService;
        private readonly ILogger<ProductsModel> _logger;

        public List<Products> PagedProductos { get; set; } = new List<Products>();
        public List<string> Categorias { get; set; } = new List<string>();
        public List<string> Marcas { get; set; } = new List<string>();
        public List<string> Colores { get; set; } = new List<string>();
        public string CurrentFilter { get; set; } = string.Empty;
        public string SearchTerm { get; set; } = string.Empty;

        // Pagination
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;
        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 15;
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }

        public ProductsModel(LittleCarService carritoService, SignInManager<ApplicationUser> signInManager, ILogger<ProductsModel> logger, IProductService productService) 
            : base(signInManager)
        {
            _carritoService = carritoService;
            _logger = logger;
            _productService = productService;
        }

        // Bind page from query string using parameter name 'page'
        public async Task OnGetAsync(int? page, string? category, string? search)
        {
            // prefer explicit parameter
            if (page.HasValue && page.Value > 0)
            {
                PageNumber = page.Value;
            }
            else if (Request?.Query != null && Request.Query.ContainsKey("page"))
            {
                var raw = Request.Query["page"].ToString();
                if (int.TryParse(raw, out var p)) PageNumber = p;
            }

            if (PageNumber < 1) PageNumber = 1;
            if (PageSize < 1) PageSize = 15;

            CurrentFilter = category ?? string.Empty;
            SearchTerm = search ?? string.Empty;

            await CargarProductosAsync();
            await CargarFiltrosAsync();
        }

        public override async Task<IActionResult> OnPostAsync(string? action = null, string? returnUrl = null)
        {
            // Manejar logout primero
            if (action == "logout")
            {
                return await HandleLogout(returnUrl);
            }
            
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAgregarAlCarrito(int id)
        {
            try
            {
                var producto = await _productService.GetProductByIdAsync(id);
                if (producto == null)
                {
                    return new JsonResult(new { success = false, message = "Producto no encontrado" });
                }

                // Validación de stock agotado
                if (producto.Stock <= 0 || string.Equals(producto.Availability?.Trim(), "Agotado", StringComparison.OrdinalIgnoreCase))
                {
                    return new JsonResult(new { success = false, message = "Este producto está agotado y no se puede agregar al carrito." });
                }

                int nuevaCantidad = _carritoService.AgregarAlCarrito(producto);

                var response = new {
                    success = true,
                    cartCount = nuevaCantidad,
                    message = $"{producto.Name} agregado al carrito",
                    productName = producto.Name
                };

                return new JsonResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar producto {Id} al carrito", id);
                return new JsonResult(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        private async Task CargarProductosAsync()
        {
            try
            {
                // Usar paginación en base de datos en lugar de cargar todo en memoria
                (List<Products> productos, int totalCount) result;

                if (!string.IsNullOrWhiteSpace(SearchTerm))
                {
                    // Búsqueda con paginación
                    result = await _productService.SearchProductsPagedAsync(SearchTerm, PageNumber, PageSize);
                }
                else if (!string.IsNullOrWhiteSpace(CurrentFilter))
                {
                    // Filtro por categoría con paginación
                    result = await _productService.GetProductsByCategoryPagedAsync(CurrentFilter, PageNumber, PageSize);
                }
                else
                {
                    // Todos los productos con paginación
                    result = await _productService.GetProductsPagedAsync(PageNumber, PageSize);
                }

                PagedProductos = result.productos;
                TotalItems = result.totalCount;

                // Calcular páginas totales
                TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
                if (TotalPages == 0) TotalPages = 1;
                if (PageNumber > TotalPages) PageNumber = TotalPages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar productos paginados");
                PagedProductos = new List<Products>();
                TotalItems = 0;
                TotalPages = 1;
            }
        }

        private async Task CargarFiltrosAsync()
        {
            try
            {
                // Cargar filtros sin traer todos los productos
                Categorias = await _productService.GetCategoriesAsync();
                Marcas = await _productService.GetBrandsAsync();
                Colores = await _productService.GetColorsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar filtros");
                Categorias = new List<string>();
                Marcas = new List<string>();
                Colores = new List<string>();
            }
        }
    }
}
