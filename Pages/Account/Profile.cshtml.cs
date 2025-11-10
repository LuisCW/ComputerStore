using ComputerStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ComputerStore.Pages.Account
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ProfileModel> _logger;

        public ProfileModel(UserManager<ApplicationUser> userManager, ILogger<ProfileModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public ProfileInputModel Input { get; set; } = new();

        public class ProfileInputModel
        {
            [Required(ErrorMessage = "El primer nombre es requerido")]
            [StringLength(100, ErrorMessage = "El primer nombre no puede exceder 100 caracteres")]
            [Display(Name = "Primer Nombre")]
            public string PrimerNombre { get; set; } = string.Empty;

            [StringLength(100, ErrorMessage = "El segundo nombre no puede exceder 100 caracteres")]
            [Display(Name = "Segundo Nombre")]
            public string? SegundoNombre { get; set; }

            [Required(ErrorMessage = "El primer apellido es requerido")]
            [StringLength(100, ErrorMessage = "El primer apellido no puede exceder 100 caracteres")]
            [Display(Name = "Primer Apellido")]
            public string PrimerApellido { get; set; } = string.Empty;

            [StringLength(100, ErrorMessage = "El segundo apellido no puede exceder 100 caracteres")]
            [Display(Name = "Segundo Apellido")]
            public string? SegundoApellido { get; set; }

            [Required(ErrorMessage = "El email es requerido")]
            [EmailAddress(ErrorMessage = "Formato de email inválido")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Phone(ErrorMessage = "Formato de teléfono inválido")]
            [Display(Name = "Teléfono")]
            public string? PhoneNumber { get; set; }

            [Required(ErrorMessage = "El tipo de documento es requerido")]
            [Display(Name = "Tipo de Documento")]
            public string TipoDocumento { get; set; } = string.Empty;

            [Required(ErrorMessage = "El número de documento es requerido")]
            [StringLength(20, ErrorMessage = "El número de documento no puede exceder 20 caracteres")]
            [Display(Name = "Número de Documento")]
            public string NumeroDocumento { get; set; } = string.Empty;

            [Required(ErrorMessage = "La dirección es requerida")]
            [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
            [Display(Name = "Dirección Principal")]
            public string Direccion { get; set; } = string.Empty;

            [StringLength(200, ErrorMessage = "La dirección secundaria no puede exceder 200 caracteres")]
            [Display(Name = "Dirección Secundaria")]
            public string? DireccionSecundaria { get; set; }

            [Required(ErrorMessage = "La ciudad es requerida")]
            [StringLength(100, ErrorMessage = "La ciudad no puede exceder 100 caracteres")]
            [Display(Name = "Ciudad")]
            public string Ciudad { get; set; } = string.Empty;

            [Required(ErrorMessage = "El departamento es requerido")]
            [StringLength(100, ErrorMessage = "El departamento no puede exceder 100 caracteres")]
            [Display(Name = "Departamento")]
            public string Departamento { get; set; } = string.Empty;

            [Required(ErrorMessage = "El código postal es requerido")]
            [StringLength(10, ErrorMessage = "El código postal no puede exceder 10 caracteres")]
            [Display(Name = "Código Postal")]
            public string CodigoPostal { get; set; } = string.Empty;

            [StringLength(500, ErrorMessage = "Las notas de entrega no pueden exceder 500 caracteres")]
            [Display(Name = "Notas de Entrega")]
            public string? NotasEntrega { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("No se pudo cargar el usuario.");
            }

            await LoadUserDataAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("No se pudo cargar el usuario.");
            }

            if (!ModelState.IsValid)
            {
                await LoadUserDataAsync(user);
                return Page();
            }

            // Verificar si el email cambió y si ya existe
            if (Input.Email != user.Email)
            {
                var existingUser = await _userManager.FindByEmailAsync(Input.Email);
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    ModelState.AddModelError(string.Empty, "El email ya está en uso por otro usuario.");
                    await LoadUserDataAsync(user);
                    return Page();
                }
            }

            // Verificar si el número de documento cambió y si ya existe
            if (Input.NumeroDocumento != user.NumeroDocumento)
            {
                var existingUserByDoc = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.NumeroDocumento == Input.NumeroDocumento && u.Id != user.Id);
                if (existingUserByDoc != null)
                {
                    ModelState.AddModelError(string.Empty, "El número de documento ya está en uso por otro usuario.");
                    await LoadUserDataAsync(user);
                    return Page();
                }
            }

            // Actualizar datos del usuario
            user.PrimerNombre = Input.PrimerNombre;
            user.SegundoNombre = Input.SegundoNombre;
            user.PrimerApellido = Input.PrimerApellido;
            user.SegundoApellido = Input.SegundoApellido;
            user.Email = Input.Email;
            user.UserName = Input.Email; // Usar email como username
            user.PhoneNumber = Input.PhoneNumber;
            user.TipoDocumento = Input.TipoDocumento;
            user.NumeroDocumento = Input.NumeroDocumento;
            user.Direccion = Input.Direccion;
            user.DireccionSecundaria = Input.DireccionSecundaria;
            user.Ciudad = Input.Ciudad;
            user.Departamento = Input.Departamento;
            user.CodigoPostal = Input.CodigoPostal;
            user.NotasEntrega = Input.NotasEntrega;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("Usuario {UserId} actualizó su perfil exitosamente.", user.Id);
                TempData["SuccessMessage"] = "Perfil actualizado exitosamente.";
                return RedirectToPage();
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            await LoadUserDataAsync(user);
            return Page();
        }

        private async Task LoadUserDataAsync(ApplicationUser user)
        {
            Input.PrimerNombre = user.PrimerNombre;
            Input.SegundoNombre = user.SegundoNombre;
            Input.PrimerApellido = user.PrimerApellido;
            Input.SegundoApellido = user.SegundoApellido;
            Input.Email = user.Email ?? string.Empty;
            Input.PhoneNumber = user.PhoneNumber;
            Input.TipoDocumento = user.TipoDocumento;
            Input.NumeroDocumento = user.NumeroDocumento;
            Input.Direccion = user.Direccion;
            Input.DireccionSecundaria = user.DireccionSecundaria;
            Input.Ciudad = user.Ciudad;
            Input.Departamento = user.Departamento;
            Input.CodigoPostal = user.CodigoPostal;
            Input.NotasEntrega = user.NotasEntrega;
        }
    }
}