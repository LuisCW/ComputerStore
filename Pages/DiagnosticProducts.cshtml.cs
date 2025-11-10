using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ComputerStore.Data;
using System.Text;

namespace ComputerStore.Pages
{
    public class DiagnosticProductsModel : PageModel
    {
 private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public string DiagnosticInfo { get; set; } = "";

        public DiagnosticProductsModel(ApplicationDbContext context, IConfiguration configuration)
        {
         _context = context;
            _configuration = configuration;
        }

        public async Task OnGetAsync()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== DIAGNOSTIC INFO ===\n");

        try
   {
          // 1. Connection String Info
  var connectionString = _configuration.GetConnectionString("DefaultConnection");
     if (string.IsNullOrEmpty(connectionString))
             {
            sb.AppendLine("? ERROR: Connection String is NULL or EMPTY");
           }
     else
 {
    sb.AppendLine("? Connection String found");
          // Hide password
        var safeCs = connectionString.Contains("Password=") 
          ? connectionString.Substring(0, connectionString.IndexOf("Password=")) + "Password=***;..." 
               : connectionString;
     sb.AppendLine($"   {safeCs}\n");
    }

         // 2. Can Connect?
            sb.AppendLine("Testing database connection...");
         var canConnect = await _context.Database.CanConnectAsync();
   if (canConnect)
            {
        sb.AppendLine("? Database connection successful\n");
      }
        else
      {
     sb.AppendLine("? ERROR: Cannot connect to database\n");
                }

      // 3. Count products
                sb.AppendLine("Counting products...");
        var productCount = await _context.Productos.CountAsync();
    sb.AppendLine($"? Total products in database: {productCount}\n");

      // 4. Count active products
     var activeCount = await _context.Productos.CountAsync(p => p.IsActive);
           sb.AppendLine($"? Active products: {activeCount}\n");

        // 5. List first 10 products
    sb.AppendLine("First 10 products:");
     var products = await _context.Productos
   .Take(10)
     .Select(p => new { p.Id, p.Name, p.IsActive, p.Stock })
           .ToListAsync();

      foreach (var p in products)
     {
        sb.AppendLine($"  - ID: {p.Id}, Name: {p.Name}, Active: {p.IsActive}, Stock: {p.Stock}");
         }
        }
            catch (Exception ex)
       {
   sb.AppendLine($"\n? EXCEPTION: {ex.Message}");
          sb.AppendLine($"\nStack Trace:\n{ex.StackTrace}");
   
      if (ex.InnerException != null)
            {
     sb.AppendLine($"\nInner Exception: {ex.InnerException.Message}");
     }
            }

      DiagnosticInfo = sb.ToString();
        }
    }
}
