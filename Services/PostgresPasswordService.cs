using Npgsql;

namespace ComputerStore.Services
{
    public interface IPostgresPasswordService
    {
        Task<bool> UpdatePostgresPasswordAsync(string newPassword);
     Task<bool> TestConnectionAsync(string password);
    }

    public class PostgresPasswordService : IPostgresPasswordService
    {
        private readonly ILogger<PostgresPasswordService> _logger;
        private readonly IConfiguration _configuration;

        public PostgresPasswordService(
      ILogger<PostgresPasswordService> logger,
          IConfiguration configuration)
        {
       _logger = logger;
     _configuration = configuration;
    }

        public async Task<bool> UpdatePostgresPasswordAsync(string newPassword)
        {
       try
            {
    // Conexión con credenciales actuales
  var connectionString = "Host=localhost;Port=5432;Database=ComputerStoreDB;Username=postgres;Password=Abcdefghij123;Timeout=10;SSL Mode=Prefer";

     using var connection = new NpgsqlConnection(connectionString);
           await connection.OpenAsync();

           // Cambiar la contraseña del usuario postgres
           var command = new NpgsqlCommand($"ALTER USER postgres WITH PASSWORD '{newPassword}';", connection);
           await command.ExecuteNonQueryAsync();

   _logger.LogInformation("Contraseña de PostgreSQL actualizada exitosamente.");

    // Actualizar appsettings.json
            await UpdateAppSettingsAsync(newPassword);

                return true;
            }
catch (Exception ex)
    {
       _logger.LogError(ex, "Error al actualizar contraseña de PostgreSQL");
           return false;
            }
        }

        public async Task<bool> TestConnectionAsync(string password)
        {
            try
 {
  var testConnectionString = $"Host=localhost;Port=5432;Database=ComputerStoreDB;Username=postgres;Password={password};Timeout=10;SSL Mode=Prefer";

        using var connection = new NpgsqlConnection(testConnectionString);
                await connection.OpenAsync();

                return true;
            }
        catch (Exception ex)
 {
          _logger.LogWarning(ex, "Test de conexión falló con la nueva contraseña");
  return false;
            }
        }

        private async Task UpdateAppSettingsAsync(string newPassword)
      {
     try
     {
       var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

 if (!File.Exists(appSettingsPath))
    {
            _logger.LogWarning("appsettings.json no encontrado en {Path}", appSettingsPath);
     return;
    }

          var json = await File.ReadAllTextAsync(appSettingsPath);
     var jsonDoc = System.Text.Json.JsonDocument.Parse(json);
        
      using var stream = new MemoryStream();
      using var writer = new System.Text.Json.Utf8JsonWriter(stream, new System.Text.Json.JsonWriterOptions { Indented = true });

    writer.WriteStartObject();

           foreach (var property in jsonDoc.RootElement.EnumerateObject())
                {
        if (property.Name == "ConnectionStrings")
  {
      writer.WritePropertyName("ConnectionStrings");
          writer.WriteStartObject();

        foreach (var connString in property.Value.EnumerateObject())
 {
        if (connString.Name == "DefaultConnection")
 {
       var newConnectionString = $"Host=localhost;Database=ComputerStoreDB;Username=postgres;Password={newPassword};Port=5432";
        writer.WriteString("DefaultConnection", newConnectionString);
   }
  else
          {
  writer.WritePropertyName(connString.Name);
      connString.Value.WriteTo(writer);
      }
      }

        writer.WriteEndObject();
        }
  else
     {
        writer.WritePropertyName(property.Name);
       property.Value.WriteTo(writer);
       }
         }

 writer.WriteEndObject();
  await writer.FlushAsync();

      var updatedJson = System.Text.Encoding.UTF8.GetString(stream.ToArray());
    await File.WriteAllTextAsync(appSettingsPath, updatedJson);

       _logger.LogInformation("appsettings.json actualizado exitosamente");
          }
            catch (Exception ex)
   {
             _logger.LogError(ex, "Error al actualizar appsettings.json");
     }
        }
    }
}
