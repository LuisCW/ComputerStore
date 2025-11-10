using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ComputerStore.Models;
using ComputerStore.Services;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace ComputerStore.Pages
{
    public class CheckoutModel : PageModel
    {
        private readonly LittleCarService _carritoService;
        private readonly IPayUService _payUService;
        private readonly ILogger<CheckoutModel> _logger;

        public List<Products> ProductosCarrito { get; set; } = new List<Products>();
        public decimal Total { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Debe seleccionar un método de pago")]
        public string MetodoPago { get; set; }

        // Propiedades para Tarjeta de Crédito (sin [Required])
        [BindProperty]
        public string NumeroTarjeta { get; set; }
        
        [BindProperty]
        public string CVV { get; set; }
        
        [BindProperty]
        public string ExpiracionMes { get; set; }
        
        [BindProperty]
        public string ExpiracionAnio { get; set; }
        
        [BindProperty]
        public string TipoTarjeta { get; set; }
        
        [BindProperty]
        public string NombreTarjeta { get; set; }

        // Propiedades para PSE (sin [Required])
        [BindProperty]
        public string DocumentoPSE { get; set; }
        
        [BindProperty]
        public string BancoPSE { get; set; }

        // Propiedades para Efecty (sin [Required])
        [BindProperty]
        public string DocumentoEfecty { get; set; }

        // Propiedades para Nequi (sin [Required])
        [BindProperty]
        public string CelularNequi { get; set; }

        public CheckoutModel(LittleCarService carritoService, IPayUService payUService, ILogger<CheckoutModel> logger)
        {
            _carritoService = carritoService;
            _payUService = payUService;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            ProductosCarrito = _carritoService.ObtenerCarrito();
            Total = _carritoService.ObtenerTotal();

            // Si el carrito está vacío, redirigir al carrito
            if (ProductosCarrito == null || !ProductosCarrito.Any())
            {
                return RedirectToPage("/LittleCar");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostRealizarPago()
        {
            try
            {
                // Validar que hay productos en el carrito
                ProductosCarrito = _carritoService.ObtenerCarrito();
                Total = _carritoService.ObtenerTotal();

                if (ProductosCarrito == null || !ProductosCarrito.Any())
                {
                    ModelState.AddModelError(string.Empty, "El carrito está vacío.");
                    return Page();
                }

                // Validar método de pago
                if (string.IsNullOrEmpty(MetodoPago))
                {
                    ModelState.AddModelError(string.Empty, "Debe seleccionar un método de pago.");
                    return Page();
                }

                // Validar campos específicos según el método de pago
                if (!ValidarCamposMetodoPago())
                {
                    return Page();
                }

                // Crear pedido
                var pedido = new Pedido
                {
                    ReferenceCode = $"ORDER_{Guid.NewGuid().ToString("N")[..8]}",
                    Description = $"Compra de {ProductosCarrito.Count} producto(s)",
                    Amount = Total,
                    BuyerName = "Cliente",
                    BuyerEmail = "cliente@ejemplo.com"
                };

                // LOG: Parámetros enviados a PayU
                _logger.LogInformation("Enviando pago a PayU: Metodo={MetodoPago}, Pedido={@pedido}, Total={Total}", MetodoPago, pedido, Total);

                // Procesar pago
                string respuestaPayU = await ProcesarPago(pedido, Total);

                // LOG: Respuesta cruda de PayU
                _logger.LogInformation("Respuesta cruda de PayU: {Respuesta}", respuestaPayU);
                
                if (string.IsNullOrEmpty(respuestaPayU))
                {
                    ModelState.AddModelError(string.Empty, "Error al procesar el pago. La respuesta de PayU fue vacía.");
                    return Page();
                }

                // Manejar redirecciones directas (URLs)
                if (respuestaPayU.StartsWith("http") || respuestaPayU.StartsWith("/"))
                {
                    return Redirect(respuestaPayU);
                }

                // Procesar respuesta JSON
                return await ProcesarRespuestaPayU(respuestaPayU);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar el pago en checkout");
                ModelState.AddModelError(string.Empty, $"Error inesperado al procesar el pago: {ex.Message}");
                
                // Recargar datos para mostrar la página
                ProductosCarrito = _carritoService.ObtenerCarrito();
                Total = _carritoService.ObtenerTotal();
                return Page();
            }
        }

        private bool ValidarCamposMetodoPago()
        {
            bool esValido = true;

            switch (MetodoPago?.ToUpper())
            {
                case "TARJETA":
                    // Limpiar el número de tarjeta de espacios y guiones
                    if (!string.IsNullOrEmpty(NumeroTarjeta))
                    {
                        NumeroTarjeta = NumeroTarjeta.Replace(" ", "").Replace("-", "");
                    }

                    if (string.IsNullOrEmpty(NumeroTarjeta))
                    {
                        ModelState.AddModelError(nameof(NumeroTarjeta), "El número de tarjeta es requerido para el pago con tarjeta.");
                        esValido = false;
                    }
                    else if (NumeroTarjeta.Length < 13)
                    {
                        ModelState.AddModelError(nameof(NumeroTarjeta), "El número de tarjeta debe tener al menos 13 dígitos.");
                        esValido = false;
                    }
                    
                    if (string.IsNullOrEmpty(CVV))
                    {
                        ModelState.AddModelError(nameof(CVV), "El CVV es requerido para el pago con tarjeta.");
                        esValido = false;
                    }
                    else if (CVV.Length < 3)
                    {
                        ModelState.AddModelError(nameof(CVV), "El CVV debe tener al menos 3 dígitos.");
                        esValido = false;
                    }
                    
                    if (string.IsNullOrEmpty(ExpiracionMes))
                    {
                        ModelState.AddModelError(nameof(ExpiracionMes), "El mes de expiración es requerido para el pago con tarjeta.");
                        esValido = false;
                    }
                    else if (!int.TryParse(ExpiracionMes, out int mes) || mes < 1 || mes > 12)
                    {
                        ModelState.AddModelError(nameof(ExpiracionMes), "El mes debe ser un número entre 1 y 12.");
                        esValido = false;
                    }
                    
                    if (string.IsNullOrEmpty(ExpiracionAnio))
                    {
                        ModelState.AddModelError(nameof(ExpiracionAnio), "El año de expiración es requerido para el pago con tarjeta.");
                        esValido = false;
                    }
                    else if (!int.TryParse(ExpiracionAnio, out int anio) || anio < DateTime.Now.Year)
                    {
                        ModelState.AddModelError(nameof(ExpiracionAnio), "El año de expiración no puede ser anterior al año actual.");
                        esValido = false;
                    }
                    
                    if (string.IsNullOrEmpty(NombreTarjeta))
                    {
                        ModelState.AddModelError(nameof(NombreTarjeta), "El nombre del titular es requerido para el pago con tarjeta.");
                        esValido = false;
                    }
                    else if (NombreTarjeta.Length < 2)
                    {
                        ModelState.AddModelError(nameof(NombreTarjeta), "El nombre del titular debe tener al menos 2 caracteres.");
                        esValido = false;
                    }
                    
                    break;

                case "PSE":
                    if (Total < 1000)
                    {
                        ModelState.AddModelError(string.Empty, "El monto mínimo para PSE es $1,000 COP.");
                        esValido = false;
                    }
                    else if (Total > 30000000)
                    {
                        ModelState.AddModelError(string.Empty, "El monto máximo para PSE es $30,000,000 COP.");
                        esValido = false;
                    }
                    
                    if (string.IsNullOrEmpty(DocumentoPSE))
                    {
                        ModelState.AddModelError(nameof(DocumentoPSE), "El número de documento es requerido para el pago con PSE.");
                        esValido = false;
                    }
                    else if (DocumentoPSE.Length < 6)
                    {
                        ModelState.AddModelError(nameof(DocumentoPSE), "El número de documento debe tener al menos 6 dígitos.");
                        esValido = false;
                    }
                    
                    if (string.IsNullOrEmpty(BancoPSE))
                    {
                        ModelState.AddModelError(nameof(BancoPSE), "Debe seleccionar un banco para el pago con PSE.");
                        esValido = false;
                    }
                    
                    break;

                case "EFECTY":
                    if (Total < 20000)
                    {
                        ModelState.AddModelError(string.Empty, "El monto mínimo para Efecty es $20,000 COP.");
                        esValido = false;
                    }
                    else if (Total > 1000000)
                    {
                        ModelState.AddModelError(string.Empty, "El monto máximo para Efecty es $1,000,000 COP. Tu compra es de $" + Total.ToString("N0") + " COP.");
                        esValido = false;
                    }
                    
                    if (string.IsNullOrEmpty(DocumentoEfecty))
                    {
                        ModelState.AddModelError(nameof(DocumentoEfecty), "El número de documento es requerido para el pago con Efecty.");
                        esValido = false;
                    }
                    else if (DocumentoEfecty.Length < 6)
                    {
                        ModelState.AddModelError(nameof(DocumentoEfecty), "El número de documento debe tener al menos 6 dígitos.");
                        esValido = false;
                    }
                    
                    break;

                case "NEQUI":
                    if (Total < 5000)
                    {
                        ModelState.AddModelError(string.Empty, "El monto mínimo para Nequi es $5,000 COP.");
                        esValido = false;
                    }
                    else if (Total > 2000000)
                    {
                        ModelState.AddModelError(string.Empty, "El monto máximo para Nequi es $2,000,000 COP. Tu compra es de $" + Total.ToString("N0") + " COP.");
                        esValido = false;
                    }
                    
                    if (string.IsNullOrEmpty(CelularNequi))
                    {
                        ModelState.AddModelError(nameof(CelularNequi), "El número de celular es requerido para el pago con Nequi.");
                        esValido = false;
                    }
                    else if (CelularNequi.Length != 10 || !CelularNequi.All(char.IsDigit))
                    {
                        ModelState.AddModelError(nameof(CelularNequi), "El número de celular debe tener exactamente 10 dígitos.");
                        esValido = false;
                    }
                    
                    break;

                default:
                    ModelState.AddModelError(string.Empty, "Método de pago no válido.");
                    esValido = false;
                    break;
            }

            return esValido;
        }

        private async Task<string> ProcesarPago(Pedido pedido, decimal total)
        {
            return MetodoPago?.ToUpper() switch
            {
                "TARJETA" => await _payUService.RealizarPago(
                    pedido, 
                    MetodoPago, 
                    NumeroTarjeta, 
                    CVV, 
                    ExpiracionMes, 
                    ExpiracionAnio, 
                    total, 
                    TipoTarjeta ?? "VISA",
                    null, null, null, null,
                    NombreTarjeta ?? "APRO"),
                    
                "PSE" => await _payUService.RealizarPago(
                    pedido, 
                    MetodoPago, 
                    null, // numeroTarjeta
                    null, // cvv
                    null, // expiracionMes
                    null, // expiracionAnio
                    total, 
                    null, // tipoTarjeta
                    DocumentoPSE, 
                    BancoPSE),
                    
                "EFECTY" => await _payUService.RealizarPago(
                    pedido, 
                    MetodoPago, 
                    null, // numeroTarjeta
                    null, // cvv
                    null, // expiracionMes
                    null, // expiracionAnio
                    total, 
                    null, // tipoTarjeta
                    null, // documentoPSE
                    null, // bancoPSE
                    DocumentoEfecty),
                    
                "NEQUI" => await _payUService.RealizarPago(
                    pedido, 
                    MetodoPago, 
                    null, // numeroTarjeta
                    null, // cvv
                    null, // expiracionMes
                    null, // expiracionAnio
                    total, 
                    null, // tipoTarjeta
                    null, // documentoPSE
                    null, // bancoPSE
                    null, // documentoEfecty
                    CelularNequi),
                    
                _ => throw new InvalidOperationException($"Método de pago no soportado: {MetodoPago}")
            };
        }

        private async Task<IActionResult> ProcesarRespuestaPayU(string respuestaPayU)
        {
            try
            {
                _logger.LogInformation("Procesando respuesta PayU: {Respuesta}", respuestaPayU);
                var payUResponse = JsonSerializer.Deserialize<PayUResponse>(respuestaPayU);

                if (payUResponse == null)
                {
                    _logger.LogError("Respuesta de PayU nula o malformada");
                    ModelState.AddModelError(string.Empty, "Error al procesar la respuesta del pago. Respuesta nula o malformada.");
                    return Page();
                }

                if (payUResponse.Error != null)
                {
                    _logger.LogError("Error recibido de PayU: {Error}", payUResponse.Error);
                    ModelState.AddModelError(string.Empty, $"Error de PayU: {payUResponse.Error}");
                    return Page();
                }

                if (payUResponse.TransactionResponse == null)
                {
                    _logger.LogError("Respuesta de PayU no contiene TransactionResponse");
                    ModelState.AddModelError(string.Empty, "Error al procesar la respuesta del pago. TransactionResponse nulo.");
                    return Page();
                }

                // Parámetros para la página de confirmación
                var redirectParams = new
                {
                    transactionState = payUResponse.TransactionResponse.State,
                    referenceCode = payUResponse.TransactionResponse.TrazabilityCode ?? "N/A",
                    message = payUResponse.TransactionResponse.PaymentNetworkResponseErrorMessage 
                             ?? payUResponse.TransactionResponse.ResponseMessage 
                             ?? "Transacción procesada",
                    orderId = payUResponse.TransactionResponse.OrderId.ToString(),
                    transactionId = payUResponse.TransactionResponse.TransactionId ?? "N/A",
                    paymentMethod = MetodoPago,
                    amount = Total.ToString("F2")
                };

                // Si el pago fue aprobado, limpiar el carrito y guardar la compra
                if (payUResponse.TransactionResponse.State?.Equals("APPROVED", StringComparison.OrdinalIgnoreCase) == true)
                {
                    try
                    {
                        _carritoService.GuardarUltimaCompra(ProductosCarrito);
                        _carritoService.VaciarCarrito();
                        _logger.LogInformation($"Compra exitosa. OrderId: {redirectParams.orderId}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al procesar carrito después del pago exitoso");
                    }
                }

                // Manejar casos especiales por método de pago
                if (payUResponse.TransactionResponse.State?.Equals("PENDING", StringComparison.OrdinalIgnoreCase) == true)
                {
                    switch (MetodoPago?.ToUpper())
                    {
                        case "PSE":
                            var bankUrl = payUResponse.TransactionResponse.ExtraParameters?.GetValueOrDefault("BANK_URL")?.ToString();
                            if (!string.IsNullOrEmpty(bankUrl))
                            {
                                return Redirect(bankUrl);
                            }
                            break;

                        case "EFECTY":
                        case "NEQUI":
                            // Para estos métodos, mostrar la página de confirmación con instrucciones
                            break;
                    }
                }

                // Para PSE DECLINED por BANK_UNREACHABLE, mostrar mensaje específico
                if (payUResponse.TransactionResponse.State?.Equals("DECLINED", StringComparison.OrdinalIgnoreCase) == true && 
                    MetodoPago?.ToUpper() == "PSE")
                {
                    var responseCode = payUResponse.TransactionResponse.ResponseCode;
                    if (responseCode?.Contains("BANK_UNREACHABLE") == true)
                    {
                        ModelState.AddModelError(string.Empty, "El servicio PSE no está disponible temporalmente. Los bancos sandbox de PayU tienen disponibilidad intermitente. Por favor intenta más tarde o usa otro método de pago.");
                        return Page();
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, $"El pago PSE fue rechazado: {payUResponse.TransactionResponse.PaymentNetworkResponseErrorMessage ?? "Error del banco"}");
                        return Page();
                    }
                }

                // Si el estado es REJECTED o ERROR, mostrar el mensaje real
                if (payUResponse.TransactionResponse.State?.Equals("REJECTED", StringComparison.OrdinalIgnoreCase) == true ||
                    payUResponse.TransactionResponse.State?.Equals("ERROR", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var errorMsg = payUResponse.TransactionResponse.PaymentNetworkResponseErrorMessage
                        ?? payUResponse.TransactionResponse.ResponseMessage
                        ?? "Pago rechazado por PayU";
                    ModelState.AddModelError(string.Empty, errorMsg);
                    _logger.LogError("Pago rechazado: {ErrorMsg}", errorMsg);
                    return Page();
                }

                return RedirectToPage("/ConfirmacionPago", redirectParams);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error al parsear respuesta de PayU: {Response}", respuestaPayU);
                ModelState.AddModelError(string.Empty, $"Error al procesar la respuesta del pago: {ex.Message}");
                return Page();
            }
        }

        // Clases para deserializar la respuesta de PayU
        public class PayUResponse
        {
            public string Code { get; set; }
            public string Error { get; set; }
            public TransactionResponse TransactionResponse { get; set; }
        }

        public class TransactionResponse
        {
            public string State { get; set; }
            public string PaymentNetworkResponseErrorMessage { get; set; }
            public string TrazabilityCode { get; set; }
            public long OrderId { get; set; }
            public string TransactionId { get; set; }
            public string ResponseMessage { get; set; }
            public string ResponseCode { get; set; }
            public Dictionary<string, object> ExtraParameters { get; set; }
        }
    }
}
