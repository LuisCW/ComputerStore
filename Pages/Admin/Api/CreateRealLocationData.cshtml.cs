using ComputerStore.Data;
using ComputerStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComputerStore.Pages.Admin.Api
{
    public class CreateRealLocationDataModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateRealLocationDataModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var ciudades = new[]
                {
                    new { Ciudad = "Bogotá", Departamento = "Cundinamarca" },
                    new { Ciudad = "Medellín", Departamento = "Antioquia" },
                    new { Ciudad = "Cali", Departamento = "Valle del Cauca" },
                    new { Ciudad = "Barranquilla", Departamento = "Atlántico" },
                    new { Ciudad = "Cartagena", Departamento = "Bolívar" },
                    new { Ciudad = "Bucaramanga", Departamento = "Santander" }
                };

                var usuariosCreados = 0;
                var pedidosCreados = 0;

                // Crear usuarios reales
                for (int i = 0; i < 20; i++)
                {
                    var ciudad = ciudades[i % ciudades.Length];
                    var email = $"real{i+1}@test.com";
                    
                    var user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        PrimerNombre = $"Usuario{i+1}",
                        SegundoNombre = "Real",
                        PrimerApellido = "Test",
                        SegundoApellido = ciudad.Ciudad,
                        TipoDocumento = "CC",
                        NumeroDocumento = $"1000000{i+1:D2}",
                        Direccion = $"Calle {i+1} # 10-20",
                        Ciudad = ciudad.Ciudad,
                        Departamento = ciudad.Departamento,
                        CodigoPostal = "110001",
                        FechaRegistro = DateTime.UtcNow.AddDays(-30),
                        IsActive = true,
                        PhoneNumber = $"300123456{i+1:D2}"
                    };

                    var result = await _userManager.CreateAsync(user, "Test123!");
                    if (result.Succeeded)
                    {
                        usuariosCreados++;
                        
                        // Crear pedidos para este usuario
                        for (int j = 0; j < (i % 3 + 1); j++)
                        {
                            var pedido = new PedidoEntity
                            {
                                UserId = user.Id,
                                FechaCreacion = DateTime.UtcNow.AddDays(-(i * 2 + j)),
                                Estado = "COMPLETADO",
                                Total = (i + 1) * 500000 + j * 100000,
                                DireccionEnvio = user.Direccion,
                                CiudadEnvio = user.Ciudad,
                                DepartamentoEnvio = user.Departamento,
                                CodigoPostalEnvio = user.CodigoPostal,
                                TelefonoContacto = user.PhoneNumber,
                                MetodoPago = "PSE"
                            };

                            _context.Pedidos.Add(pedido);
                            pedidosCreados++;
                        }
                    }
                }

                await _context.SaveChangesAsync();

                return new JsonResult(new { 
                    success = true, 
                    usuarios = usuariosCreados, 
                    pedidos = pedidosCreados 
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, error = ex.Message });
            }
        }
    }
}