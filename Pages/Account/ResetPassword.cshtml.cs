using ComputerStore.Models;
using ComputerStore.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace ComputerStore.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
  private readonly UserManager<ApplicationUser> _userManager;
 private readonly ILogger<ResetPasswordModel> _logger;

    public ResetPasswordModel(
 UserManager<ApplicationUser> userManager,
   ILogger<ResetPasswordModel> logger)
        {
_userManager = userManager;
       _logger = logger;
     }

   [BindProperty]
   public ResetPasswordViewModel Input { get; set; } = new();

[TempData]
  public string? StatusMessage { get; set; }

   public IActionResult OnGet(string? email = null, string? token = null)
        {
 if (email == null || token == null)
      {
  return RedirectToPage("./ForgotPassword");
            }

        Input.Email = email;
        Input.Token = token;
  
            return Page();
        }

  public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine($"?? ========== RESET PASSWORD ==========");
   Console.WriteLine($"?? Email: {Input.Email}");
            
   if (!ModelState.IsValid)
     {
   Console.WriteLine($"? ModelState inválido");
    return Page();
}

     var user = await _userManager.FindByEmailAsync(Input.Email);
  if (user == null)
  {
     Console.WriteLine($"? Usuario no encontrado");
   _logger.LogWarning("Intento de reseteo de contraseña para usuario no existente: {Email}", Input.Email);
     TempData["ErrorMessage"] = "Hubo un error al restablecer la contraseña.";
  return RedirectToPage("./Login");
     }

   try
  {
       Console.WriteLine($"?? Intentando resetear contraseña...");
         var result = await _userManager.ResetPasswordAsync(user, Input.Token, Input.Password);
   
        if (result.Succeeded)
      {
    Console.WriteLine($"? Contraseña restablecida exitosamente");
        _logger.LogInformation("Usuario {Email} restableció su contraseña exitosamente.", Input.Email);
         
   // Enviar email de confirmación EN SEGUNDO PLANO
        _ = Task.Run(async () =>
      {
     try
    {
      var emailService = HttpContext.RequestServices.GetService<EmailService>();
     if (emailService != null)
   {
        var userName = $"{user.PrimerNombre} {user.PrimerApellido}";
       Console.WriteLine($"?? [Background] Enviando email de confirmación...");
         await SendPasswordChangedEmailAsync(emailService, Input.Email, userName);
          Console.WriteLine($"? [Background] Email de confirmación enviado");
  }
       }
    catch (Exception ex)
             {
      Console.WriteLine($"? [Background] Error al enviar email: {ex.Message}");
        _logger.LogWarning(ex, "No se pudo enviar email de confirmación");
      }
   });

       TempData["SuccessMessage"] = "Tu contraseña ha sido restablecida exitosamente. Ya puedes iniciar sesión.";
    Console.WriteLine($"?? Redirigiendo a Login");
     return RedirectToPage("./Login");
    }
   
   Console.WriteLine($"? Error al resetear contraseña:");
   foreach (var error in result.Errors)
       {
       Console.WriteLine($"   - {error.Code}: {error.Description}");
      if (error.Code == "InvalidToken")
       {
  ModelState.AddModelError(string.Empty, "El enlace de recuperación ha expirado o es inválido. Solicita uno nuevo.");
   }
           else
   {
  ModelState.AddModelError(string.Empty, error.Description);
         }
 }
 }
      catch (Exception ex)
    {
       Console.WriteLine($"? Excepción: {ex.Message}");
       _logger.LogError(ex, "Error al resetear contraseña para {Email}", Input.Email);
  ModelState.AddModelError(string.Empty, "Ocurrió un error al restablecer la contraseña. Inténtalo de nuevo.");
  }

    return Page();
     }

        private async Task SendPasswordChangedEmailAsync(EmailService emailService, string email, string userName)
     {
      var subject = "Contraseña Cambiada - CompuHiperMegaRed";
            
   var body = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
     .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
     .header {{ background: linear-gradient(135deg, #198754, #157347); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
 .content {{ background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px; }}
     .alert-box {{ background: #d1e7dd; border-left: 4px solid #198754; padding: 15px; margin: 20px 0; }}
        .footer {{ text-align: center; color: #6c757d; font-size: 12px; margin-top: 20px; }}
        .button {{ display: inline-block; padding: 12px 24px; background: #198754; color: white; text-decoration: none; border-radius: 5px; margin-top: 15px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
    <h1>? Contraseña Actualizada</h1>
        </div>
        <div class='content'>
        <p>Hola <strong>{userName}</strong>,</p>
     
  <div class='alert-box'>
   <p style='margin: 0;'><strong>? Tu contraseña ha sido cambiada exitosamente</strong></p>
  </div>
   
     <p>Tu contraseña de <strong>CompuHiperMegaRed</strong> se ha actualizado correctamente el {DateTime.Now:dd/MM/yyyy} a las {DateTime.Now:HH:mm} (hora del servidor).</p>

    <p>Si no realizaste este cambio, por favor contacta inmediatamente con nuestro equipo de soporte.</p>
   
     <div style='text-align: center;'>
        <a href='https://localhost:7100/Account/Login' class='button'>Iniciar Sesión Ahora</a>
</div>
       
            <p style='margin-top: 30px;'>Saludos,<br><strong>El equipo de CompuHiperMegaRed</strong></p>
        </div>
    <div class='footer'>
      <p>Este es un correo automático, por favor no respondas a este mensaje.</p>
   <p>&copy; {DateTime.Now.Year} CompuHiperMegaRed. Todos los derechos reservados.</p>
            <p>Si no reconoces esta actividad, contacta: soporte@computerhipermegared.com</p>
      </div>
    </div>
</body>
</html>";

      await emailService.SendEmailAsync(email, subject, body);
        }
    }
}
