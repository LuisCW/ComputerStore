using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Npgsql;

namespace ComputerStore.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class TestDatabaseModel : PageModel
    {
        private readonly IConfiguration _configuration;
      private readonly ILogger<TestDatabaseModel> _logger;

        public string? ConnectionString { get; set; }
        public bool? ConnectionSuccessful { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> Tables { get; set; } = new();
   public Dictionary<string, long> TableCounts { get; set; } = new();
        public Dictionary<string, string> Migrations { get; set; } = new();

   public TestDatabaseModel(
IConfiguration configuration,
            ILogger<TestDatabaseModel> logger)
      {
     _configuration = configuration;
          _logger = logger;
        }

        public void OnGet()
        {
    ConnectionString = _configuration.GetConnectionString("DefaultConnection");
        }

     public async Task<IActionResult> OnPostAsync()
        {
            ConnectionString = _configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(ConnectionString))
            {
     ConnectionSuccessful = false;
 ErrorMessage = "Connection string 'DefaultConnection' no encontrado en configuración.";
 return Page();
      }

         try
      {
      _logger.LogInformation("Probando conexión a base de datos...");
     
  using var connection = new NpgsqlConnection(ConnectionString);
           await connection.OpenAsync();

  ConnectionSuccessful = true;
       _logger.LogInformation("? Conexión exitosa");

    // Obtener lista de tablas
         await GetTablesAsync(connection);

             // Contar registros en tablas principales
           await CountRecordsAsync(connection);

     // Obtener migraciones
    await GetMigrationsAsync(connection);
    }
      catch (Exception ex)
            {
      ConnectionSuccessful = false;
 ErrorMessage = $"Tipo: {ex.GetType().Name}\n\n" +
        $"Mensaje: {ex.Message}\n\n" +
$"Stack Trace:\n{ex.StackTrace}";
         
  _logger.LogError(ex, "? Error al conectar a la base de datos");

// Log adicional para debugging
 if (ex.InnerException != null)
   {
   ErrorMessage += $"\n\nInner Exception:\n{ex.InnerException.Message}";
                }
         }

            return Page();
        }

        private async Task GetTablesAsync(NpgsqlConnection connection)
        {
          try
            {
        var query = @"
        SELECT table_name 
    FROM information_schema.tables 
   WHERE table_schema = 'public' 
 ORDER BY table_name;";

   using var cmd = new NpgsqlCommand(query, connection);
     using var reader = await cmd.ExecuteReaderAsync();

     while (await reader.ReadAsync())
    {
       Tables.Add(reader.GetString(0));
  }

          _logger.LogInformation($"?? Encontradas {Tables.Count} tablas");
   }
            catch (Exception ex)
       {
         _logger.LogWarning(ex, "No se pudieron obtener las tablas");
         }
  }

        private async Task CountRecordsAsync(NpgsqlConnection connection)
        {
            var tablesToCount = new[]
 {
   "AspNetUsers",
       "AspNetRoles",
  "Productos",
        "Categorias",
        "Pedidos",
          "PedidoItems",
        "CarritoItems",
     "Envios",
     "Transacciones",
      "__EFMigrationsHistory"
        };

       foreach (var tableName in tablesToCount)
      {
   try
       {
        var query = $"SELECT COUNT(*) FROM \"{tableName}\"";
        using var cmd = new NpgsqlCommand(query, connection);
           var count = (long)(await cmd.ExecuteScalarAsync() ?? 0L);
      TableCounts[tableName] = count;
         _logger.LogInformation($"  {tableName}: {count} registros");
    }
       catch (Exception ex)
        {
          TableCounts[tableName] = -1; // Indica error
    _logger.LogWarning($"  {tableName}: Tabla no existe o error - {ex.Message}");
 }
 }
}

        private async Task GetMigrationsAsync(NpgsqlConnection connection)
        {
      try
            {
     var query = @"
    SELECT ""MigrationId"", ""ProductVersion"" 
      FROM ""__EFMigrationsHistory"" 
  ORDER BY ""MigrationId""";

              using var cmd = new NpgsqlCommand(query, connection);
        using var reader = await cmd.ExecuteReaderAsync();

      while (await reader.ReadAsync())
                {
    var migrationId = reader.GetString(0);
           var productVersion = reader.GetString(1);
         Migrations[migrationId] = productVersion;
  }

           _logger.LogInformation($"?? Encontradas {Migrations.Count} migraciones");
 }
       catch (Exception ex)
  {
  _logger.LogWarning(ex, "No se pudieron obtener las migraciones");
       }
        }
    }
}
