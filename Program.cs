using System;
using System.Collections;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ComputerStore.Data;
using ComputerStore.Middleware;
using ComputerStore.Models;
using ComputerStore.Services;

Log.Logger = new LoggerConfiguration()
 .WriteTo.Console()
 .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// 🔍 DIAGNÓSTICO: Mostrar configuración al inicio
Console.WriteLine("========================================");
Console.WriteLine("🔍 DIAGNÓSTICO DE CONFIGURACIÓN");
Console.WriteLine("========================================");
Console.WriteLine($"🌍 Ambiente: {builder.Environment.EnvironmentName}");
Console.WriteLine($"📁 Content Root: {builder.Environment.ContentRootPath}");

// 🕐 CONFIGURAR ZONA HORARIA PARA COLOMBIA (GMT-5)
Console.WriteLine("🕐 Configurando zona horaria para Colombia (GMT-5)...");
TimeZoneInfo colombiaTimeZone;
try
{
    // Intentar obtener zona horaria de Colombia
    colombiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"); // Windows
    Console.WriteLine("✅ Zona horaria Colombia detectada (Windows): SA Pacific Standard Time");
}
catch
{
    try
    {
        colombiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Bogota"); // Linux/Mac
        Console.WriteLine("✅ Zona horaria Colombia detectada (Linux/Mac): America/Bogota");
    }
    catch
    {
        // Fallback: crear zona horaria manualmente
        colombiaTimeZone = TimeZoneInfo.CreateCustomTimeZone(
            "Colombia Standard Time",
            TimeSpan.FromHours(-5),
            "Colombia Standard Time",
            "Colombia Standard Time"
        );
        Console.WriteLine("✅ Zona horaria Colombia creada manualmente: GMT-5");
    }
}

// Verificar la zona horaria configurada
var horaActualColombia = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, colombiaTimeZone);
Console.WriteLine($"🕐 Hora actual Colombia: {horaActualColombia:yyyy-MM-dd HH:mm:ss} (UTC{colombiaTimeZone.BaseUtcOffset})");

// 🌎 CONFIGURAR CULTURA PARA COLOMBIA
var cultureInfo = new CultureInfo("es-CO");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
Console.WriteLine($"🌎 Cultura configurada: {cultureInfo.DisplayName}");

// 🔍 OBTENER CONNECTION STRING CON DIAGNÓSTICO MEJORADO
Console.WriteLine("\n🔍 Buscando Connection String...");

string? connectionString = null;

// 1. Intentar desde Configuration (appsettings.json o Application Settings)
connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("✅ Connection String encontrado en Configuration.GetConnectionString('DefaultConnection')");
}
else
{
    Console.WriteLine("⚠️ Connection String NO encontrado en Configuration.GetConnectionString");

    // 2. Buscar en variables de entorno con prefijos de Azure
    Console.WriteLine("🔍 Buscando en variables de entorno con prefijos de Azure...");

    var postgresConnString = Environment.GetEnvironmentVariable("POSTGRESQLCONNSTR_DefaultConnection");
    var sqlAzureConnString = Environment.GetEnvironmentVariable("SQLAZURECONNSTR_DefaultConnection");
    var customConnString = Environment.GetEnvironmentVariable("CUSTOMCONNSTR_DefaultConnection");
    var sqlConnString = Environment.GetEnvironmentVariable("SQLCONNSTR_DefaultConnection");
    var rawConnString = Environment.GetEnvironmentVariable("DefaultConnection");

    if (!string.IsNullOrEmpty(postgresConnString))
    {
        Console.WriteLine("✅ Connection String encontrado en POSTGRESQLCONNSTR_DefaultConnection");
        connectionString = postgresConnString;
    }
    else if (!string.IsNullOrEmpty(customConnString))
    {
        Console.WriteLine("✅ Connection String encontrado en CUSTOMCONNSTR_DefaultConnection");
        connectionString = customConnString;
    }
    else if (!string.IsNullOrEmpty(sqlAzureConnString))
    {
        Console.WriteLine("✅ Connection String encontrado en SQLAZURECONNSTR_DefaultConnection");
        connectionString = sqlAzureConnString;
    }
    else if (!string.IsNullOrEmpty(sqlConnString))
    {
        Console.WriteLine("✅ Connection String encontrado en SQLCONNSTR_DefaultConnection");
        connectionString = sqlConnString;
    }
    else if (!string.IsNullOrEmpty(rawConnString))
    {
        Console.WriteLine("✅ Connection String encontrado en DefaultConnection (sin prefijo)");
        connectionString = rawConnString;
    }
    else
    {
        Console.WriteLine("❌ ERROR CRÍTICO: No se encontró Connection String en ninguna ubicación");
        Console.WriteLine("Variables de entorno disponibles relacionadas con conexión:");
        foreach (DictionaryEntry env in Environment.GetEnvironmentVariables())
        {
            var key = env.Key?.ToString() ?? "";
            if (key.Contains("CONNSTR", StringComparison.OrdinalIgnoreCase) ||
                key.Contains("Connection", StringComparison.OrdinalIgnoreCase) ||
                key.Contains("Database", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"  - {key}");
            }
        }
        throw new InvalidOperationException("Connection string 'DefaultConnection' not found in any location.");
    }
}

