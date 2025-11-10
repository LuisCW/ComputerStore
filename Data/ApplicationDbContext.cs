using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ComputerStore.Models;

namespace ComputerStore.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets para las entidades
        public DbSet<ProductEntity> Productos { get; set; }
        public DbSet<ProductoImagen> ProductoImagenes { get; set; }
        public DbSet<PedidoEntity> Pedidos { get; set; }
        public DbSet<PedidoDetalle> PedidoDetalles { get; set; }
        public DbSet<EnvioEntity> Envios { get; set; }
        public DbSet<TransaccionEntity> Transacciones { get; set; }
        public DbSet<AdminNotification> AdminNotifications { get; set; }
        public DbSet<AdminNotificationRead> AdminNotificationReads { get; set; }
        public DbSet<TrafficEvent> TrafficEvents { get; set; } // NEW
        public DbSet<MarketingCost> MarketingCosts { get; set; } // NUEVO para ROI real
        public DbSet<MarketingConfiguration> MarketingConfigurations { get; set; } // NUEVO para guardar credenciales de forma segura

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Verificar el proveedor de base de datos
            var isPostgreSQL = Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL";
            var isSQLite = Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite";

            // Configuraciones de entidades

            // ProductEntity
            builder.Entity<ProductEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                
                if (isPostgreSQL)
                {
                    entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                    entity.Property(e => e.PrecioCompra).HasColumnType("decimal(18,2)");
                    entity.Property(e => e.PorcentajeGanancia).HasColumnType("decimal(5,2)");
                    entity.Property(e => e.PorcentajeDescuento).HasColumnType("decimal(5,2)");
                    entity.Property(e => e.DetailsJson).HasColumnType("text");
                    // Índice único con filtro para PostgreSQL
                    entity.HasIndex(e => e.SKU).IsUnique().HasFilter("\"SKU\" IS NOT NULL");
                }
                else if (isSQLite)
                {
                    entity.Property(e => e.Price).HasColumnType("REAL");
                    entity.Property(e => e.PrecioCompra).HasColumnType("REAL");
                    entity.Property(e => e.PorcentajeGanancia).HasColumnType("REAL");
                    entity.Property(e => e.PorcentajeDescuento).HasColumnType("REAL");
                    entity.Property(e => e.DetailsJson).HasColumnType("TEXT");
                    // Índice único con filtro para SQLite
                    entity.HasIndex(e => e.SKU).IsUnique().HasFilter("[SKU] IS NOT NULL");
                }
                else
                {
                    // Configuración genérica sin filtro
                    entity.HasIndex(e => e.SKU).IsUnique();
                }
    
                // Índices para búsquedas y filtros
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Category);
                entity.HasIndex(e => e.Brand);
                entity.HasIndex(e => e.Color);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.TieneDescuento);
                entity.HasIndex(e => new { e.FechaInicioDescuento, e.FechaFinDescuento });
        
                // Índices compuestos para optimizar consultas comunes
                entity.HasIndex(e => new { e.IsActive, e.CreatedDate }); // Para GetAllProducts ordenado
                entity.HasIndex(e => new { e.IsActive, e.Category }); // Para filtros por categoría
                entity.HasIndex(e => new { e.IsActive, e.Price }); // Para productos destacados
                entity.HasIndex(e => new { e.IsActive, e.Stock }); // Para más vendidos
            });

            // ApplicationUser
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.PrimerNombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PrimerApellido).IsRequired().HasMaxLength(100);
                entity.Property(e => e.NumeroDocumento).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.NumeroDocumento).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // PedidoEntity
            builder.Entity<PedidoEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                if (isPostgreSQL)
                {
                    entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
                }
                else if (isSQLite)
                {
                    entity.Property(e => e.Total).HasColumnType("REAL");
                }
                
                entity.HasIndex(e => e.ReferenceCode).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Estado);
                entity.HasIndex(e => e.FechaCreacion);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.Pedidos)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // PedidoDetalle
            builder.Entity<PedidoDetalle>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                if (isPostgreSQL)
                {
                    entity.Property(e => e.PrecioUnitario).HasColumnType("decimal(18,2)");
                    entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
                }
                else if (isSQLite)
                {
                    entity.Property(e => e.PrecioUnitario).HasColumnType("REAL");
                    entity.Property(e => e.Subtotal).HasColumnType("REAL");
                }

                entity.HasOne(e => e.Pedido)
                      .WithMany(p => p.Detalles)
                      .HasForeignKey(e => e.PedidoId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Producto)
                      .WithMany(p => p.PedidoDetalles)
                      .HasForeignKey(e => e.ProductoId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // EnvioEntity
            builder.Entity<EnvioEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                if (isPostgreSQL)
                {
                    entity.Property(e => e.CostoEnvio).HasColumnType("decimal(18,2)");
                }
                else if (isSQLite)
                {
                    entity.Property(e => e.CostoEnvio).HasColumnType("REAL");
                }
                
                entity.HasIndex(e => e.NumeroGuia).IsUnique();
                entity.HasIndex(e => e.Estado);

                entity.HasOne(e => e.Pedido)
                      .WithOne(p => p.Envio)
                      .HasForeignKey<EnvioEntity>(e => e.PedidoId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ProductoImagen
            builder.Entity<ProductoImagen>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                entity.HasOne(e => e.Producto)
                      .WithMany(p => p.Imagenes)
                      .HasForeignKey(e => e.ProductoId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // TransaccionEntity
            builder.Entity<TransaccionEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                
                if (isPostgreSQL)
                {
                    entity.Property(e => e.Monto).HasColumnType("decimal(18,2)");
                    entity.Property(e => e.ExtraParametersJson).HasColumnType("text");
                }
                else if (isSQLite)
                {
                    entity.Property(e => e.Monto).HasColumnType("REAL");
                    entity.Property(e => e.ExtraParametersJson).HasColumnType("TEXT");
                }
                
                entity.HasIndex(e => e.TransactionId).IsUnique();
                entity.HasIndex(e => e.Estado);
                entity.HasIndex(e => e.FechaTransaccion);

                entity.HasOne(e => e.Pedido)
                      .WithMany()
                      .HasForeignKey(e => e.PedidoId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // AdminNotification
            builder.Entity<AdminNotification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Leida);
                entity.HasIndex(e => e.FechaCreacion);
            });

            // AdminNotificationRead
            builder.Entity<AdminNotificationRead>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.NotificationId }).IsUnique();
                entity.HasOne(e => e.Notification)
                      .WithMany()
                      .HasForeignKey(e => e.NotificationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // TrafficEvent
            builder.Entity<TrafficEvent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Fecha);
                entity.HasIndex(e => e.Path);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => new { e.Source, e.Medium, e.Campaign }); // Para analytics marketing
                entity.Property(e => e.Path).HasMaxLength(300).IsRequired();
            });

            // MarketingCost - NUEVO para ROI real
            builder.Entity<MarketingCost>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Fecha);
                entity.HasIndex(e => e.Canal);
                entity.HasIndex(e => e.Campana);
                entity.HasIndex(e => new { e.Canal, e.Fecha }); // Para consultas por canal y fecha
                entity.HasIndex(e => e.AplicaDescuento);
                entity.HasIndex(e => e.CategoriaDescuento);
                entity.HasIndex(e => new { e.FechaInicioDescuento, e.FechaFinDescuento });
                
                if (isPostgreSQL)
                {
                    entity.Property(e => e.CostoTotal).HasColumnType("decimal(18,2)");
                    entity.Property(e => e.CostoPorClick).HasColumnType("decimal(18,2)");
                    entity.Property(e => e.PorcentajeDescuento).HasColumnType("decimal(5,2)");
                }
                else if (isSQLite)
                {
                    entity.Property(e => e.CostoTotal).HasColumnType("REAL");
                    entity.Property(e => e.CostoPorClick).HasColumnType("REAL");
                    entity.Property(e => e.PorcentajeDescuento).HasColumnType("REAL");
                }
                
                entity.Property(e => e.Canal).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Campana).HasMaxLength(200).IsRequired();
            });

            // MarketingConfiguration - NUEVO para guardar credenciales de forma segura
            builder.Entity<MarketingConfiguration>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.Canal, e.Plataforma }).IsUnique(); // Solo una configuración por canal/plataforma
                entity.HasIndex(e => e.Activo);
                entity.HasIndex(e => e.Estado);
                entity.HasIndex(e => e.FechaCreacion);
            });

            // Datos iniciales (Seed Data) - solo para desarrollo
            if (isSQLite || isPostgreSQL)
            {
                SeedData(builder);
            }
        }

        private void SeedData(ModelBuilder builder)
        {
            // Productos iniciales (manteniendo los existentes)
            builder.Entity<ProductEntity>().HasData(
                new ProductEntity
                {
                    Id = 1,
                    Name = "Laptop Gaming MSI",
                    Category = "Laptops",
                    Price = 3500000,
                    Brand = "MSI",
                    Component = "Laptop",
                    Color = "Negro",
                    Availability = "Disponible",
                    Stock = 10,
                    StockMinimo = 2,
                    SKU = "MSI-GAM-001",
                    Description = "Laptop gaming de alto rendimiento",
                    ImageUrl = "https://asset.msi.com/resize/image/global/product/product_1_20220519181943_628632df8d5e1.png62405b38c58fe0f07fcdc9d1",
                    DetailsJson = """{""Procesador"":""Intel i7-12700H"",""RAM"":""16GB DDR4"",""Almacenamiento"":""512GB NVMe SSD"",""Tarjeta Gr?fica"":""RTX 3060"",""Pantalla"":""15.6 FHD 144Hz""}""",
                    CreatedDate = DateTime.UtcNow.AddDays(-10),
                },
                new ProductEntity
                {
                    Id = 2,
                    Name = "PC Oficina Dell OptiPlex",
                    Category = "Computadoras de Escritorio",
                    Price = 1500000,
                    Brand = "Dell",
                    Component = "PC",
                    Color = "Negro",
                    Availability = "Disponible",
                    Stock = 15,
                    StockMinimo = 3,
                    SKU = "DELL-OPT-002",
                    Description = "PC para oficina y tareas cotidianas",
                    ImageUrl = "https://i.dell.com/is/image/DellContent/content/dam/ss2/product-images/dell-client-products/desktops/optiplex-desktops/optiplex-3090-micro/media-gallery/optiplex-3090-micro-gallery-1.psd",
                    DetailsJson = """{""Procesador"":""Intel i5-11400"",""RAM"":""8GB DDR4"",""Almacenamiento"":""256GB SSD"",""Puertos"":""USB 3.0, HDMI"",""Sistema"":""Windows 11 Pro""}""",
                    CreatedDate = DateTime.UtcNow.AddDays(-10),
                },
                new ProductEntity
                {
                    Id = 3,
                    Name = "Monitor 4K Samsung",
                    Category = "Monitores",
                    Price = 800000,
                    Brand = "Samsung",
                    Component = "Monitor",
                    Color = "Negro",
                    Availability = "Disponible",
                    Stock = 20,
                    StockMinimo = 5,
                    SKU = "SAM-MON-003",
                    Description = "Monitor 4K de 27 pulgadas",
                    ImageUrl = "https://images.samsung.com/is/image/samsung/p6pim/latin/lu28e590ds-zl/gallery/latin-uhd-monitor-28-lu28e590ds-zl-531938-lu28e590ds-zl-531938-1",
                    DetailsJson = """{""Tama?o"":""27 pulgadas"",""Resoluci?n"":""3840x2160 4K"",""Tecnolog?a"":""IPS"",""Conectividad"":""HDMI, DisplayPort, USB-C"",""HDR"":""HDR10""}""",
                    CreatedDate = DateTime.UtcNow.AddDays(-10),
                },
                new ProductEntity
                {
                    Id = 4,
                    Name = "PC Gaming ASUS ROG",
                    Category = "Computadoras de Escritorio",
                    Price = 4200000,
                    Brand = "ASUS",
                    Component = "PC",
                    Color = "Negro",
                    Availability = "Disponible",
                    Stock = 5,
                    StockMinimo = 1,
                    SKU = "ASUS-ROG-004",
                    Description = "PC Gaming de alto rendimiento para entusiastas",
                    ImageUrl = "https://dlcdnwebimgs.asus.com/gain/285C0B70-5A0D-4C89-995B-A70B82E0F9FE/w800/h600",
                    DetailsJson = """{""Procesador"":""AMD Ryzen 7 5800X"",""RAM"":""32GB DDR4"",""Almacenamiento"":""1TB NVMe SSD"",""Tarjeta Gr?fica"":""NVIDIA RTX 3070"",""Fuente"":""750W 80+ Gold""}""",
                    CreatedDate = DateTime.UtcNow.AddDays(-10),
                },
                new ProductEntity
                {
                    Id = 5,
                    Name = "AMD Ryzen 9 7900X",
                    Category = "Componentes",
                    Price = 1200000,
                    Brand = "AMD",
                    Component = "Procesador",
                    Color = "Gris",
                    Availability = "Disponible",
                    Stock = 12,
                    StockMinimo = 3,
                    SKU = "AMD-RYZ-005",
                    Description = "Procesador de ?ltima generaci?n para gaming y productividad",
                    ImageUrl = "https://c1.neweggimages.com/ProductImageCompressAll1280/19-113-770-V01.jpg",
                    DetailsJson = """{""N?cleos"":""12"",""Hilos"":""24"",""Frecuencia Base"":""4.7 GHz"",""Frecuencia Turbo"":""5.6 GHz"",""Socket"":""AM5"",""TDP"":""170W""}""",
                    CreatedDate = DateTime.UtcNow.AddDays(-10),
                }
            );
        }
    }
}