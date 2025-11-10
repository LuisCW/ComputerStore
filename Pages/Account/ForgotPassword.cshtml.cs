using ComputerStore.Models;
using ComputerStore.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComputerStore.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
private readonly UserManager<ApplicationUser> _userManager;
     private readonly EmailService _emailService;
        private readonly ILogger<ForgotPasswordModel> _logger;

        public ForgotPasswordModel(
            UserManager<ApplicationUser> userManager,
     EmailService emailService,
            ILogger<ForgotPasswordModel> logger)
        {
       _userManager = userManager;
            _emailService = emailService;
     _logger = logger;
  }

        [BindProperty]
    public ForgotPasswordViewModel Input { get; set; } = new();

    public string VerificationCode { get; set; } = string.Empty;
        public bool CodeSent { get; set; }

        public void OnGet()
        {
      var sessionEmail = HttpContext.Session.GetString("PasswordResetEmail");
            Console.WriteLine($"?? OnGet - Session Email: {sessionEmail ?? "NULL"}");
     
       if (!string.IsNullOrEmpty(sessionEmail))
            {
       Input.Email = sessionEmail;
  CodeSent = true;
            }
        }

    public async Task<IActionResult> OnPostAsync()
        {
          Console.WriteLine($"?? POST - Email: {Input.Email}");

 if (!ModelState.IsValid)
   {
   return Page();
   }

  var user = await _userManager.FindByEmailAsync(Input.Email);
 if (user == null)
     {
          Console.WriteLine($"? Usuario no encontrado: {Input.Email}");
     
   HttpContext.Session.SetString("PasswordResetEmail", Input.Email);
        HttpContext.Session.SetString("PasswordResetCode", "000000");
       HttpContext.Session.SetString("PasswordResetExpiry", DateTime.UtcNow.AddMinutes(15).ToString("O"));
   await HttpContext.Session.CommitAsync();

        Console.WriteLine("?? Session guardada (fake)");
 return RedirectToPage();
    }

      var code = new Random().Next(100000, 999999).ToString();
Console.WriteLine($"?? ========================================");
            Console.WriteLine($"?? CÓDIGO GENERADO: {code}");
Console.WriteLine($"?? ========================================");

  // USAR HORA LOCAL DE COLOMBIA en lugar de UTC
        var colombiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time");
      var nowColombia = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, colombiaTimeZone);
        var expiryColombia = nowColombia.AddMinutes(15);
        
  Console.WriteLine($"?? Hora actual Colombia: {nowColombia:yyyy-MM-dd HH:mm:ss}");
     Console.WriteLine($"?? Expira en: {expiryColombia:yyyy-MM-dd HH:mm:ss}");

      // Guardar en Session PRIMERO
 HttpContext.Session.SetString("PasswordResetEmail", Input.Email);
   HttpContext.Session.SetString("PasswordResetCode", code);
 HttpContext.Session.SetString("PasswordResetExpiry", expiryColombia.ToString("O")); // Guardar hora de Colombia
 HttpContext.Session.SetString("PasswordResetTimeZone", "SA Pacific Standard Time"); // Guardar zona horaria
       await HttpContext.Session.CommitAsync();

      Console.WriteLine($"?? Session guardada correctamente");

        // NO ESPERAR POR EL EMAIL - enviarlo en background
 var userName = $"{user.PrimerNombre} {user.PrimerApellido}";
      _ = Task.Run(async () =>
      {
     try
   {
   Console.WriteLine($"?? [Background] Enviando email a {Input.Email}...");
    await _emailService.SendPasswordResetCodeAsync(Input.Email, code, userName);
       Console.WriteLine($"? [Background] Email enviado exitosamente");
   }
       catch (Exception ex)
  {
          Console.WriteLine($"? [Background] Error al enviar email: {ex.Message}");
      _logger.LogError(ex, "Error al enviar email");
   }
        });

      Console.WriteLine($"?? Redirigiendo inmediatamente al formulario de verificación...");
 return RedirectToPage();
 }
        public async Task<IActionResult> OnPostVerifyCodeAsync(string verificationCode)
        {
Console.WriteLine($"");
        Console.WriteLine($"?? ========== VERIFICANDO CÓDIGO ==========");
      Console.WriteLine($"?? Código recibido: '{verificationCode}'");
  
  var savedEmail = HttpContext.Session.GetString("PasswordResetEmail");
        var savedCode = HttpContext.Session.GetString("PasswordResetCode");
 var savedExpiry = HttpContext.Session.GetString("PasswordResetExpiry");

 Console.WriteLine($"?? Email en session: '{savedEmail ?? "NULL"}'");
 Console.WriteLine($"?? Código en session: '{savedCode ?? "NULL"}'");

     if (string.IsNullOrEmpty(savedEmail) || string.IsNullOrEmpty(savedCode))
            {
       Console.WriteLine($"? ERROR CRÍTICO: No hay datos en session");
      Console.WriteLine($"? Limpiando session y volviendo al inicio");
       HttpContext.Session.Clear();
      TempData["ErrorMessage"] = "Sesión expirada. Por favor, solicita un nuevo código.";
        return RedirectToPage();
      }

   if (!string.IsNullOrEmpty(savedExpiry))
   {
        // Obtener zona horaria de la session o usar Colombia por defecto
    var timeZoneId = HttpContext.Session.GetString("PasswordResetTimeZone") ?? "SA Pacific Standard Time";
     var colombiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        
        var expiry = DateTime.Parse(savedExpiry);
    var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, colombiaTimeZone);
      
       Console.WriteLine($"? Verificación de expiración:");
 Console.WriteLine($"   Hora actual Colombia: {now:yyyy-MM-dd HH:mm:ss}");
  Console.WriteLine($"   Código expira: {expiry:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"   Diferencia: {(expiry - now).TotalMinutes:F1} minutos");
    
  if (now > expiry)
     {
  Console.WriteLine($"? Código expirado");
 HttpContext.Session.Clear();
      TempData["ErrorMessage"] = "El código ha expirado. Solicita uno nuevo.";
return RedirectToPage();
   }
else
        {
  Console.WriteLine($"? Código aún válido");
  }
   }

            if (string.IsNullOrWhiteSpace(verificationCode))
   {
    Console.WriteLine($"? Código vacío");
 ModelState.AddModelError(string.Empty, "Debes ingresar el código.");
 Input.Email = savedEmail;
       CodeSent = true;
  return Page();
}

       if (verificationCode.Length != 6)
 {
    Console.WriteLine($"? Longitud incorrecta: {verificationCode.Length}");
 ModelState.AddModelError(string.Empty, "El código debe tener 6 dígitos.");
    Input.Email = savedEmail;
       CodeSent = true;
  return Page();
    }

   // Comparar códigos (TRIM para evitar espacios)
     var cleanCode = verificationCode.Trim();
       var cleanSavedCode = savedCode.Trim();
   
        Console.WriteLine($"?? Comparando:");
   Console.WriteLine($"   Recibido: '{cleanCode}' (longitud: {cleanCode.Length})");
     Console.WriteLine($"   Guardado: '{cleanSavedCode}' (longitud: {cleanSavedCode.Length})");
   
    if (cleanCode != cleanSavedCode)
     {
        Console.WriteLine($"? ? ? CÓDIGO INCORRECTO ? ? ?");
      // NO MOSTRAR EL CÓDIGO ESPERADO - ES UN ERROR DE SEGURIDAD
        ModelState.AddModelError(string.Empty, "Código incorrecto. Inténtalo de nuevo.");
   Input.Email = savedEmail;
     CodeSent = true;
         return Page();
     }

   Console.WriteLine($"? ? ? CÓDIGO CORRECTO ? ? ?");

       var user = await _userManager.FindByEmailAsync(savedEmail);
     if (user == null)
     {
 Console.WriteLine($"? Usuario no encontrado");
     HttpContext.Session.Clear();
   ModelState.AddModelError(string.Empty, "Usuario no encontrado.");
      CodeSent = false;
      return Page();
      }

    Console.WriteLine($"? Usuario encontrado: {user.Email}");
    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
Console.WriteLine($"? Token generado correctamente");
  
   // Limpiar session
            HttpContext.Session.Clear();
   await HttpContext.Session.CommitAsync();
 
        Console.WriteLine($"");
       Console.WriteLine($"?? ?? ?? REDIRIGIENDO A RESETPASSWORD ?? ?? ??");
      Console.WriteLine($"   Email: {savedEmail}");
       Console.WriteLine($"   Token Length: {token.Length}");
      Console.WriteLine($"");
    
     return RedirectToPage("/Account/ResetPassword", new { email = savedEmail, token = token });
     }
        public async Task<IActionResult> OnPostResendCodeAsync()
        {
    HttpContext.Session.Clear();
         return RedirectToPage();
  }
    }
}