// Validar y mostrar connection string (sin password)
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is null or empty.");
}

// Convertir formato URI a formato de parámetros si es necesario
if (connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine("⚠️ Connection String en formato URI, convirtiendo a formato de parámetros...");
    connectionString = ConvertPostgreSqlUriToConnectionString(connectionString);
    Console.WriteLine("✅ Connection String convertido");
}

// Mostrar connection string (sin password)
var safeConnString = connectionString.Contains("Password=", StringComparison.OrdinalIgnoreCase)
    ? connectionString.Substring(0, connectionString.IndexOf("Password=", StringComparison.OrdinalIgnoreCase)) + "Password=***"
    : connectionString.Contains("://") && connectionString.Contains("@")
        ? connectionString.Substring(0, connectionString.IndexOf("@")) + "@***"
        : connectionString;
Console.WriteLine($"🔗 Connection String: {safeConnString}");
Console.WriteLine("========================================\n");

// Configurar DbContext con la connection string obtenida
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.CommandTimeout(60); // Aumentar timeout a60 segundos
        npgsqlOptions.EnableRetryOnFailure(
      maxRetryCount:5, // Más intentos de reconexión
 maxRetryDelay: TimeSpan.FromSeconds(10),
  errorCodesToAdd: null
 );
      // Optimizaciones para Supabase
      npgsqlOptions.MaxBatchSize(200); // Aumentar tamaño de batch para consultas
        npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery); // Dividir queries complejas
    });
    
    // Configuraciones adicionales de rendimiento
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    options.EnableDetailedErrors(builder.Environment.IsDevelopment());
});

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// 🛒 CRÍTICO: CONFIGURAR SESIONES PARA EL CARRITO
builder.Services.AddDistributedMemoryCache(); // ✅ Usa memoria en lugar de Redis
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.Name = ".ComputerStore.Session";
});
Console.WriteLine("🛒 Sesiones configuradas para el carrito de compras");

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
    options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
});

// Configurar caché de memoria con límites
builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 100; // Límite de 100 entradas en caché
    options.CompactionPercentage = 0.25; // Liberar 25% cuando se alcance el límite
});
Console.WriteLine("💾 Caché de memoria configurado para optimizar consultas");

// Agregar compresión de respuesta en Program.cs
builder.Services.AddResponseCompression(options =>
{
 options.EnableForHttps = true;
 options.MimeTypes = new[]
 {
 "text/plain",
 "text/css",
 "application/javascript",
 "text/html",
 "application/xml",
 "text/xml",
 "application/json",
 "text/json"
 };
});

// ❌ REDIS DESACTIVADO - Causa error porque no está configurado
// builder.Services.AddStackExchangeRedisCache(options =>
	// {
	//     options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
	//     options.InstanceName = "ComputerStore_";
	// });
Console.WriteLine("⚠️ Redis desactivado - usando memoria para sesiones");

// Servicios personalizados
builder.Services.AddScoped<IPayUService, PayUService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();
builder.Services.AddScoped<EnvioService>();
builder.Services.AddScoped<LittleCarService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<IPostgresPasswordService, PostgresPasswordService>();
// Registrar el servicio RedisCacheService
builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();

// Servicios de marketing y encriptación
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IMarketingIntegrationService, MarketingIntegrationService>();
builder.Services.AddHttpClient<MarketingIntegrationService>();

// Configurar HttpClient para servicios externos
builder.Services.AddHttpClient();

// Registrar el servicio JsonExportService
builder.Services.AddScoped<JsonExportService>();

Log.Logger = new LoggerConfiguration()
 .WriteTo.Console()
 .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

// Usar compresión de respuesta
app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Configurar Excel antes del routing
app.UseExcelConfiguration();

