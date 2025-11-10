using ComputerStore.Models;
using ComputerStore.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComputerStore.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public RegisterViewModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                // Verificar si el email ya existe
                var existingUserByEmail = await _userManager.FindByEmailAsync(Input.Email);
                if (existingUserByEmail != null)
                {
                    ModelState.AddModelError("Input.Email", "Ya existe una cuenta con este correo electrónico.");
                    return Page();
                }

                // Verificar si el número de documento ya existe
                var existingUserByDocument = _userManager.Users.FirstOrDefault(u => u.NumeroDocumento == Input.NumeroDocumento);
                if (existingUserByDocument != null)
                {
                    ModelState.AddModelError("Input.NumeroDocumento", "Ya existe una cuenta con este número de documento.");
                    return Page();
                }

                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    PrimerNombre = Input.PrimerNombre,
                    SegundoNombre = Input.SegundoNombre ?? "",
                    PrimerApellido = Input.PrimerApellido,
                    SegundoApellido = Input.SegundoApellido ?? "",
                    TipoDocumento = Input.TipoDocumento,
                    NumeroDocumento = Input.NumeroDocumento,
                    PhoneNumber = Input.PhoneNumber,
                    Direccion = Input.Direccion,
                    Ciudad = Input.Ciudad,
                    Departamento = Input.Departamento,
                    CodigoPostal = Input.CodigoPostal,
                    DireccionSecundaria = Input.DireccionSecundaria,
                    NotasEntrega = Input.NotasEntrega,
                    FechaRegistro = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Usuario creó una nueva cuenta con contraseña.");

                    // Asignar rol de User por defecto
                    await _userManager.AddToRoleAsync(user, "User");

                    // Opcional: Confirmar email automáticamente para testing
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    
                    TempData["SuccessMessage"] = "¡Registro exitoso! Bienvenido a CompuHiperMegaRed.";
                    return LocalRedirect(returnUrl);
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Si llegamos hasta aquí, algo falló, volver a mostrar el formulario
            return Page();
        }
    }
}