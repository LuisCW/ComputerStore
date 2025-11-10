using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using ComputerStore.Models;
using ComputerStore.Services;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace ComputerStore.Pages
{
    public class PayUDiagnosticModel : PageModel
    {
        private readonly IPayUService _payUService;
        private readonly PayUSettings _payUSettings;
        private readonly HttpClient _httpClient;
        private readonly ILogger<PayUDiagnosticModel> _logger;

        public PayUSettings PayUConfiguration { get; set; }
        public ConnectivityResult ConnectivityTestResult { get; set; }
        public PaymentTestResult PaymentTestResult { get; set; }
        public string? PaymentResult { get; set; }

        public PayUDiagnosticModel(
            IPayUService payUService, 
            IOptions<PayUSettings> payUSettings, 
            HttpClient httpClient,
            ILogger<PayUDiagnosticModel> logger)
        {
            _payUService = payUService;
            _payUSettings = payUSettings.Value;
            _httpClient = httpClient;
            _logger = logger;
        }

        public void OnGet()
        {
            PayUConfiguration = _payUSettings;
        }

        public async Task<IActionResult> OnPostTestConnectivityAsync()
        {
            PayUConfiguration = _payUSettings;

            try
            {
                _logger.LogInformation("Iniciando prueba de conectividad desde página de diagnóstico...");
                
                var resultado = await _payUService.ProbarConectividadPayU();
                
                ConnectivityTestResult = new ConnectivityResult
                {
                    Success = resultado.Success,
                    Message = resultado.Success 
                        ? $"? {resultado.Message}" 
                        : $"? {resultado.Message}",
                    Details = resultado.Details
                };
                
                _logger.LogInformation($"Resultado de prueba: Success={resultado.Success}, Message={resultado.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en prueba de conectividad");
                ConnectivityTestResult = new ConnectivityResult
                {
                    Success = false,
                    Message = $"? Error inesperado: {ex.Message}",
                    Details = ex.ToString()
                };
            }

            return Page();
        }

        public async Task<IActionResult> OnPostTestPaymentAsync()
        {
            PayUConfiguration = _payUSettings;

            try
            {
                // Crear un pedido de prueba
                var pedidoPrueba = new Pedido
                {
                    ReferenceCode = $"TEST_{DateTime.UtcNow:yyyyMMddHHmmss}",
                    Description = "Pago de prueba - Diagnóstico PayU",
                    Amount = 1000m,
                    BuyerName = "Usuario de Prueba",
                    BuyerEmail = "test@compuhipermegared.com"
                };

                _logger.LogInformation("Iniciando prueba de pago con PayU...");

                // Usar parámetros correctos según documentación oficial PayU
                var pruebaAprobada = new
                {
                    numeroTarjeta = "4097440000000004",
                    cvv = "777", // CVV correcto para aprobadas
                    expiracionMes = "05", // Menor que 6
                    expiracionAnio = "2025",
                    tipoTarjeta = "VISA",
                    nombreTarjeta = "APPROVED" // Nombre correcto
                };

                var pruebaRechazada = new
                {
                    numeroTarjeta = "4097440000000004", 
                    cvv = "666", // CVV para rechazadas
                    expiracionMes = "07", // Mayor que 6
                    expiracionAnio = "2025",
                    tipoTarjeta = "VISA",
                    nombreTarjeta = "REJECTED" // Nombre correcto
                };
                
                var pruebaPendiente = new
                {
                    numeroTarjeta = "4111111111111111",
                    cvv = "777", // CVV para pendientes
                    expiracionMes = "05", // Menor que 6
                    expiracionAnio = "2025",
                    tipoTarjeta = "VISA",
                    nombreTarjeta = "PENDING", // Nombre correcto
                    email = "manual-review-hub@email.com" // Email especial para pendientes
                };

                ViewData["PruebaAprobada"] = pruebaAprobada;
                ViewData["PruebaRechazada"] = pruebaRechazada;
                ViewData["PruebaPendiente"] = pruebaPendiente;

                // Realizar pago de prueba con parámetros correctos para transacción aprobada
                var resultado = await _payUService.RealizarPago(
                    pedidoPrueba,
                    "TARJETA",
                    "4097440000000004", // Tarjeta VISA de prueba
                    "777", // CVV correcto para aprobadas
                    "05", // Mes menor que 6
                    "2025", // Año futuro
                    1000m,
                    "VISA",
                    null, null, null, null,
                    "APPROVED" // Nombre que debería generar transacción aprobada
                );

                // Analizar el resultado
                if (resultado.Contains("transactionState=APPROVED"))
                {
                    PaymentTestResult = new PaymentTestResult
                    {
                        Success = true,
                        TransactionState = "APPROVED",
                        Message = "? Pago de prueba procesado correctamente. PayU está funcionando y aplicando reglas de testing."
                    };
                }
                else if (resultado.Contains("transactionState=PENDING"))
                {
                    PaymentTestResult = new PaymentTestResult
                    {
                        Success = true,
                        TransactionState = "PENDING",
                        Message = "? Pago pendiente. PayU está respondiendo correctamente."
                    };
                }
                else if (resultado.Contains("transactionState=DECLINED"))
                {
                    PaymentTestResult = new PaymentTestResult
                    {
                        Success = true,
                        TransactionState = "DECLINED",
                        Message = "? Pago rechazado, pero PayU está respondiendo correctamente."
                    };
                }
                else if (resultado.Contains("ERROR"))
                {
                    PaymentTestResult = new PaymentTestResult
                    {
                        Success = false,
                        TransactionState = "ERROR",
                        Message = "?? Error en el procesamiento del pago. Verificar configuración."
                    };
                }
                else
                {
                    PaymentTestResult = new PaymentTestResult
                    {
                        Success = false,
                        TransactionState = "UNKNOWN",
                        Message = $"? Respuesta inesperada: {resultado}"
                    };
                }

                _logger.LogInformation($"Resultado de prueba de pago: {PaymentTestResult.TransactionState}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en prueba de pago");
                PaymentTestResult = new PaymentTestResult
                {
                    Success = false,
                    TransactionState = "ERROR",
                    Message = $"?? Error durante la prueba: {ex.Message}"
                };
            }

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            PayUConfiguration = _payUSettings;

            if (Request.Method == "POST")
            {
                var action = Request.Form["action"];
                
                if (action == "connectivity")
                {
                    var result = await _payUService.ProbarConectividadPayU();
                    ConnectivityTestResult = new ConnectivityResult
                    {
                        Success = result.Success,
                        Message = result.Success 
                            ? $"? {result.Message}" 
                            : $"? {result.Message}",
                        Details = result.Details
                    };
                }
                else if (action == "testPayment")
                {
                    return await RealizarPruebaPago("4097440000000004", "777", "APPROVED", "VISA", "05");
                }
                else if (action == "testPSE")
                {
                    return await RealizarPruebaPSE();
                }
                else if (action == "testSpecific")
                {
                    var tipo = Request.Form["tipo"];
                    
                    return tipo.ToString() switch
                    {
                        // Pruebas según documentación oficial PayU
                        "APROBADA" => await RealizarPruebaPago("4097440000000004", "777", "APPROVED", "VISA", "05"),
                        "PENDIENTE" => await RealizarPruebaPago("4111111111111111", "777", "PENDING", "VISA", "05", "manual-review-hub@email.com"),
                        "RECHAZADA" => await RealizarPruebaPago("4097440000000004", "666", "REJECTED", "VISA", "07"),
                        "CVV666_APRO" => await RealizarPruebaPago("4097440000000004", "666", "APPROVED", "VISA", "07"),
                        "CVV666_CALL" => await RealizarPruebaPago("4097440000000004", "666", "REJECTED", "VISA", "07"),
                        "PSE_TEST" => await RealizarPruebaPSE(),
                        _ => Page()
                    };
                }
            }

            return Page();
        }

        private async Task<IActionResult> RealizarPruebaPSE()
        {
            try
            {
                var pedidoPrueba = new Pedido
                {
                    ReferenceCode = $"PSE_TEST_{Guid.NewGuid().ToString("N")[..8]}",
                    Description = "Prueba PSE - Diagnóstico PayU",
                    Amount = 25000, // Monto mínimo para PSE
                    BuyerName = "Cliente PSE Prueba",
                    BuyerEmail = "pse-test@compuhipermegared.com"
                };

                _logger.LogInformation("Iniciando prueba PSE específica...");

                var resultado = await _payUService.RealizarPago(
                    pedidoPrueba,
                    "PSE",
                    null, // No hay tarjeta en PSE
                    null, // No hay CVV en PSE
                    null, // No hay mes en PSE
                    null, // No hay año en PSE
                    25000,
                    null, // No hay tipo de tarjeta en PSE
                    "123456789", // Documento para PSE
                    "1022", // Banco de prueba
                    null, null,
                    "PSE_USER"
                );

                if (!string.IsNullOrEmpty(resultado))
                {
                    return Redirect(resultado);
                }

                ViewData["TestResult"] = "ERROR: PSE no devolvió respuesta válida";
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en prueba de PSE");
                ViewData["TestResult"] = $"ERROR PSE: {ex.Message}";
                return Page();
            }
        }

        private async Task<IActionResult> RealizarPruebaPago(string numeroTarjeta, string cvv, string nombreTarjeta, 
            string tipoTarjeta, string mes = "12", string emailEspecial = null)
        {
            try
            {
                var pedidoPrueba = new Pedido
                {
                    ReferenceCode = $"TEST_{Guid.NewGuid().ToString("N")[..8]}",
                    Description = "Prueba de conectividad PayU",
                    Amount = 50000, // $50,000 COP para prueba
                    BuyerName = "Cliente Prueba",
                    BuyerEmail = emailEspecial ?? "test@compuhipermegared.com"
                };

                _logger.LogInformation($"Iniciando prueba: {nombreTarjeta} con CVV {cvv}");

                var resultado = await _payUService.RealizarPago(
                    pedidoPrueba,
                    "TARJETA",
                    numeroTarjeta,
                    cvv,
                    mes,
                    "2025",
                    50000,
                    tipoTarjeta,
                    null, null, null, null,
                    nombreTarjeta
                );

                if (!string.IsNullOrEmpty(resultado))
                {
                    return Redirect(resultado);
                }

                ViewData["TestResult"] = "ERROR: PayU no devolvió respuesta válida";
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en prueba de pago");
                ViewData["TestResult"] = $"ERROR: {ex.Message}";
                return Page();
            }
        }
    }

    public class ConnectivityResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Details { get; set; } // Agregado para detalles adicionales
    }

    public class PaymentTestResult
    {
        public bool Success { get; set; }
        public string TransactionState { get; set; }
        public string Message { get; set; }
    }
}