app.UseRouting();

// 🛒 CRÍTICO: USAR SESIONES ANTES DE AUTHENTICATION
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Tracking de tráfico (después de auth para capturar UserId)
app.UseTrafficTracking();

app.MapRazorPages();

// 🔧 INICIALIZAR BASE DE DATOS Y CONFIGURAR ZONA HORARIA
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        // Configurar zona horaria en PayUService
        var payUService = services.GetRequiredService<IPayUService>();
        payUService.ConfigurarZonaHoraria(colombiaTimeZone);

        // Verificar configuración
        var fechaColombiaServicio = payUService.ObtenerFechaColombiaActual();
        Console.WriteLine($"✅ PayUService configurado - Fecha Colombia: {fechaColombiaServicio:yyyy-MM-dd HH:mm:ss}");
        logger.LogInformation("PayUService configurado - Fecha Colombia: {FechaColombia}", fechaColombiaServicio);

        // Verificar conexión a base de datos
        logger.LogInformation("🔍 Verificando conexión a base de datos...");
        Console.WriteLine("🔍 Verificando conexión a base de datos...");

        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        var canConnect = await dbContext.Database.CanConnectAsync();

        if (canConnect)
        {
            Console.WriteLine("✅ Conexión a base de datos exitosa");
            logger.LogInformation("✅ Conexión a base de datos exitosa");

            await DatabaseInitializer.InitializeAsync(services, colombiaTimeZone);
            Console.WriteLine("🗄️ Base de datos inicializada correctamente");
            logger.LogInformation("Base de datos inicializada correctamente");
        }
        else
        {
            Console.WriteLine("❌ No se pudo conectar a la base de datos");
            logger.LogError("❌ No se pudo conectar a la base de datos");

            if (app.Environment.IsProduction())
            {
                logger.LogWarning("⚠️ La aplicación continuará sin inicialización de base de datos");
                Console.WriteLine("⚠️ La aplicación continuará sin inicialización de base de datos");
            }
            else
            {
                throw new Exception("No se pudo conectar a la base de datos");
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Error durante la inicialización de la base de datos");
        Console.WriteLine($"❌ Error: {ex.Message}");
        Console.WriteLine($"📋 Stack Trace: {ex.StackTrace}");

        // En producción, no detenemos la app, pero logueamos el error
        if (app.Environment.IsProduction())
        {
            logger.LogWarning("⚠️ La aplicación continuará sin inicialización de base de datos");
            Console.WriteLine("⚠️ La aplicación continuará sin inicialización de base de datos");
        }
        else
        {
            throw; // En desarrollo, propagamos el error
        }
    }
}

// Generar el archivo JSON al iniciar la aplicación
using (var scope = app.Services.CreateScope())
{
 var jsonExportService = scope.ServiceProvider.GetRequiredService<JsonExportService>();
 await jsonExportService.GenerateProductsJsonAsync();
 Console.WriteLine("✅ Archivo JSON generado en wwwroot/data/products.json");
}

Console.WriteLine("🚀 Aplicación iniciada correctamente");
Console.WriteLine($"🌐 URLs disponibles:");
Console.WriteLine($"   - https://localhost:7108");
Console.WriteLine($"   - http://localhost:5108");
Console.WriteLine($"👤 Credenciales Admin: Admin / E_commerce123$");
Console.WriteLine("🛒 Carrito de compras configurado y funcionando");
Console.WriteLine("📊 Excel: Configuración automática habilitada");
Console.WriteLine("⚡ Optimizaciones de rendimiento: Paginación BD + Caché habilitado");

app.Run();

// Método helper para convertir URI de PostgreSQL a formato de parámetros
static string ConvertPostgreSqlUriToConnectionString(string uri)
{
    try
    {
        // postgresql://username:password@host:port/database
        var uriObj = new Uri(uri);

        var userInfo = uriObj.UserInfo.Split(':');
        var username = userInfo[0];
        var password = userInfo.Length > 1 ? userInfo[1] : "";
        var host = uriObj.Host;
        var port = uriObj.Port > 0 ? uriObj.Port : 5432;
        var database = uriObj.AbsolutePath.TrimStart('/');

        return $"Host={host};Port={port};Database={database};Username={username};Password={password};Pooling=true;SSL Mode=Require;Trust Server Certificate=true";
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Error al convertir URI: {ex.Message}");
        return uri; // Devolver el original si falla
    }
}

//CwJ4jyBHT2FBX_x