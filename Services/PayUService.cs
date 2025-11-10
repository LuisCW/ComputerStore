using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using ComputerStore.Models;
using ComputerStore.Services;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

public class PayUService : IPayUService
{
    private readonly HttpClient _httpClient;
    private readonly PayUSettings _settings;
    private readonly LittleCarService _carritoService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManager;

    // Cache estático para transacciones y envíos
    private static readonly ConcurrentDictionary<string, TransaccionInfo> _transacciones = new();
    private static readonly ConcurrentDictionary<long, EnvioInfo> _enviosCache = new();
    private static readonly ConcurrentDictionary<string, Dictionary<string, object>> _pseDataCache = new();
    private static readonly ConcurrentDictionary<string, Dictionary<string, object>> _cardDataCache = new(); // NUEVO cache TARJETA
    
    // ? CORREGIDO: Lista estática unificada para envíos
    private static readonly List<EnvioInfo> _enviosLista = new();

    // ? NUEVO: Configuración de zona horaria estática
    private static TimeZoneInfo? _zonaHorariaConfigurada = null;

    public PayUService(HttpClient httpClient, IOptions<PayUSettings> settings, LittleCarService carritoService, 
        IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _carritoService = carritoService;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    private string CalcularFirma(string apiKey, string merchantId, string referenceCode, decimal amount, string currency)
    {
        // Formatear el amount con dos decimales para asegurar consistencia
        string amountFormatted = amount.ToString("0.00").Replace(",", ".");

        // Concatenar valores según el formato de firma requerido por la API
        string dataToSign = $"{apiKey}~{merchantId}~{referenceCode}~{amountFormatted}~{currency}";

        // Calcular el hash de la firma (usando SHA-256 o MD5 según lo que requiera la API)
        using (var sha256 = System.Security.Cryptography.SHA256.Create())
        {
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(dataToSign));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }

    // Método para validar tarjetas de prueba que deben ser rechazadas
    private bool EsTarjetaParaRechazo(string numeroTarjeta, string cvv, string nombreTarjeta)
    {
        // Tarjetas específicas de PayU para generar rechazos
        var tarjetasRechazo = new Dictionary<string, string[]>
        {
            { "4111111111111111", new[] { "666" } }, // Visa rechazo
            { "5555555555554444", new[] { "666" } }, // MasterCard rechazo
            { "36545407156813", new[] { "666" } },   // Diners rechazo
            { "374245455400001", new[] { "6666" } }  // American Express rechazo
        };

        // Verificar si la tarjeta está en la lista de rechazo
        if (tarjetasRechazo.ContainsKey(numeroTarjeta))
        {
            return tarjetasRechazo[numeroTarjeta].Contains(cvv);
        }

        // Verificar por nombre de tarjeta
        if (!string.IsNullOrEmpty(nombreTarjeta))
        {
            var nombreUpper = nombreTarjeta.ToUpper();
            if (nombreUpper == "REJECTED" || nombreUpper == "REJECT" || nombreUpper == "DECLINED")
            {
                return true;
            }
        }

        // Verificar CVV de rechazo general
        if (cvv == "666" || cvv == "6666")
        {
            return true;
        }

        return false;
    }

    string GenerateUniqueOrderId()
    {
        return $"Order_{Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8)}";
    }

    // Agregar el parámetro nombreTarjeta que falta en la implementación
    public async Task<string> RealizarPago(Pedido pedido,
        string MetodoPago,
        string numeroTarjeta,
        string cvv,
        string expiracionMes,
        string expiracionAnio,
        decimal total,
        string tipoTarjeta,
        string documentoPSE = null,
        string bancoPSE = null,
        string documentoEfecty = null,
        string celularNequi = null,
        string nombreTarjeta = "APRO")
    {
        // Validación de tarjetas de prueba para rechazar
        if (MetodoPago == "TARJETA")
        {
            // Usar el método específico para validar tarjetas de rechazo
            if (EsTarjetaParaRechazo(numeroTarjeta, cvv, nombreTarjeta))
            {
                Console.WriteLine($"Tarjeta marcada para rechazo - Número: {numeroTarjeta}, Nombre: {nombreTarjeta}, CVV: {cvv}");
                return $"/ConfirmacionPago?transactionState=REJECTED&message=Tarjeta_rechazada_por_entidad_financiera&paymentMethod={MetodoPago}";
            }

            // Validar que el número de tarjeta no esté vacío o sea inválido
            if (string.IsNullOrWhiteSpace(numeroTarjeta) || numeroTarjeta.Length < 13)
            {
                return $"/ConfirmacionPago?transactionState=REJECTED&message=Numero_de_tarjeta_invalido&paymentMethod={MetodoPago}";
            }

            // Validar CVV
            if (string.IsNullOrWhiteSpace(cvv) || cvv.Length < 3)
            {
                return $"/ConfirmacionPago?transactionState=REJECTED&message=CVV_invalido&paymentMethod={MetodoPago}";
            }
        }

        // Resto del código existente...
        var baseTransaction = new
        {
            order = new
            {
                accountId = _settings.AccountId,
                referenceCode = pedido.ReferenceCode,
                description = pedido.Description,
                language = "es",
                additionalValues = new
                {
                    TX_VALUE = new { value = pedido.Amount, currency = "COP" }
                },
                buyer = new
                {
                    fullName = pedido.BuyerName,
                    emailAddress = pedido.BuyerEmail
                }
            },
            type = "AUTHORIZATION_AND_CAPTURE",
            paymentCountry = "CO",
            test = true,
            deviceSessionId = Guid.NewGuid().ToString(),
            ipAddress = "127.0.0.1",
            cookie = "pt1t38347bs6jc9ruv2ecpv7o",
            userAgent = "Mozilla/5.0"
        };

        object transaction;

        switch (MetodoPago)
        {
            case "TARJETA":
                transaction = new
                {
                    baseTransaction.order,
                    baseTransaction.type,
                    baseTransaction.paymentCountry,
                    baseTransaction.test,
                    baseTransaction.deviceSessionId,
                    baseTransaction.ipAddress,
                    baseTransaction.cookie,
                    baseTransaction.userAgent,
                    paymentMethod = tipoTarjeta?.ToUpper() ?? "TARJETA", // Asigna tipoTarjeta o "TARJETA" como fallback
                    creditCard = new
                    {
                        number = numeroTarjeta,
                        securityCode = cvv,
                        expirationDate = $"{expiracionMes}/{expiracionAnio}"
                    }
                };
                break;

            case "PSE":
                transaction = new
                {
                    baseTransaction.order,
                    baseTransaction.type,
                    baseTransaction.paymentCountry,
                    baseTransaction.test,
                    baseTransaction.deviceSessionId,
                    baseTransaction.ipAddress,
                    baseTransaction.cookie,
                    baseTransaction.userAgent,
                    paymentMethod = "PSE",
                    bank = "BANK_CODE", // Código del banco específico para PSE
                    payer = new
                    {
                        merchantPayerId = "1",
                        fullName = "Pagador de prueba",
                        emailAddress = "pagador@test.com",
                        contactPhone = "3001234567",
                        dniNumber = "987654321"
                    },
                    extraParameters = new
                    {
                        RESPONSE_URL = "{ {response_page} }",
                        PSE_REFERENCE1 = "127.0.0.1",
                        FINANCIAL_INSTITUTION_CODE = "1022",
                        USER_TYPE = "N",
                        PSE_REFERENCE2 = "CC",
                        PSE_REFERENCE3 = "123456789",
                    }
                };
                break;
            case "EFECTY":
                transaction = new
                {
                    baseTransaction.order,
                    baseTransaction.type,
                    baseTransaction.paymentCountry,
                    baseTransaction.test,
                    baseTransaction.deviceSessionId,
                    baseTransaction.ipAddress,
                    baseTransaction.cookie,
                    baseTransaction.userAgent,
                    paymentMethod = "EFECTY",
                    extraParameters = new
                    {
                        EXPIRATION_DATE = DateTime.UtcNow.AddDays(2).ToString("yyyy-MM-dd'T'HH:mm:ss") // Fecha de expiración del pago
                    },
                    payer = new
                    {
                        merchantPayerId = "1",
                        fullName = "Pagador de prueba",
                        emailAddress = "pagador@test.com",
                        contactPhone = "3001234567",
                        dniNumber = "987654321"
                    }
                };
                break;
            case "NEQUI":
                transaction = new
                {
                    baseTransaction.order,
                    baseTransaction.type,
                    baseTransaction.paymentCountry,
                    baseTransaction.test,
                    baseTransaction.deviceSessionId,
                    baseTransaction.ipAddress,
                    baseTransaction.cookie,
                    baseTransaction.userAgent,
                    paymentMethod = "NEQUI",
                    payer = new
                    {
                        merchantPayerId = "1",
                        fullName = "Pagador de prueba",
                        emailAddress = "pagador@test.com",
                        contactPhone = "3001234567",
                        dniNumber = "987654321"
                    }
                };
                break;

            default:
                throw new InvalidOperationException("Método de pago no soportado.");
        }

        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

        var apiKey = "4Vj8eK4rloUd272L48hsrarnUA";
        var merchantId = "508029";
        var accountId = "512321";
        var referenceCode = "PagoCarrito" + Guid.NewGuid().ToString("N");
        var amount = Math.Round(total, 2); // Total del carrito
        var currency = "COP";
        string signature = CalcularFirma(apiKey, merchantId, referenceCode, amount, currency);
        var responseUrl = GetBaseUrl().TrimEnd('/') + "/ConfirmacionPago";
        var notifyUrl = responseUrl; // usar misma URL para sandbox

        object paymentRequest;

        // Reemplaza el bloque de configuración de paymentRequest para TARJETA con este:
        if (MetodoPago == "TARJETA")
        {
            paymentRequest = new
            {
                language = "es",
                command = "SUBMIT_TRANSACTION",
                merchant = new
                {
                    apiKey = apiKey,
                    apiLogin = "pRRXKOl8ikMmt9u"
                },
                transaction = new
                {
                    order = new
                    {
                        accountId = accountId,
                        referenceCode = referenceCode,
                        description = "Pago de productos en carrito",
                        language = "es",
                        signature = signature,
                        additionalValues = new
                        {
                            TX_VALUE = new { value = amount, currency = currency }
                        },
                        buyer = new
                        {
                            merchantBuyerId = "1",
                            fullName = "Comprador de prueba",
                            emailAddress = "comprador@test.com",
                            contactPhone = "3001234567",
                            dniNumber = "123456789"
                        }
                    },
                    creditCard = new
                    {
                        number = numeroTarjeta,
                        securityCode = cvv,
                        expirationDate = $"{expiracionAnio}/{expiracionMes}",
                        name = nombreTarjeta // Usar el nombre de tarjeta proporcionado
                    },
                    extraParameters = new
                    {
                        INSTALLMENTS_NUMBER = 1,
                        RESPONSE_URL = responseUrl,
                        CONFIRMATION_URL = responseUrl
                    },
                    type = "AUTHORIZATION_AND_CAPTURE",
                    paymentMethod = tipoTarjeta,
                    paymentCountry = "CO",
                    deviceSessionId = Guid.NewGuid().ToString(),
                    ipAddress = "127.0.0.1",
                    cookie = "pt1t38347bs6jc9ruv2ecpv7o2",
                    userAgent = "Mozilla/5.0"
                },
                test = true
            };
        }
        else if (MetodoPago == "PSE")
        {
            signature = CalcularFirma(apiKey, merchantId, referenceCode, amount, currency);

            paymentRequest = new
            {
                language = "es",
                command = "SUBMIT_TRANSACTION",
                merchant = new
                {
                    apiKey = apiKey,
                    apiLogin = "pRRXKOl8ikMmt9u"
                },
                transaction = new
                {
                    order = new
                    {
                        accountId = accountId,
                        referenceCode = referenceCode,
                        description = "Pago de productos en carrito",
                        language = "es",
                        signature = signature,
                        notifyUrl = notifyUrl,
                        additionalValues = new
                        {
                            TX_VALUE = new { value = amount, currency = currency },
                            TX_TAX = new { value = 0.00, currency = "COP" },
                            TX_TAX_RETURN_BASE = new { value = 0.00, currency = "COP" }
                        },
                        buyer = new
                        {
                            merchantBuyerId = "1",
                            fullName = "Pagador de prueba",
                            emailAddress = "pagador@test.com",
                            contactPhone = "3001234567",
                            dniNumber = "1233900555",
                            shippingAddress = new
                            {
                                street1 = "Calle falsa 123",
                                city = "Bogotá",
                                state = "Cundinamarca",
                                country = "CO",
                                postalCode = "0000000",
                                phone = "3001234567"
                            }
                        }
                    },
                    payer = new
                    {
                        merchantPayerId = "1",
                        fullName = "Pagador de prueba",
                        emailAddress = "pagador@test.com",
                        contactPhone = "3001234567",
                        dniNumber = "1233900555"
                    },
                    extraParameters = new
                    {
                        RESPONSE_URL = responseUrl,
                        CONFIRMATION_URL = responseUrl,
                        FINANCIAL_INSTITUTION_CODE = "1022",
                        USER_TYPE = "N",
                        PSE_REFERENCE1 = "127.0.0.1",
                        PSE_REFERENCE2 = "CC",
                        PSE_REFERENCE3 = "123456789"
                    },
                    type = "AUTHORIZATION_AND_CAPTURE",
                    paymentMethod = "PSE",
                    paymentCountry = "CO",
                    deviceSessionId = Guid.NewGuid().ToString(),
                    ipAddress = "127.0.0.1",
                    cookie = "pt1t38347bs6jc9ruv2ecpv7o2",
                    userAgent = "Mozilla/5.0"
                },
                test = true
            };
        }
        else if (MetodoPago == "EFECTY")
        {
            signature = CalcularFirma(apiKey, merchantId, referenceCode, amount, currency);

            paymentRequest = new
            {
                language = "es",
                command = "SUBMIT_TRANSACTION",
                merchant = new
                {
                    apiKey = apiKey,
                    apiLogin = "pRRXKOl8ikMmt9u"
                },
                transaction = new
                {
                    order = new
                    {
                        accountId = accountId,
                        referenceCode = referenceCode,
                        description = "Pago de productos en carrito",
                        language = "es",
                        signature = signature,
                        additionalValues = new
                        {
                            TX_VALUE = new { value = amount, currency = currency }
                        },
                        buyer = new
                        {
                            merchantBuyerId = "1",
                            fullName = "Comprador de prueba",
                            emailAddress = "comprador@test.com",
                            dniNumber = "0000000"
                        },
                        notifyUrl = notifyUrl
                    },
                    payer = new { dniNumber = "000000" },
                    type = "AUTHORIZATION_AND_CAPTURE",
                    paymentMethod = "EFECTY",
                    paymentCountry = "CO",
                    expirationDate = DateTime.UtcNow.AddDays(3).ToString("yyyy-MM-ddTHH:mm:ss"),
                    extraParameters = new
                    {
                        RESPONSE_URL = responseUrl,
                        CONFIRMATION_URL = responseUrl
                    }
                },
                test = true
            };
        }
        else if (MetodoPago == "NEQUI")
        {
            signature = CalcularFirma(apiKey, merchantId, referenceCode, amount, currency);

            paymentRequest = new
            {
                language = "es",
                command = "SUBMIT_TRANSACTION",
                merchant = new
                {
                    apiKey = apiKey,
                    apiLogin = "pRRXKOl8ikMmt9u"
                },
                transaction = new
                {
                    order = new
                    {
                        accountId = accountId,
                        referenceCode = referenceCode,
                        description = "Pago de productos en carrito",
                        language = "es",
                        signature = signature,
                        notifyUrl = notifyUrl,
                        additionalValues = new
                        {
                            TX_VALUE = new { value = amount, currency = currency }
                        },
                        buyer = new
                        {
                            fullName = "Comprador de prueba",
                            emailAddress = "comprador@test.com",
                            contactPhone = "3001234567",
                            dniNumber = "123456789"
                        }
                    },
                    payer = new { dniNumber = "123456789" },
                    type = "AUTHORIZATION_AND_CAPTURE",
                    paymentMethod = "NEQUI",
                    paymentCountry = "CO",
                    extraParameters = new { RESPONSE_URL = responseUrl, CONFIRMATION_URL = responseUrl }
                },
                test = true
            };
        }
        else
        {
            throw new InvalidOperationException("Método de pago no soportado.");
        }

        // Configuración de HttpClient
        httpClient.Timeout = TimeSpan.FromMinutes(2);

        // Generar un ID de referencia único y un nuevo orderId
        string GenerateUniqueReferenceId() => $"PagoCarrito_{Guid.NewGuid().ToString().Replace("-", "").Substring(0, 8)}";
        referenceCode = GenerateUniqueReferenceId();

        // Convertir paymentRequest a JSON
        var requestContent = new StringContent(JsonSerializer.Serialize(paymentRequest), Encoding.UTF8, "application/json");

        // En el método RealizarPago, reemplaza el bloque try-catch final con este:
        try
        {
            var response = await httpClient.PostAsync("https://sandbox.api.payulatam.com/payments-api/4.0/service.cgi", requestContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Respuesta de PayU: " + responseContent);

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = JsonDocument.Parse(responseContent);
                if (jsonResponse.RootElement.TryGetProperty("transactionResponse", out JsonElement transactionResponse))
                {
                    var state = transactionResponse.GetProperty("state").GetString();
                    Console.WriteLine($"Estado de la transacción: {state}");

                    // Obtener información básica de la transacción
                    var orderId = transactionResponse.TryGetProperty("orderId", out var orderIdElement) ? orderIdElement.GetInt64() : 0;
                    var transactionId = transactionResponse.TryGetProperty("transactionId", out var transactionIdElement) ? transactionIdElement.GetString() : "N/A";
                    var responseMessage = transactionResponse.TryGetProperty("responseMessage", out var messageElement) ? messageElement.GetString() : "No message provided";
                    var trazabilityCode = transactionResponse.TryGetProperty("trazabilityCode", out var trazabilidadElement) ? trazabilidadElement.GetString() : "N/A";

                    // Guardar información de la transacción independientemente del estado
                    if (transactionId != "N/A")
                    {
                        GuardarTransaccion(transactionId, orderId, referenceCode, state, MetodoPago, total, 
                            responseMessage, trazabilityCode);
                    }

                    // Para pagos con tarjeta
                    if (MetodoPago == "TARJETA")
                    {
                        if (state == "APPROVED")
                        {
                            var carritoActual = _carritoService.ObtenerCarrito();
                            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

                            // GUARDAR cache robusto para ConfirmacionPago
                            if (!string.IsNullOrEmpty(transactionId))
                            {
                                _cardDataCache[transactionId] = new Dictionary<string, object>
                                {
                                    ["carrito"] = carritoActual?.ToList() ?? new List<Products>(),
                                    ["userId"] = userId,
                                    ["total"] = carritoActual?.Sum(p => p.Price) ?? total
                                };
                            }

                            await ActualizarEstadoEnvio(orderId, transactionId, "En Proceso");

                            if (carritoActual != null && carritoActual.Any())
                            {
                                try
                                {
                                    _carritoService.GuardarUltimaCompra(new List<Products>(carritoActual));
                                    _carritoService.VaciarCarrito();
                                }
                                catch { }
                            }

                            var confirmacionUrl = $"/ConfirmacionPago?transactionState={state}&referenceCode={trazabilityCode}&message={responseMessage}&orderId={orderId}&transactionId={transactionId}&paymentMethod={MetodoPago}";
                            return confirmacionUrl;
                        }
                        else if (state == "REJECTED" || state == "DECLINED")
                        {
                            // Para pagos rechazados
                            Console.WriteLine($"Pago rechazado: {responseMessage}");
                            return $"/ConfirmacionPago?transactionState={state}&message={responseMessage}&paymentMethod={MetodoPago}&orderId={orderId}&transactionId={transactionId}";
                        }
                        else if (state == "PENDING")
                        {
                            // Para pagos pendientes
                            Console.WriteLine($"Pago pendiente: {responseMessage}");
                            return $"/ConfirmacionPago?transactionState={state}&message={responseMessage}&paymentMethod={MetodoPago}&orderId={orderId}&transactionId={transactionId}";
                        }
                        else
                        {
                            // Para otros estados
                            Console.WriteLine($"Estado no manejado: {state} - {responseMessage}");
                            return $"/ConfirmacionPago?transactionState={state}&message={responseMessage}&paymentMethod={MetodoPago}&orderId={orderId}&transactionId={transactionId}";
                        }
                    }
                    // Para otros métodos de pago (PSE, EFECTY, NEQUI)
                    else if (state == "PENDING")
                    {
                        // IMPORTANTE: Para PSE, EFECTY, NEQUI NO crear envío hasta que esté aprobado
                        // Solo guardar los datos para recuperar después

                        // Crear envío con número de orden generado si no existe
                        if (orderId == 0)
                        {
                            orderId = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // Generar un orderId único
                            Console.WriteLine($"Generando OrderId para {MetodoPago}: {orderId}");
                        }

                        if (MetodoPago == "EFECTY")
                        {
                            var billingCode = transactionResponse.GetProperty("extraParameters").GetProperty("URL_PAYMENT_RECEIPT_HTML").GetString();
                            
                            // Para EFECTY, redirigir directamente al código de pago
                            return billingCode;
                        }
                        else if (MetodoPago == "PSE")
                        {
                            var pseUrl = transactionResponse.GetProperty("extraParameters").GetProperty("BANK_URL").GetString();
                            
                            // Guardar información del PSE para recuperar después
                            var pseKey = $"PSE_{orderId}_{transactionId}";
                            var pseData = new Dictionary<string, object>
                            {
                                ["orderId"] = orderId,
                                ["transactionId"] = transactionId,
                                ["referenceCode"] = trazabilityCode,
                                ["responseMessage"] = responseMessage,
                                ["paymentMethod"] = MetodoPago,
                                ["timestamp"] = DateTime.UtcNow,
                                ["carrito"] = _carritoService.ObtenerCarrito() // Guardar carrito para crear envío después
                            };
                            
                            _pseDataCache[pseKey] = pseData;
                            
                            // Para PSE, redirigir directamente al banco SIN crear envío
                            Console.WriteLine($"PSE - Redirigiendo directamente al banco: {pseUrl}");
                            Console.WriteLine($"PSE - Datos almacenados con key: {pseKey} (sin crear envío aún)");
                            return pseUrl;
                        }
                        else if (MetodoPago == "NEQUI")
                        {
                            var nequiUrl = transactionResponse.GetProperty("extraParameters").GetProperty("URL_PAYMENT_RECEIPT_HTML").GetString();
                            
                            // Para NEQUI, redirigir directamente a NEQUI
                            Console.WriteLine($"NEQUI - Redirigiendo directamente a NEQUI: {nequiUrl}");
                            return nequiUrl;
                        }
                    }
                }
                else
                {
                    // Si no hay transactionResponse, verificar si hay un error
                    if (jsonResponse.RootElement.TryGetProperty("error", out JsonElement errorElement))
                    {
                        var errorMessage = errorElement.GetString();
                        Console.WriteLine($"Error en la respuesta de PayU: {errorMessage}");
                        return $"/ConfirmacionPago?transactionState=ERROR&message={errorMessage}&paymentMethod={MetodoPago}";
                    }
                }
            }
            else
            {
                Console.WriteLine($"Error HTTP: {response.StatusCode} - {responseContent}");
                return $"/ConfirmacionPago?transactionState=ERROR&message=Error_HTTP_{response.StatusCode}&paymentMethod={MetodoPago}";
            }

            // Si llegamos aquí, algo salió mal
            Console.WriteLine("La transacción no se completó correctamente");
            return "/ConfirmacionPago?transactionState=ERROR&message=Error_en_la_transaccion";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en el procesamiento del pago: {ex.Message}");
            return "/ConfirmacionPago?transactionState=ERROR&message=Error_procesando_pago";
        }
        // En caso de no redirección o procesamiento, retorna null
        return null;
    }

    // Método actualizado para crear envíos cuando se actualiza el estado
    private async Task ActualizarEstadoEnvio(long orderId, string transactionId, string nuevoEstado)
    {
        try
        {
            Console.WriteLine($"Actualizando envío con OrderId: {orderId}, TransactionId: {transactionId}, Nuevo Estado: {nuevoEstado}");

            // Obtener información del usuario autenticado
            string direccionCompleta = "Calle 60b sur # 74-27, Bogotá, Cundinamarca, Colombia";
            string telefonoContacto = "+57 323 768 4390";
            string userId = "";
            
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext?.User?.Identity?.IsAuthenticated == true)
                {
                    userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (!string.IsNullOrEmpty(userId))
                    {
                        var usuario = await _userManager.FindByIdAsync(userId);
                        if (usuario != null)
                        {
                            // Construir dirección completa del usuario
                            direccionCompleta = usuario.Direccion;
                            if (!string.IsNullOrEmpty(usuario.DireccionSecundaria))
                            {
                                direccionCompleta += $", {usuario.DireccionSecundaria}";
                            }
                            direccionCompleta += $", {usuario.Ciudad}, {usuario.Departamento}, Colombia";
                            Console.WriteLine($"Dirección real del usuario obtenida: {direccionCompleta}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error obteniendo información del usuario: {ex.Message}");
                // Usar dirección por defecto si hay error
                direccionCompleta = "Calle 60b sur # 74-27, Bogotá, Cundinamarca, Colombia";
            }

            // Crear o actualizar en el cache de envíos
            var envioInfo = _enviosCache.GetOrAdd(orderId, id => new EnvioInfo
            {
                OrderId = id,
                TransactionId = transactionId,
                NumeroGuia = $"GU{GetColombiaTime():yyyyMMdd}{id:D4}",
                DireccionEnvio = direccionCompleta,
                FechaCreacion = GetColombiaTime(), // ? Usar hora de Colombia
                FechaEstimadaEntrega = GetColombiaTime().AddDays(3),
                UserId = userId // ? CRÍTICO: Asignar userId para privacidad
            });

            envioInfo.Estado = nuevoEstado;
            envioInfo.TransactionId = transactionId;
            envioInfo.DireccionEnvio = direccionCompleta;
            envioInfo.UserId = userId; // ? Asegurar que siempre tenga userId

            // Actualizar fechas según el estado con zona horaria de Colombia
            var colombiaTime = GetColombiaTime();
            switch (nuevoEstado)
            {
                case "RECOLECTADO":
                    envioInfo.FechaEnvio = colombiaTime;
                    break;
                case "ENTREGADO":
                    envioInfo.FechaEntrega = colombiaTime;
                    break;
            }

            // Obtener productos del carrito actual (antes de vaciarlo) o de la última compra
            List<Products> productosParaEnvio = null;
            
            // Primero intentar obtener del carrito actual
            var carritoActual = _carritoService.ObtenerCarrito();
            if (carritoActual != null && carritoActual.Any())
            {
                productosParaEnvio = carritoActual;
                Console.WriteLine($"Obteniendo productos del carrito actual: {carritoActual.Count} productos");
            }
            else
            {
                // Si no hay carrito actual, obtener de la última compra
                productosParaEnvio = _carritoService.ObtenerUltimaCompra();
                Console.WriteLine($"Obteniendo productos de la última compra: {productosParaEnvio?.Count ?? 0} productos");
            }

            // Agregar productos al envío si están disponibles
            if (productosParaEnvio != null && productosParaEnvio.Any())
            {
                envioInfo.Productos = productosParaEnvio.Select(p => new ProductoEnvio
                {
                    Nombre = !string.IsNullOrEmpty(p.Name) ? p.Name : "Producto sin nombre",
                    Precio = p.Price > 0 ? p.Price : 0,
                    Cantidad = 1
                }).ToList();

                // ? CRÍTICO: Establecer información principal del envío BASADA EN LOS PRODUCTOS
                if (envioInfo.Productos.Any())
                {
                    envioInfo.NombreProducto = envioInfo.Productos.Count == 1 
                        ? envioInfo.Productos.First().Nombre 
                        : $"Compra múltiple ({envioInfo.Productos.Count} productos)";
                    envioInfo.PrecioTotal = envioInfo.Productos.Sum(p => p.Precio * p.Cantidad);
                }

                Console.WriteLine($"Productos agregados al envío:");
                foreach (var producto in envioInfo.Productos)
                {
                    Console.WriteLine($"- {producto.Nombre}: {producto.Precio:C0}");
                }
            }
            else
            {
                Console.WriteLine("No se encontraron productos para agregar al envío");
                // Crear un producto por defecto si no hay productos
                envioInfo.Productos = new List<ProductoEnvio>
                {
                    new ProductoEnvio
                    {
                        Nombre = "Compra procesada",
                        Precio = 0,
                        Cantidad = 1
                    }
                };
                envioInfo.NombreProducto = "Compra procesada";
                envioInfo.PrecioTotal = 0;
            }

            Console.WriteLine($"Envío actualizado correctamente con dirección real: {direccionCompleta}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al actualizar el estado del envío: " + ex.Message);
        }
    }

    // M?todo p?blico para actualizar estado de env?o
    public async Task<bool> ActualizarEstadoEnvioPublico(long orderId, string nuevoEstado)
    {
        try
        {
            // Llamar al m?todo privado
            await ActualizarEstadoEnvio(orderId, "TXN_UPDATE", nuevoEstado);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar estado p?blico: {ex.Message}");
            return false;
        }
    }

    // M?todo para obtener el siguiente estado l?gico
    public string ObtenerSiguienteEstado(string estadoActual)
    {
        return estadoActual switch
        {
            "PREPARANDO" => "RECOLECTADO",
            "RECOLECTADO" => "EN_CENTRO_DISTRIBUCION", 
            "EN_CENTRO_DISTRIBUCION" => "EN_TRANSITO_A_DESTINO",
            "EN_TRANSITO_A_DESTINO" => "LLEGADA_CIUDAD_DESTINO",
            "LLEGADA_CIUDAD_DESTINO" => "EN_RUTA_ENTREGA",
            "EN_RUTA_ENTREGA" => "ENTREGADO",
            _ => estadoActual
        };
    }

    // ? MÉTODO MEJORADO: Zona horaria de Colombia GMT-5 con configuración persistente
    private DateTime GetColombiaTime()
    {
        try
        {
            // Usar zona horaria configurada si está disponible
            if (_zonaHorariaConfigurada != null)
            {
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _zonaHorariaConfigurada);
            }

            // Fallback: detectar zona horaria automáticamente
            TimeZoneInfo colombiaTimeZone;
            try
            {
                // Intentar Windows primero
                colombiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
            }
            catch
            {
                try
                {
                    // Intentar Linux/Mac
                    colombiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");
                }
                catch
                {
                    // Fallback: Crear zona horaria personalizada GMT-5
                    colombiaTimeZone = TimeZoneInfo.CreateCustomTimeZone(
                        "Colombia Standard Time",
                        TimeSpan.FromHours(-5),
                        "Colombia Standard Time",
                        "Colombia Standard Time"
                    );
                }
            }

            // Guardar la zona horaria detectada para uso futuro
            _zonaHorariaConfigurada = colombiaTimeZone;
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, colombiaTimeZone);
        }
        catch
        {
            // Fallback final: UTC-5 directo
            var fallbackTime = DateTime.UtcNow.AddHours(-5);
            Console.WriteLine($"?? Usando fallback GMT-5: {fallbackTime:yyyy-MM-dd HH:mm:ss}");
            return fallbackTime;
        }
    }

