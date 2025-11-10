using System.Text.Json;
using ComputerStore.Data;
using ComputerStore.Models;
using Microsoft.EntityFrameworkCore;

namespace ComputerStore.Services
{
 public class JsonExportService
 {
 private readonly ApplicationDbContext _context;
 private readonly IWebHostEnvironment _environment;

 public JsonExportService(ApplicationDbContext context, IWebHostEnvironment environment)
 {
 _context = context;
 _environment = environment;
 }

 public async Task GenerateProductsJsonAsync()
 {
 // Reestructurar la consulta para evitar el operador de propagación nula en la proyección
 var products = await _context.Productos
 .AsNoTracking()
 .Include(p => p.Imagenes.Where(i => i.EsPrincipal))
 .Where(p => p.IsActive)
 .Select(p => new
 {
 p.Id,
 p.Name,
 p.Price,
 p.Category,
 p.Brand,
 p.Description,
 p.Stock,
 ImageUrl = p.Imagenes.FirstOrDefault(i => i.EsPrincipal).ImagenUrl // Evitar el uso de ?. en la proyección
 })
 .ToListAsync();

 var json = JsonSerializer.Serialize(products, new JsonSerializerOptions
 {
 WriteIndented = true
 });

 var filePath = Path.Combine(_environment.WebRootPath, "data", "products.json");
 Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
 await File.WriteAllTextAsync(filePath, json);
 }
 }
}