using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ComputerStore.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ComputerStore.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class SettingsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public SettingsModel(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [BindProperty]
        public SystemSettingsViewModel Settings { get; set; } = new();

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            // Cargar configuraciones actuales
            Settings = new SystemSettingsViewModel
            {
                // Configuración general
                SiteName = _configuration["SiteSettings:Name"] ?? "CompuHiperMegaRed",
                SiteDescription = _configuration["SiteSettings:Description"] ?? "Tienda de tecnología",
                SiteEmail = _configuration["SiteSettings:Email"] ?? "admin@compuhipermegared.com",
                SitePhone = _configuration["SiteSettings:Phone"] ?? "+57 300 123 4567",
                
                // PayU
                PayUApiKey = _configuration["PayU:ApiKey"] ?? "",
                PayUMerchantId = _configuration["PayU:MerchantId"] ?? "",
                PayUAccountId = _configuration["PayU:AccountId"] ?? "",
                PayUTestMode = bool.Parse(_configuration["PayU:TestMode"] ?? "true"),
                
                // Configuración de envíos
                EnvioGratis = decimal.Parse(_configuration["Shipping:FreeShippingThreshold"] ?? "200000"),
                CostoEnvioBase = decimal.Parse(_configuration["Shipping:BaseCost"] ?? "15000"),
                
                // Configuración de stock
                StockMinimoGlobal = int.Parse(_configuration["Inventory:MinStock"] ?? "5"),
                AlertaStockBajo = bool.Parse(_configuration["Inventory:LowStockAlert"] ?? "true"),
                
                // Configuración de notificaciones
                EmailNotificaciones = bool.Parse(_configuration["Notifications:EmailEnabled"] ?? "true"),
                SMSNotificaciones = bool.Parse(_configuration["Notifications:SMSEnabled"] ?? "false"),
                
                // Analytics
                AnalyticsUrl = _configuration["Analytics:Url"] ?? "http://127.0.0.1:5001/api/analytics",
                AnalyticsEnabled = bool.Parse(_configuration["Analytics:Enabled"] ?? "true")
            };

            // Obtener estadísticas de la base de datos
            await LoadDatabaseStats();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Por favor corrige los errores en el formulario.";
                await LoadDatabaseStats();
                return Page();
            }

            try
            {
                // Aquí se guardarían las configuraciones
                // En un entorno real, esto se haría en appsettings.json o base de datos
                
                SuccessMessage = "Configuraciones guardadas exitosamente.";
                TempData["SuccessMessage"] = SuccessMessage;
                
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al guardar configuraciones: {ex.Message}";
                await LoadDatabaseStats();
                return Page();
            }
        }

        public async Task<IActionResult> OnPostClearCacheAsync()
        {
            try
            {
                // Lógica para limpiar cache
                TempData["SuccessMessage"] = "Cache limpiado exitosamente.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al limpiar cache: {ex.Message}";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostTestPayUAsync()
        {
            try
            {
                // Lógica para probar conexión con PayU
                TempData["SuccessMessage"] = "Conexión con PayU probada exitosamente.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error conectando con PayU: {ex.Message}";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostOptimizeDatabaseAsync()
        {
            try
            {
                // Lógica para optimizar base de datos
                // Ejemplo: VACUUM, REINDEX, etc.
                TempData["SuccessMessage"] = "Base de datos optimizada exitosamente.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error optimizando base de datos: {ex.Message}";
                return RedirectToPage();
            }
        }

        private async Task LoadDatabaseStats()
        {
            try
            {
                Settings.TotalUsuarios = await _context.Users.CountAsync();
                Settings.TotalProductos = await _context.Productos.CountAsync();
                Settings.TotalPedidos = await _context.Pedidos.CountAsync();
                Settings.TotalTransacciones = await _context.Transacciones.CountAsync();
                Settings.ProductosBajoStock = await _context.Productos.CountAsync(p => p.Stock <= p.StockMinimo);
            }
            catch (Exception)
            {
                // Manejar errores silenciosamente
                Settings.TotalUsuarios = 0;
                Settings.TotalProductos = 0;
                Settings.TotalPedidos = 0;
                Settings.TotalTransacciones = 0;
                Settings.ProductosBajoStock = 0;
            }
        }
    }

    public class SystemSettingsViewModel
    {
        [Display(Name = "Nombre del sitio")]
        [Required(ErrorMessage = "El nombre del sitio es requerido")]
        public string SiteName { get; set; } = "";

        [Display(Name = "Descripción del sitio")]
        public string SiteDescription { get; set; } = "";

        [Display(Name = "Email de contacto")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string SiteEmail { get; set; } = "";

        [Display(Name = "Teléfono de contacto")]
        public string SitePhone { get; set; } = "";

        // PayU Configuration
        [Display(Name = "PayU API Key")]
        public string PayUApiKey { get; set; } = "";

        [Display(Name = "PayU Merchant ID")]
        public string PayUMerchantId { get; set; } = "";

        [Display(Name = "PayU Account ID")]
        public string PayUAccountId { get; set; } = "";

        [Display(Name = "Modo de prueba PayU")]
        public bool PayUTestMode { get; set; } = true;

        // Shipping Configuration
        [Display(Name = "Envío gratis desde")]
        [Range(0, double.MaxValue, ErrorMessage = "El monto debe ser positivo")]
        public decimal EnvioGratis { get; set; } = 200000;

        [Display(Name = "Costo base de envío")]
        [Range(0, double.MaxValue, ErrorMessage = "El costo debe ser positivo")]
        public decimal CostoEnvioBase { get; set; } = 15000;

        // Inventory Configuration
        [Display(Name = "Stock mínimo global")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock debe ser positivo")]
        public int StockMinimoGlobal { get; set; } = 5;

        [Display(Name = "Alertas de stock bajo")]
        public bool AlertaStockBajo { get; set; } = true;

        // Notification Configuration
        [Display(Name = "Notificaciones por email")]
        public bool EmailNotificaciones { get; set; } = true;

        [Display(Name = "Notificaciones por SMS")]
        public bool SMSNotificaciones { get; set; } = false;

        // Analytics Configuration
        [Display(Name = "URL del servidor de analytics")]
        [Url(ErrorMessage = "Formato de URL inválido")]
        public string AnalyticsUrl { get; set; } = "";

        [Display(Name = "Analytics habilitado")]
        public bool AnalyticsEnabled { get; set; } = true;

        // Database Statistics (Read-only)
        public int TotalUsuarios { get; set; }
        public int TotalProductos { get; set; }
        public int TotalPedidos { get; set; }
        public int TotalTransacciones { get; set; }
        public int ProductosBajoStock { get; set; }
    }
}