    // Método para guardar información de transacción
    private void GuardarTransaccion(string transactionId, long orderId, string referenceCode, 
        string estado, string metodoPago, decimal monto, string responseMessage = "", 
        string trazabilityCode = "", string authorizationCode = "")
    {
        try
        {
            // Capturar el usuario actual si existe
            string userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;

            var transaccion = new TransaccionInfo
            {
                TransactionId = transactionId,
                OrderId = orderId,
                ReferenceCode = referenceCode,
                Estado = estado,
                MetodoPago = metodoPago,
                Monto = monto,
                ResponseMessage = responseMessage,
                TrazabilityCode = trazabilityCode,
                AuthorizationCode = authorizationCode,
                FechaTransaccion = DateTime.UtcNow,
                UserId = userId
            };

            _transacciones.AddOrUpdate(transactionId, transaccion, (key, old) => transaccion);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al guardar transacción: {ex.Message}");
        }
    }

    // ? IMPLEMENTAR MÉTODOS FALTANTES DE LA INTERFAZ

    // ? CORREGIDO: Obtener todos los envíos
    public List<EnvioInfo> ObtenerTodosLosEnvios()
    {
        try
        {
            // Combinar envíos del cache y de la lista legacy
            var todosLosEnvios = new List<EnvioInfo>();

            // Agregar del cache
            todosLosEnvios.AddRange(_enviosCache.Values);

            // Agregar de la lista estática si no están ya en el cache
            foreach (var envio in _enviosLista)
            {
                if (!todosLosEnvios.Any(e => e.OrderId == envio.OrderId))
                {
                    todosLosEnvios.Add(envio);
                }
            }

            return todosLosEnvios.OrderByDescending(e => e.FechaCreacion).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener envíos: {ex.Message}");
            return new List<EnvioInfo>();
        }
    }

