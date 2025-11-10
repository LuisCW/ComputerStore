using ComputerStore.Models;
using ComputerStore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace ComputerStore.Pages
{
    public class SupportModel : PageModel
    {
        private readonly EmailService _emailService;
        private readonly ILogger<SupportModel> _logger;

        [BindProperty]
        public ServiceRequest ServiceRequest { get; set; } = new ServiceRequest();

        public string Message { get; set; } = string.Empty;

        public SupportModel(EmailService emailService, ILogger<SupportModel> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public void OnGet()
        {
            // Página cargada
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Formulario de soporte inválido: {Errors}", 
                        string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                    
                    return new JsonResult(new { 
                        success = false, 
                        message = "Por favor, completa todos los campos requeridos." 
                    });
                }

                // Generar número de ticket
                var numeroTicket = $"SUPP-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";

                // Crear contenido del email para el administrador
                var emailContent = GenerarEmailSoporte(ServiceRequest, numeroTicket);

                // Enviar email al administrador
                await _emailService.SendEmailAsync(
                    "lugapemu98@gmail.com",
                    $"Nueva Solicitud de Soporte - Ticket #{numeroTicket}",
                    emailContent
                );

                // Crear contenido del email de confirmación para el cliente
                var confirmationEmailContent = GenerarEmailConfirmacion(ServiceRequest, numeroTicket);

                // Enviar email de confirmación al cliente
                await _emailService.SendEmailAsync(
                    ServiceRequest.Email,
                    $"Confirmación de Solicitud de Soporte - Ticket #{numeroTicket}",
                    confirmationEmailContent
                );

                _logger.LogInformation("Solicitud de soporte enviada exitosamente. Ticket: {Ticket}, Cliente: {Email}", 
                    numeroTicket, ServiceRequest.Email);

                return new JsonResult(new { 
                    success = true, 
                    message = $"Solicitud enviada exitosamente. Tu número de ticket es: {numeroTicket}. Recibirás una confirmación por email." 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar solicitud de soporte para {Email}", ServiceRequest?.Email);
                
                return new JsonResult(new { 
                    success = false, 
                    message = "Hubo un error al enviar tu solicitud. Por favor, inténtalo nuevamente o contáctanos directamente." 
                });
            }
        }

        private string GenerarEmailSoporte(ServiceRequest request, string numeroTicket)
        {
            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .header {{ background-color: #2563eb; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .info-box {{ background-color: #f8f9fa; padding: 15px; margin: 10px 0; border-left: 4px solid #2563eb; }}
                        .urgent {{ border-left-color: #ef4444; }}
                        .footer {{ background-color: #6b7280; color: white; padding: 15px; text-align: center; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <h1>?? Nueva Solicitud de Soporte Técnico</h1>
                        <h2>Ticket #{numeroTicket}</h2>
                    </div>
                    
                    <div class='content'>
                        <div class='info-box'>
                            <h3>?? Información del Cliente</h3>
                            <p><strong>Nombre:</strong> {request.Name}</p>
                            <p><strong>Email:</strong> {request.Email}</p>
                            <p><strong>Teléfono:</strong> {request.Phone}</p>
                            <p><strong>Fecha de Solicitud:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                        </div>
                        
                        <div class='info-box'>
                            <h3>?? Información del Equipo</h3>
                            <p><strong>Tipo de Equipo:</strong> {request.DeviceType}</p>
                            <p><strong>Marca:</strong> {request.Brand ?? "No especificada"}</p>
                        </div>
                        
                        <div class='info-box urgent'>
                            <h3>?? Descripción del Problema</h3>
                            <p>{request.Problem}</p>
                        </div>
                        
                        <div class='info-box'>
                            <h3>?? Próximos Pasos</h3>
                            <ol>
                                <li>Contactar al cliente en las próximas 2 horas</li>
                                <li>Programar cita para diagnóstico</li>
                                <li>Enviar cotización si es necesario</li>
                            </ol>
                        </div>
                    </div>
                    
                    <div class='footer'>
                        <p>CompuHiperMegaRed - Sistema de Gestión de Soporte</p>
                        <p>Email generado automáticamente - {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
                    </div>
                </body>
                </html>";
        }

        private string GenerarEmailConfirmacion(ServiceRequest request, string numeroTicket)
        {
            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .header {{ background-color: #059669; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; }}
                        .info-box {{ background-color: #f0fdf4; padding: 15px; margin: 10px 0; border-left: 4px solid #059669; }}
                        .contact-box {{ background-color: #eff6ff; padding: 15px; margin: 10px 0; border-left: 4px solid #2563eb; }}
                        .footer {{ background-color: #6b7280; color: white; padding: 15px; text-align: center; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='header'>
                        <h1>? Solicitud de Soporte Recibida</h1>
                        <h2>Ticket #{numeroTicket}</h2>
                    </div>
                    
                    <div class='content'>
                        <p>Hola <strong>{request.Name}</strong>,</p>
                        
                        <p>Hemos recibido tu solicitud de soporte técnico y nuestro equipo la está revisando.</p>
                        
                        <div class='info-box'>
                            <h3>?? Resumen de tu Solicitud</h3>
                            <p><strong>Número de Ticket:</strong> {numeroTicket}</p>
                            <p><strong>Equipo:</strong> {request.DeviceType} {request.Brand}</p>
                            <p><strong>Problema Reportado:</strong> {request.Problem}</p>
                            <p><strong>Fecha:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                        </div>
                        
                        <div class='contact-box'>
                            <h3>? ¿Qué sigue?</h3>
                            <ul>
                                <li><strong>Tiempo de Respuesta:</strong> Te contactaremos en las próximas 2 horas hábiles</li>
                                <li><strong>Diagnóstico:</strong> Programaremos una cita para revisar tu equipo</li>
                                <li><strong>Cotización:</strong> Si es necesario, te enviaremos un presupuesto sin compromiso</li>
                            </ul>
                        </div>
                        
                        <div class='contact-box'>
                            <h3>?? Información de Contacto</h3>
                            <p><strong>Email:</strong> lugapemu98@gmail.com</p>
                            <p><strong>Teléfono:</strong> +57 300 123 4567</p>
                            <p><strong>Horario:</strong> Lunes a Viernes 8:00 AM - 6:00 PM</p>
                        </div>
                        
                        <p><em>Guarda este email como referencia. Tu número de ticket es: <strong>{numeroTicket}</strong></em></p>
                    </div>
                    
                    <div class='footer'>
                        <p>CompuHiperMegaRed - Tu especialista en tecnología</p>
                        <p>Gracias por confiar en nosotros</p>
                    </div>
                </body>
                </html>";
        }
    }

    public class ServiceRequest
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es requerido")]
        [Phone(ErrorMessage = "Teléfono inválido")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe especificar el tipo de equipo")]
        public string DeviceType { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "La marca no puede exceder 50 caracteres")]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe describir el problema")]
        [StringLength(1000, ErrorMessage = "La descripción no puede exceder 1000 caracteres")]
        public string Problem { get; set; } = string.Empty;
    }
}
