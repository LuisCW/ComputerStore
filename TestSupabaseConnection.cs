using Npgsql;
using System;
using System.Threading.Tasks;

namespace ComputerStore
{
    /// <summary>
    /// Herramienta de diagnóstico para probar conexión a Supabase
    /// Ejecutar: dotnet run --project TestSupabaseConnection.cs
    /// </summary>
    public class TestSupabaseConnection
    {
        public static async Task Main(string[] args)
        {
   Console.WriteLine("?? DIAGNÓSTICO DE CONEXIÓN A SUPABASE\n");
     
            var connectionString = "Host=db.xakfuxhafqbelwankypo.supabase.co;Database=postgres;Username=postgres;Password=CwJ4jyBHT2FBX_x;Port=5432;SSL Mode=Require;Trust Server Certificate=true";
      
      try
       {
        Console.WriteLine("?? Conectando a Supabase...");
    using var connection = new NpgsqlConnection(connectionString);
   await connection.OpenAsync();
        
    Console.WriteLine("? Conexión exitosa!\n");
      
       // Listar tablas
    Console.WriteLine("?? Tablas en la base de datos:");
                var tableQuery = @"
        SELECT table_name 
                FROM information_schema.tables 
             WHERE table_schema = 'public' 
         ORDER BY table_name;";
        
 using var tableCmd = new NpgsqlCommand(tableQuery, connection);
         using var tableReader = await tableCmd.ExecuteReaderAsync();
          
          while (await tableReader.ReadAsync())
       {
             Console.WriteLine($"  - {tableReader.GetString(0)}");
     }
     tableReader.Close();
  
           // Contar registros
    Console.WriteLine("\n?? Registros en tablas principales:");
       var countQueries = new[]
            {
            ("AspNetUsers", "SELECT COUNT(*) FROM \"AspNetUsers\""),
          ("Productos", "SELECT COUNT(*) FROM \"Productos\""),
  ("Categorias", "SELECT COUNT(*) FROM \"Categorias\""),
      ("Pedidos", "SELECT COUNT(*) FROM \"Pedidos\""),
         ("__EFMigrationsHistory", "SELECT COUNT(*) FROM \"__EFMigrationsHistory\"")
    };
          
        foreach (var (tableName, query) in countQueries)
     {
         try
           {
           using var cmd = new NpgsqlCommand(query, connection);
  var count = await cmd.ExecuteScalarAsync();
     Console.WriteLine($"  {tableName}: {count} registros");
  }
          catch (Exception ex)
    {
              Console.WriteLine($"  {tableName}: ??  Tabla no existe o error - {ex.Message}");
        }
                }
    
   // Verificar migraciones
                Console.WriteLine("\n?? Migraciones aplicadas:");
     try
     {
                var migrationQuery = "SELECT \"MigrationId\", \"ProductVersion\" FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\"";
       using var migCmd = new NpgsqlCommand(migrationQuery, connection);
       using var migReader = await migCmd.ExecuteReaderAsync();
       
        while (await migReader.ReadAsync())
  {
       Console.WriteLine($"  - {migReader.GetString(0)} (EF Core {migReader.GetString(1)})");
              }
              }
    catch (Exception ex)
    {
          Console.WriteLine($"  ??  No se pudieron leer las migraciones: {ex.Message}");
      }

  Console.WriteLine("\n? Diagnóstico completado exitosamente!");
            }
     catch (Exception ex)
    {
                Console.WriteLine($"\n? ERROR DE CONEXIÓN:");
     Console.WriteLine($"Tipo: {ex.GetType().Name}");
          Console.WriteLine($"Mensaje: {ex.Message}");
      Console.WriteLine($"\nDetalles completos:");
            Console.WriteLine(ex.ToString());
       }
        }
    }
}
