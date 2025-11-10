using ComputerStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ComputerStore.Data
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider, TimeZoneInfo timeZone)
        {
            using var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Aplicar migraciones para asegurar persistencia del esquema
            await context.Database.MigrateAsync();

            // Crear roles si no existen
            var roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Crear usuario administrador si no existe
            var adminEmail = "admin@computerhipermegared.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "Admin",
                    Email = adminEmail,
                    EmailConfirmed = true,
                    PrimerNombre = "Administrador",
                    PrimerApellido = "Sistema",
                    NumeroDocumento = "00000000",
                    TipoDocumento = "CC",
                    Ciudad = "Bogotá",
                    Departamento = "Cundinamarca",
                    Direccion = "Calle Principal",
                    PhoneNumber = "3001234567",
                    IsActive = true,
                    FechaRegistro = GetColombiaTime(timeZone)
                };

                var result = await userManager.CreateAsync(adminUser, "E_commerce123$");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                    Console.WriteLine("? Usuario administrador creado");
                    Console.WriteLine($"   ?? Usuario: {adminUser.UserName}");
                    Console.WriteLine($"   ?? Email: {adminUser.Email}");
                    Console.WriteLine($"   ?? Contraseña: E_commerce123$");
                }
            }
            else
            {
                // Verificar que el admin tenga el rol
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
                Console.WriteLine("? Usuario administrador ya existe");
                Console.WriteLine($"   ?? Usuario: {adminUser.UserName}");
                Console.WriteLine($"   ?? Email: {adminUser.Email}");
                Console.WriteLine($"   ?? Contraseña: E_commerce123$");
            }

            // Verificar si ya existen productos
            if (!await context.Productos.AnyAsync())
            {
                Console.WriteLine("?? Creando productos de ejemplo...");
                await CreateSampleProducts(context, timeZone);
            }
            else
            {
                Console.WriteLine("? Los productos ya existen en la base de datos");
            }

            Console.WriteLine("? Base de datos inicializada correctamente");
        }

        private static async Task CreateSampleProducts(ApplicationDbContext context, TimeZoneInfo timeZone)
        {
            var productos = new[]
            {
                new ProductEntity
                {
                    Name = "Laptop Gaming MSI Katana",
                    Category = "Laptops",
                    Price = 3500000,
                    Brand = "MSI",
                    Component = "Laptop",
                    Color = "Negro",
                    Availability = "Disponible",
                    Stock = 10,
                    StockMinimo = 2,
                    Description = "Laptop gaming de alto rendimiento con RTX 3060",
                    ImageUrl = "https://asset.msi.com/resize/image/global/product/product_1_20220519181943_628632df8d5e1.png62405b38c58fe0f07fcdc9d1",
                    DetailsJson = """{"Procesador":"Intel i7-12700H","RAM":"16GB DDR4","Almacenamiento":"512GB NVMe SSD","Tarjeta Gráfica":"RTX 3060","Pantalla":"15.6 FHD 144Hz"}""",
                    CreatedDate = GetColombiaTime(timeZone),
                    IsActive = true
                },
                new ProductEntity
                {
                    Name = "PC Oficina Dell OptiPlex",
                    Category = "Computadoras de Escritorio",
                    Price = 1500000,
                    Brand = "Dell",
                    Component = "PC",
                    Color = "Negro",
                    Availability = "Disponible",
                    Stock = 15,
                    StockMinimo = 3,
                    Description = "PC para oficina y tareas cotidianas",
                    ImageUrl = "https://i.dell.com/is/image/DellContent/content/dam/ss2/product-images/dell-client-products/desktops/optiplex-desktops/optiplex-3090-micro/media-gallery/optiplex-3090-micro-gallery-1.psd",
                    DetailsJson = """{"Procesador":"Intel i5-11400","RAM":"8GB DDR4","Almacenamiento":"256GB SSD","Puertos":"USB 3.0, HDMI","Sistema":"Windows 11 Pro"}""",
                    CreatedDate = GetColombiaTime(timeZone),
                    IsActive = true
                },
                new ProductEntity
                {
                    Name = "Monitor 4K Samsung",
                    Category = "Monitores",
                    Price = 800000,
                    Brand = "Samsung",
                    Component = "Monitor",
                    Color = "Negro",
                    Availability = "Disponible",
                    Stock = 20,
                    StockMinimo = 5,
                    Description = "Monitor 4K de 27 pulgadas para profesionales",
                    ImageUrl = "https://images.samsung.com/is/image/samsung/p6pim/latin/lu28e590ds-zl/gallery/latin-uhd-monitor-28-lu28e590ds-zl-531938-lu28e590ds-zl-531938-1",
                    DetailsJson = """{"Tamaño":"27 pulgadas","Resolución":"3840x2160 4K","Tecnología":"IPS","Conectividad":"HDMI, DisplayPort, USB-C","HDR":"HDR10"}""",
                    CreatedDate = GetColombiaTime(timeZone),
                    IsActive = true
                }
            };

            context.Productos.AddRange(productos);
            await context.SaveChangesAsync();
            Console.WriteLine($"? {productos.Length} productos creados con zona horaria de Colombia");
        }

        private static DateTime GetColombiaTime(TimeZoneInfo timeZone)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
        }
    }
}