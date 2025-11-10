using ComputerStore.Models;
using ComputerStore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComputerStore.Pages
{
    public class IndexModel : BasePageModel
    {
        private readonly LittleCarService _carritoService;
        private readonly IProductService _productService;

        public List<Products> DestacadosProductos { get; set; } = new List<Products>();
        public List<Products> NuevosProductos { get; set; } = new List<Products>();
        public List<Products> MasVendidosProductos { get; set; } = new List<Products>();

        public IndexModel(LittleCarService carritoService, SignInManager<ApplicationUser> signInManager, IProductService productService)
            : base(signInManager)
        {
            _carritoService = carritoService;
            _productService = productService;
        }

        public async Task OnGetAsync()
        {
            await CargarProductosAsync();
        }

        public async Task<IActionResult> OnPostLogout(string? returnUrl = null)
        {
            return await HandleLogout(returnUrl);
        }

        // Agregar más registros en OnPostAgregarAlCarrito
        public async Task<IActionResult> OnPostAgregarAlCarrito(int id)
        {
         try
            {
    Console.WriteLine($"================================");
    Console.WriteLine($"?? OnPostAgregarAlCarrito llamado con ID: {id}");
    Console.WriteLine($"?? Usuario autenticado: {User.Identity?.IsAuthenticated ?? false}");
    Console.WriteLine($"?? HttpContext existe: {HttpContext != null}");
    Console.WriteLine($"?? Session existe: {HttpContext?.Session != null}");
 
    if (HttpContext?.Session != null)
    {
        Console.WriteLine($"?? Session ID: {HttpContext.Session.Id}");
        Console.WriteLine($"?? Session IsAvailable: {HttpContext.Session.IsAvailable}");
    }
    Console.WriteLine($"================================");

       var producto = await _productService.GetProductByIdAsync(id);
         if (producto == null)
   {
        Console.WriteLine($"?? Producto {id} no encontrado");
       return new JsonResult(new { success = false, message = "Producto no encontrado" });
      }

    // Bloqueo cuando está agotado (server-side)
         if (producto.Stock <= 0 || string.Equals(producto.Availability?.Trim(), "Agotado", StringComparison.OrdinalIgnoreCase))
{
       Console.WriteLine($"?? Producto {id} agotado");
     return new JsonResult(new { success = false, message = "Este producto está agotado y no se puede agregar al carrito." });
     }

       Console.WriteLine($"? Producto obtenido: ID={producto.Id}, Nombre={producto.Name}, Precio={producto.Price}");

   int nuevaCantidad = _carritoService.AgregarAlCarrito(producto);
  Console.WriteLine($"? Producto {producto.Name} agregado, nueva cantidad: {nuevaCantidad}");
       
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
       Console.WriteLine($"? Error en OnPostAgregarAlCarrito: {ex.Message}");
          Console.WriteLine($"?? Stack Trace: {ex.StackTrace}");
        return new JsonResult(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

        // Modificar CargarProductosAsync para cargar datos completos como en Products
        private async Task CargarProductosAsync()
        {
            try
            {
                (List<Products> productosDestacados, _) = await _productService.GetProductsPagedAsync(1,3);
                DestacadosProductos = productosDestacados;

                (List<Products> nuevosProductos, _) = await _productService.GetProductsPagedAsync(1,3);
                NuevosProductos = nuevosProductos;

                (List<Products> masVendidosProductos, _) = await _productService.GetProductsPagedAsync(1,3);
                MasVendidosProductos = masVendidosProductos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error al cargar productos: {ex.Message}");
                Console.WriteLine($"?? Stack Trace: {ex.StackTrace}");
                DestacadosProductos = new List<Products>();
                NuevosProductos = new List<Products>();
                MasVendidosProductos = new List<Products>();
            }
        }
    }
}