// ExcelConfigurationMiddleware.cs
using OfficeOpenXml;

namespace ComputerStore.Middleware
{
    public class ExcelConfigurationMiddleware
    {
        private readonly RequestDelegate _next;
        private static bool _isConfigured = false;
        private static readonly object _lock = new object();

        public ExcelConfigurationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!_isConfigured)
            {
                lock (_lock)
                {
                    if (!_isConfigured)
                    {
                        try
                        {
                            // Configurar EPPlus 5.x
                            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                            
                            // Test básico
                            using (var testPackage = new ExcelPackage())
                            {
                                var testWorksheet = testPackage.Workbook.Worksheets.Add("Test");
                                testWorksheet.Cells[1, 1].Value = "Test";
                                var testBytes = testPackage.GetAsByteArray();
                                
                                if (testBytes != null && testBytes.Length > 0)
                                {
                                    Console.WriteLine("?? EPPlus 5.x configurado y funcionando correctamente");
                                }
                                else
                                {
                                    Console.WriteLine("?? EPPlus configurado pero con problemas de generación");
                                }
                            }
                            
                            _isConfigured = true;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"? Error configurando EPPlus: {ex.Message}");
                            _isConfigured = true; // Marcar como configurado para evitar intentos repetidos
                        }
                    }
                }
            }

            await _next(context);
        }
    }

    public static class ExcelConfigurationMiddlewareExtensions
    {
        public static IApplicationBuilder UseExcelConfiguration(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExcelConfigurationMiddleware>();
        }
    }
}