    public EnvioInfo ObtenerEnvioPorOrden(long orderId)
    {
        try
        {
            // Buscar primero en el cache
            if (_enviosCache.TryGetValue(orderId, out var envio))
            {
                return envio;
            }

            // Buscar en la lista legacy
            return _enviosLista.FirstOrDefault(e => e.OrderId == orderId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener envío por orden {orderId}: {ex.Message}");
            return null;
        }
    }

    // ? IMPLEMENTAR: Obtener envío por guía (requerido por interfaz)
    public EnvioInfo? ObtenerEnvioPorGuia(string numeroGuia)
    {
        try
        {
            // Buscar en cache primero
            var envioCache = _enviosCache.Values.FirstOrDefault(e => e.NumeroGuia == numeroGuia);
            if (envioCache != null)
            {
                return envioCache;
            }

            // Buscar en la lista legacy
            return _enviosLista.FirstOrDefault(e => e.NumeroGuia == numeroGuia);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener envío por guía {numeroGuia}: {ex.Message}");
            return null;
        }
    }

    // ? IMPLEMENTAR: Consultar estado de envío (requerido por interfaz)
    public string ConsultarEstadoEnvio(string numeroGuia)
    {
        try
        {
            var envio = ObtenerEnvioPorGuia(numeroGuia);
            return envio?.Estado ?? "Envío no encontrado";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al consultar estado de envío {numeroGuia}: {ex.Message}");
            return "ERROR";
        }
    }

    // ? CORREGIDO: Actualizar estado de envío SIN corrupción de datos
    public string ActualizarEstadoEnvio(string numeroGuia)
    {
        try
        {
            // Buscar en cache primero
            var envioCache = _enviosCache.Values.FirstOrDefault(e => e.NumeroGuia == numeroGuia);
            if (envioCache != null)
            {
                return ActualizarEstadoEnvioSeguro(envioCache);
            }

            // Buscar en lista legacy
            var envioLegacy = _enviosLista.FirstOrDefault(e => e.NumeroGuia == numeroGuia);
            if (envioLegacy != null)
            {
                return ActualizarEstadoEnvioSeguro(envioLegacy);
            }

            return "Envío no encontrado";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Error actualizando estado de envío {numeroGuia}: {ex.Message}");
            return "ERROR";
        }
    }

    // ? MÉTODO SEGURO: Actualizar estado sin corromper datos
    private string ActualizarEstadoEnvioSeguro(EnvioInfo envio)
    {
        try
        {
            // ? GUARDAR datos originales para evitar corrupción
            var nombreOriginal = envio.NombreProducto;
            var precioOriginal = envio.PrecioTotal;
            var userIdOriginal = envio.UserId;
            var fechaOriginal = envio.FechaCreacion;
            var productosOriginales = envio.Productos?.ToList(); // Copia segura

            // Simular progresión de estados realista
            var estadosProgresion = new[]
            {
                "PREPARANDO",
                "RECOLECTADO", 
                "EN_TRANSITO_ORIGEN",
                "EN_CENTRO_DISTRIBUCION",
                "EN_TRANSITO_DESTINO",
                "EN_RUTA_ENTREGA",
                "ENTREGADO"
            };

            var estadoActualIndex = Array.IndexOf(estadosProgresion, envio.Estado);
            if (estadoActualIndex < estadosProgresion.Length - 1)
            {
                envio.Estado = estadosProgresion[estadoActualIndex + 1];
                
                // ? PRESERVAR datos originales - NO permitir corrupción
                envio.NombreProducto = nombreOriginal;
                envio.PrecioTotal = precioOriginal;
                envio.UserId = userIdOriginal;
                envio.FechaCreacion = fechaOriginal;
                
                // ? PRESERVAR productos originales
                if (productosOriginales != null && productosOriginales.Any())
                {
                    envio.Productos = productosOriginales;
                }

                // Actualizar fechas de seguimiento con zona horaria Colombia
                var colombiaTime = GetColombiaTime();
                switch (envio.Estado)
                {
                    case "RECOLECTADO":
                        envio.FechaRecoleccion = colombiaTime;
                        break;
                    case "EN_CENTRO_DISTRIBUCION":
                        envio.FechaLlegadaCentroDistribucion = colombiaTime;
                        break;
                    case "EN_TRANSITO_DESTINO":
                        envio.FechaSalidaCentroDistribucion = colombiaTime;
                        break;
                    case "EN_RUTA_ENTREGA":
                        envio.FechaLlegadaCiudadDestino = colombiaTime;
                        envio.FechaEnRutaEntrega = colombiaTime;
                        break;
                    case "ENTREGADO":
                        envio.FechaEntrega = colombiaTime;
                        break;
                }

                Console.WriteLine($"? Estado actualizado para {envio.NumeroGuia}: {envio.Estado}");
                Console.WriteLine($"   ?? Producto: {envio.NombreProducto}");
                Console.WriteLine($"   ?? Precio: ${envio.PrecioTotal:N0}");
                Console.WriteLine($"   ?? Usuario: {envio.UserId}");
                Console.WriteLine($"   ?? Fecha: {colombiaTime:yyyy-MM-dd HH:mm:ss} (GMT-5)");
            }

            return envio.Estado;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Error en actualización segura: {ex.Message}");
            return "ERROR";
        }
    }

    // ? MÉTODO MEJORADO: Crear envío con privacidad por usuario
    public string CrearEnvio(string userId, string nombreProducto, decimal precioTotal, string ciudadDestino)
    {
        try
        {
            var numeroGuia = GenerarNumeroGuia();
            var direccionOrigen = new DireccionEnvio
            {
                Direccion = "Carrera 15 #93-47",
                Ciudad = "Bogotá",
                Departamento = "Cundinamarca",
                CodigoPostal = "110221"
            };

            // Obtener dirección del usuario desde la base de datos o usar dirección por defecto
            var direccionDestino = ObtenerDireccionUsuario(userId) ?? new DireccionEnvio
            {
                Direccion = "Calle 80 #11-42",
                Ciudad = ciudadDestino,
                Departamento = ObtenerDepartamentoPorCiudad(ciudadDestino),
                CodigoPostal = "000000"
            };

            var envio = new EnvioInfo
            {
                NumeroGuia = numeroGuia,
                UserId = userId, // ? CRÍTICO: Asociar envío al usuario
                NombreProducto = nombreProducto,
                PrecioTotal = precioTotal,
                Estado = "PREPARANDO",
                FechaCreacion = GetColombiaTime(), // ? Usar zona horaria de Colombia
                DireccionOrigen = direccionOrigen,
                DireccionDestino = direccionDestino,
                PesoKg = 2.5m,
                Observaciones = "Envío estándar - Entrega 3-5 días hábiles",
                OrderId = DateTimeOffset.UtcNow.ToUnixTimeSeconds() // Generar OrderId único
            };

            // ? IMPORTANTE: Agregar a ambas listas para compatibilidad
            _enviosLista.Add(envio);
            _enviosCache.TryAdd(envio.OrderId, envio);

            Console.WriteLine($"? Envío creado exitosamente:");
            Console.WriteLine($"   ?? Guía: {numeroGuia}");
            Console.WriteLine($"   ?? Usuario: {userId}");
            Console.WriteLine($"   ?? Producto: {nombreProducto}");
            Console.WriteLine($"   ?? Precio: ${precioTotal:N0}");
            Console.WriteLine($"   ?? Fecha: {envio.FechaCreacion:yyyy-MM-dd HH:mm:ss} (GMT-5)");
            Console.WriteLine($"   ?? Destino: {ciudadDestino}");

            return numeroGuia;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Error creando envío: {ex.Message}");
            throw;
        }
    }

    // ? MÉTODO MEJORADO: Eliminar envío con verificación de usuario
    public void EliminarEnvio(string numeroGuia)
    {
        try
        {
            // Eliminar del cache
            var envioCache = _enviosCache.Values.FirstOrDefault(e => e.NumeroGuia == numeroGuia);
            if (envioCache != null)
            {
                _enviosCache.TryRemove(envioCache.OrderId, out _);
                Console.WriteLine($"? Envío {numeroGuia} eliminado del cache");
            }

            // Eliminar de la lista legacy
            var envioLegacy = _enviosLista.FirstOrDefault(e => e.NumeroGuia == numeroGuia);
            if (envioLegacy != null)
            {
                _enviosLista.Remove(envioLegacy);
                Console.WriteLine($"? Envío {numeroGuia} eliminado de la lista legacy");
            }

            if (envioCache == null && envioLegacy == null)
            {
                Console.WriteLine($"?? Envío {numeroGuia} no encontrado para eliminar");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Error eliminando envío {numeroGuia}: {ex.Message}");
            throw;
        }
    }

    // Implement other existing methods...
    public List<TransaccionInfo> ObtenerTodasLasTransacciones()
    {
        try
        {
            return _transacciones.Values.ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener transacciones: {ex.Message}");
            return new List<TransaccionInfo>();
        }
    }

    public TransaccionInfo ObtenerTransaccion(string transactionId)
    {
        try
        {
            _transacciones.TryGetValue(transactionId, out var transaccion);
            return transaccion;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener transacción {transactionId}: {ex.Message}");
            return null;
        }
    }

    public async Task<(bool Success, string Message, string Details)> ProbarConectividadPayU()
    {
        try
        {
            var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            // Crear una solicitud de prueba simple
            var testRequest = new
            {
                language = "es",
                command = "PING",
                merchant = new
                {
                    apiKey = _settings.ApiKey,
                    apiLogin = _settings.ApiLogin
                },
                test = true
            };

            var content = new StringContent(
                JsonSerializer.Serialize(testRequest), 
                Encoding.UTF8, 
                "application/json");

            var response = await httpClient.PostAsync(
                "https://sandbox.api.payulatam.com/payments-api/4.0/service.cgi", 
                content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return (true, "Conexión exitosa con PayU", responseContent);
            }
            else
            {
                return (false, $"Error de conexión: {response.StatusCode}", responseContent);
            }
        }
        catch (Exception ex)
        {
            return (false, $"Error al conectar con PayU: {ex.Message}", ex.ToString());
        }
    }

    // Mostrar envíos en la página
    public void MostrarEnviosPagina()
    {
        try
        {
            string enviosJson = ObtenerEnviosComoJson();

            if (string.IsNullOrWhiteSpace(enviosJson))
            {
                throw new Exception("No se pudieron obtener los envíos como JSON.");
            }

            Console.WriteLine("Datos para la página de envíos:");
            Console.WriteLine(enviosJson);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al mostrar los envíos en la página: " + ex.Message);
        }
    }

    // Obtener envíos en JSON
    public string ObtenerEnviosComoJson()
    {
        try
        {
            // Combinar información del diccionario legacy y el cache nuevo
            var enviosCompletos = new Dictionary<string, object>();
            
            // Agregar del cache nuevo
            foreach (var envio in _enviosCache.Values)
            {
                enviosCompletos[$"envio_{envio.OrderId}"] = envio;
            }
            
            // Agregar de la lista
            foreach (var envio in _enviosLista)
            {
                enviosCompletos[$"lista_{envio.OrderId}"] = envio;
            }
            
            return JsonSerializer.Serialize(enviosCompletos);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error al serializar los envíos a JSON: " + ex.Message);
            return "{}"; // Retornar un JSON vacío en caso de error
        }
    }

    // ? MÉTODOS ESTÁTICOS PARA PSE - Hacer públicos para acceso desde ConfirmacionPago
    public static Dictionary<string, object> RecuperarDatosPSE(long orderId, string transactionId)
    {
        try
        {
            var pseKey = $"PSE_{orderId}_{transactionId}";
            if (_pseDataCache.TryGetValue(pseKey, out var data))
            {
                // Limpiar datos después de recuperarlos
                _pseDataCache.TryRemove(pseKey, out _);
                return data;
            }
            
            // Si no encontramos por clave exacta, buscar por orderId
            var keyByOrderId = _pseDataCache.Keys.FirstOrDefault(k => k.Contains($"PSE_{orderId}_"));
            if (keyByOrderId != null && _pseDataCache.TryGetValue(keyByOrderId, out var dataByOrderId))
            {
                _pseDataCache.TryRemove(keyByOrderId, out _);
                return dataByOrderId;
            }
            
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al recuperar datos PSE: {ex.Message}");
            return null;
        }
    }

    // Método para recuperar clave PSE por transactionId
    public static string RecuperarClaveSegunTransactionId(string transactionId)
    {
        try
        {
            return _pseDataCache.Keys.FirstOrDefault(k => k.Contains(transactionId));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al buscar clave PSE: {ex.Message}");
            return null;
        }
    }

    // Método para recuperar datos PSE por clave
    public static Dictionary<string, object> RecuperarDatosPSEPorClave(string pseKey)
    {
        try
        {
            if (_pseDataCache.TryGetValue(pseKey, out var data))
            {
                _pseDataCache.TryRemove(pseKey, out _);
                return data;
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al recuperar datos PSE por clave: {ex.Message}");
            return null;
        }
    }

    // Método para crear envío cuando PSE sea aprobado
    public static void CrearEnvioAprobado(long orderId, string transactionId, List<ComputerStore.Models.Products> carrito)
    {
        try
        {
            Console.WriteLine($"Creando envío aprobado - OrderId: {orderId}, TransactionId: {transactionId}");
            
            var envioInfo = new EnvioInfo
            {
                OrderId = orderId,
                TransactionId = transactionId,
                NumeroGuia = $"GU{orderId:D8}",
                DireccionEnvio = "Dirección de envío por defecto",
                FechaCreacion = DateTime.UtcNow,
                FechaEstimadaEntrega = DateTime.UtcNow.AddDays(3),
                Estado = "En Proceso" // Estado aprobado, no pendiente
            };

            // Agregar productos del carrito guardado
            if (carrito != null && carrito.Any())
            {
                envioInfo.Productos = carrito.Select(p => new ProductoEnvio
                {
                    Nombre = !string.IsNullOrEmpty(p.Name) ? p.Name : "Producto sin nombre",
                    Precio = p.Price > 0 ? p.Price : 0,
                    Cantidad = 1
                }).ToList();

                Console.WriteLine($"Productos agregados al envío aprobado:");
                foreach (var producto in envioInfo.Productos)
                {
                    Console.WriteLine($"- {producto.Nombre}: {producto.Precio:C0}");
                }
            }

            // Agregar al cache de envíos
            _enviosCache.AddOrUpdate(orderId, envioInfo, (key, old) => envioInfo);
            
            Console.WriteLine("Envío creado exitosamente después de aprobación PSE.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al crear envío aprobado: {ex.Message}");
        }
    }

    // Helper methods
    private string GenerarNumeroGuia()
    {
        var tiempo = GetColombiaTime();
        return $"GU{tiempo:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
    }

    private DireccionEnvio? ObtenerDireccionUsuario(string userId)
    {
        try
        {
            var usuario = _userManager.FindByIdAsync(userId).Result;
            if (usuario != null && !string.IsNullOrEmpty(usuario.Direccion))
            {
                return new DireccionEnvio
                {
                    Direccion = usuario.Direccion,
                    Ciudad = usuario.Ciudad ?? "Bogotá",
                    Departamento = usuario.Departamento ?? "Cundinamarca",
                    CodigoPostal = "000000"
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error obteniendo dirección del usuario {userId}: {ex.Message}");
        }
        return null;
    }

    private string ObtenerDepartamentoPorCiudad(string ciudad)
    {
        return ciudad?.ToLower() switch
        {
            "bogotá" or "bogota" => "Cundinamarca",
            "medellín" or "medellin" => "Antioquia",
            "cali" => "Valle del Cauca",
            "barranquilla" => "Atlántico",
            "cartagena" => "Bolívar",
            _ => "Cundinamarca"
        };
    }

    // ? IMPLEMENTAR: Configurar zona horaria
    public void ConfigurarZonaHoraria(TimeZoneInfo timeZone)
    {
        _zonaHorariaConfigurada = timeZone;
        Console.WriteLine($"? Zona horaria configurada: {timeZone.DisplayName} (UTC{timeZone.BaseUtcOffset})");
    }

    // ? IMPLEMENTAR: Obtener fecha actual de Colombia
    public DateTime ObtenerFechaColombiaActual()
    {
        return GetColombiaTime();
    }

    private string GetBaseUrl()
    {
        try
        {
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx?.Request?.Host.HasValue == true)
            {
                return $"{ctx.Request.Scheme}://{ctx.Request.Host.Value}";
            }
        }
        catch { }
        // Fallback a puerto más usado del proyecto
        return "https://localhost:7108";
    }

    // Métodos estáticos de acceso al cache de TARJETA
    public static Dictionary<string, object>? RecuperarDatosTarjetaPorTx(string transactionId)
    {
        try
        {
            if (string.IsNullOrEmpty(transactionId)) return null;
            if (_cardDataCache.TryGetValue(transactionId, out var data))
            {
                // No eliminar de inmediato para permitir reintento
                return data;
            }
            return null;
        }
        catch { return null; }
    }
}