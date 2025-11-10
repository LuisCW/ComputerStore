using ComputerStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComputerStore.Pages
{
    public class BasePageModel : PageModel
    {
        protected readonly SignInManager<ApplicationUser> _signInManager;

        public BasePageModel(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public virtual async Task<IActionResult> OnPostAsync(string? action = null, string? returnUrl = null)
        {
            if (action == "logout")
            {
                return await HandleLogout(returnUrl);
            }
            
            return RedirectToPage();
        }

        protected async Task<IActionResult> HandleLogout(string? returnUrl = null)
        {
            // Manejar logout
            if (_signInManager.IsSignedIn(User))
            {
                await _signInManager.SignOutAsync();
                HttpContext.Session.Clear();
                
                // Limpiar cookies de autenticación
                foreach (var cookieName in Request.Cookies.Keys)
                {
                    if (cookieName.StartsWith("AspNetCore.Identity") || 
                        cookieName.StartsWith("AspNetCore.Session") ||
                        cookieName.Contains("auth"))
                    {
                        Response.Cookies.Delete(cookieName);
                    }
                }
            }
            
            // Determinar dónde redirigir
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                var normalizedUrl = returnUrl.ToLowerInvariant();
                
                // Si la página requiere autenticación, ir al inicio
                if (normalizedUrl.Contains("/account/") || 
                    normalizedUrl.Contains("/littlecar") || 
                    normalizedUrl.Contains("/envios") ||
                    normalizedUrl.Contains("/admin/"))
                {
                    return Redirect("/");
                }
                
                // Si es página pública, regresar ahí
                return Redirect(returnUrl);
            }
            
            // Por defecto, ir al inicio
            return Redirect("/");
        }
    }
}