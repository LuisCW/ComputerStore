using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class EmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            _logger.LogInformation("Enviando email a {Email} con asunto: {Subject}", toEmail, subject);

            // Configuración directa para Gmail
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("lugapemu98@gmail.com", "cqlnnaavreoahunu"), // App password
                EnableSsl = true,
                Timeout = 30000
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("lugapemu98@gmail.com", "CompuHiperMegaRed"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
                Priority = MailPriority.Normal
            };
            
            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("Email enviado exitosamente a {Email}", toEmail);
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "Error SMTP al enviar email a {Email}: {Message}", toEmail, ex.Message);
            throw new InvalidOperationException($"Error al enviar email: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error general al enviar email a {Email}", toEmail);
            throw;
        }
    }

    public async Task SendPasswordResetCodeAsync(string email, string code, string userName)
    {
        Console.WriteLine($"📧 Preparando email de recuperación para {email}");
        
        var subject = "Código de Recuperación de Contraseña - CompuHiperMegaRed";

        var body = $@"
<!DOCTYPE html>
<html>
<head>
 <style>
body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
    .header {{ background: linear-gradient(135deg, #0d6efd, #0b5ed7); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
     .content {{ background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px; }}
   .code-box {{ background: white; border: 2px dashed #0d6efd; padding: 20px; margin: 20px 0; text-align: center; border-radius: 10px; }}
        .code {{ font-size: 32px; font-weight: bold; color: #0d6efd; letter-spacing: 5px; }}
     .warning {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; }}
      .footer {{ text-align: center; color: #6c757d; font-size: 12px; margin-top: 20px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔐 Recuperación de Contraseña</h1>
        </div>
    <div class='content'>
   <p>Hola <strong>{userName}</strong>,</p>
         
 <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta en <strong>CompuHiperMegaRed</strong>.</p>
        
   <div class='code-box'>
    <p style='margin: 0; color: #6c757d;'>Tu código de verificación es:</p>
  <div class='code'>{code}</div>
  <p style='margin: 10px 0 0 0; color: #6c757d; font-size: 14px;'>Este código expira en 15 minutos</p>
    </div>
  
     <p>Ingresa este código en la página de recuperación de contraseña para continuar con el proceso.</p>
       
         <div class='warning'>
   <p style='margin: 0;'><strong>⚠️ Importante:</strong></p>
   <ul style='margin: 10px 0 0 20px; padding: 0;'>
   <li>Si no solicitaste este cambio, ignora este correo.</li>
  <li>Nunca compartas este código con nadie.</li>
         <li>Nuestro equipo nunca te pedirá este código por teléfono o email.</li>
      </ul>
    </div>
        
         <p>Si tienes alguna pregunta o necesitas ayuda, no dudes en contactarnos.</p>
     
   <p>Saludos,<br><strong>El equipo de CompuHiperMegaRed</strong></p>
        </div>
        <div class='footer'>
            <p>Este es un correo automático, por favor no respondas a este mensaje.</p>
<p>&copy; {DateTime.Now.Year} CompuHiperMegaRed. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";

        try
        {
     await SendEmailAsync(email, subject, body);
            Console.WriteLine($"✅ Email de recuperación enviado a {email}");
  }
        catch (Exception ex)
  {
       Console.WriteLine($"❌ ERROR al enviar email de recuperación: {ex.Message}");
            throw;
        }
    }

    public async Task SendBulkEmailAsync(List<string> toEmails, string subject, string body)
    {
        var tasks = toEmails.Select(email => SendEmailAsync(email, subject, body));
        await Task.WhenAll(tasks);
    }

    public async Task SendEmailWithAttachmentAsync(string toEmail, string subject, string body, string attachmentPath)
    {
        try
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("lugapemu98@gmail.com", "cqlnnaavreoahunu"),
                EnableSsl = true,
                Timeout = 60000 // Mayor timeout para archivos adjuntos
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("lugapemu98@gmail.com", "CompuHiperMegaRed"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            
            mailMessage.To.Add(toEmail);

            if (File.Exists(attachmentPath))
            {
                var attachment = new Attachment(attachmentPath);
                mailMessage.Attachments.Add(attachment);
            }

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("Email con adjunto enviado exitosamente a {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar email con adjunto a {Email}", toEmail);
            throw;
        }
    }
}
