using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;

namespace ComputerStore
{
    /// <summary>
  /// Clase de diagnóstico para verificar configuración en Azure
    /// </summary>
    public static class DiagnosticStartup
    {
        public static void LogConfiguration(IConfiguration configuration, IWebHostEnvironment environment)
        {
      Console.WriteLine("========================================");
            Console.WriteLine("?? DIAGNÓSTICO DE CONFIGURACIÓN");
            Console.WriteLine("========================================");
            
            // 1. Ambiente
            Console.WriteLine($"\n?? Ambiente: {environment.EnvironmentName}");
            Console.WriteLine($"?? Content Root: {environment.ContentRootPath}");
  
     // 2. Connection String
      var connString = configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(connString))
            {
// Ocultar password para logs
          var safeConnString = connString.Contains("Password=") 
        ? connString.Substring(0, connString.IndexOf("Password=")) + "Password=***" 
    : connString;
    Console.WriteLine($"\n? Connection String encontrado:");
             Console.WriteLine($"   {safeConnString}");
            }
   else
        {
          Console.WriteLine("\n? Connection String NO encontrado en configuración");
    
  // Verificar si existe en variables de entorno
       var envConnString = Environment.GetEnvironmentVariable("SQLAZURECONNSTR_DefaultConnection");
              if (!string.IsNullOrEmpty(envConnString))
                {
        Console.WriteLine("??  Connection String encontrado en variable de entorno SQLAZURECONNSTR_DefaultConnection");
 }
          
                var postgresConnString = Environment.GetEnvironmentVariable("POSTGRESQLCONNSTR_DefaultConnection");
      if (!string.IsNullOrEmpty(postgresConnString))
    {
      Console.WriteLine("??  Connection String encontrado en variable de entorno POSTGRESQLCONNSTR_DefaultConnection");
         var safePostgres = postgresConnString.Contains("Password=") 
         ? postgresConnString.Substring(0, postgresConnString.IndexOf("Password=")) + "Password=***" 
        : postgresConnString;
       Console.WriteLine($"   {safePostgres}");
       }
            }
       
            // 3. Variables de entorno importantes
            Console.WriteLine("\n?? Variables de entorno:");
   var importantVars = new[] 
   { 
   "ASPNETCORE_ENVIRONMENT",
    "PayU__ApiKey",
          "EmailSettings__SmtpServer",
              "Encryption__Key"
            };
            
      foreach (var varName in importantVars)
          {
              var value = configuration[varName.Replace("__", ":")];
           if (!string.IsNullOrEmpty(value))
       {
       var displayValue = varName.Contains("Key") || varName.Contains("Password") 
               ? "***" 
      : value;
        Console.WriteLine($"   ? {varName}: {displayValue}");
          }
  else
                {
         Console.WriteLine($"   ? {varName}: NO CONFIGURADO");
    }
            }
        
            Console.WriteLine("\n========================================\n");
        }
    }
}
