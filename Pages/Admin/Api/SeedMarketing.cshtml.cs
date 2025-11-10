using ComputerStore.Data;
using ComputerStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComputerStore.Pages.Admin.Api
{
    [Authorize(Roles = "Admin")]
    public class SeedMarketingModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SeedMarketingModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Verificar si ya existen datos
                var existingData = _context.MarketingCosts.Any();
                if (existingData)
                {
                    return new JsonResult(new { success = false, message = "Ya existen datos de marketing", count = _context.MarketingCosts.Count() });
                }

                var random = new Random(42);
                var marketingCosts = new List<MarketingCost>();
                var fechaBase = DateTime.UtcNow.Date;

                // Generar datos para los últimos 30 días
                for (int dia = -29; dia <= 0; dia++)
                {
                    var fecha = fechaBase.AddDays(dia);

                    // Google Ads diario
                    marketingCosts.Add(new MarketingCost
                    {
                        Fecha = fecha,
                        Canal = "Google Ads",
                        Campana = "Computadores Gaming",
                        CostoTotal = random.Next(250000, 350000),
                        CostoPorClick = random.Next(900, 1400),
                        Clicks = random.Next(180, 280),
                        Impresiones = random.Next(7000, 12000),
                        TipoCampana = "PPC",
                        PlataformaPublicidad = "google_ads"
                    });

                    // Facebook Ads diario
                    marketingCosts.Add(new MarketingCost
                    {
                        Fecha = fecha,
                        Canal = "Facebook Ads", 
                        Campana = "Productos Destacados",
                        CostoTotal = random.Next(150000, 220000),
                        CostoPorClick = random.Next(500, 800),
                        Clicks = random.Next(200, 350),
                        Impresiones = random.Next(12000, 20000),
                        TipoCampana = "Social",
                        PlataformaPublicidad = "facebook_ads"
                    });

                    // SEO diario (costo fijo)
                    marketingCosts.Add(new MarketingCost
                    {
                        Fecha = fecha,
                        Canal = "SEO",
                        Campana = "Optimización Orgánica",
                        CostoTotal = 50000, // 50k diarios = 1.5M mensual
                        TipoCampana = "Organic",
                        Notas = "Herramientas SEO y mantenimiento"
                    });

                    // Email Marketing (3 veces por semana)
                    if (dia % 2 == 0)
                    {
                        marketingCosts.Add(new MarketingCost
                        {
                            Fecha = fecha,
                            Canal = "Email",
                            Campana = "Newsletter y Promociones",
                            CostoTotal = random.Next(25000, 40000),
                            TipoCampana = "Email",
                            Notas = "Plataforma email + diseño"
                        });
                    }

                    // Referrals (semanalmente) 
                    if (dia % 7 == 0)
                    {
                        marketingCosts.Add(new MarketingCost
                        {
                            Fecha = fecha,
                            Canal = "Referrals",
                            Campana = "Programa de Referidos",
                            CostoTotal = random.Next(30000, 60000),
                            TipoCampana = "Referral",
                            Notas = "Comisiones pagadas a referidores"
                        });
                    }
                }

                // Insertar en la base de datos
                await _context.MarketingCosts.AddRangeAsync(marketingCosts);
                await _context.SaveChangesAsync();

                // Calcular resumen
                var resumen = marketingCosts.GroupBy(m => m.Canal)
                    .Select(g => new {
                        Canal = g.Key,
                        TotalRegistros = g.Count(),
                        CostoTotal = g.Sum(m => m.CostoTotal),
                        PromedioDaily = g.Average(m => m.CostoTotal)
                    }).ToList();

                return new JsonResult(new { 
                    success = true, 
                    message = "Datos de marketing insertados exitosamente", 
                    total_records = marketingCosts.Count,
                    resumen = resumen
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }
    }
}