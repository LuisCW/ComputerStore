using ComputerStore.Models;
using ComputerStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComputerStore.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class UsersModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IExcelExportService _excelExportService;

        public List<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public List<ApplicationUser> Admins { get; set; } = new List<ApplicationUser>();

        [BindProperty]
        public CreateAdminViewModel NewAdmin { get; set; } = new CreateAdminViewModel();

        public UsersModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IExcelExportService excelExportService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _excelExportService = excelExportService;
        }

        public async Task OnGetAsync()
        {
            await LoadUsers();
        }

        // ?? NUEVO: Exportar usuarios a Excel
        public async Task<IActionResult> OnGetExportUsersAsync()
        {
            try
            {
                await LoadUsers();
                
                var excelBytes = await _excelExportService.ExportUsersToExcelAsync(Users, Admins);
                
                var fileName = $"Usuarios_ComputerStore_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al exportar usuarios: {ex.Message}";
                return RedirectToPage();
            }
        }

        // ?? NUEVO: Exportar análisis de usuarios a Excel
        public async Task<IActionResult> OnGetExportUsersAnalysisAsync()
        {
            try
            {
                await LoadUsers();
                
                var excelBytes = await _excelExportService.ExportUsersAnalysisAsync(Users, Admins);
                
                var fileName = $"Analisis_Usuarios_ComputerStore_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al exportar análisis de usuarios: {ex.Message}";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostCreateAdminAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadUsers();
                return Page();
            }

            try
            {
                // Verificar si el usuario ya existe
                var existingUser = await _userManager.FindByEmailAsync(NewAdmin.Email);
                if (existingUser != null)
                {
                    TempData["ErrorMessage"] = "Ya existe un usuario con ese email.";
                    await LoadUsers();
                    return Page();
                }

                existingUser = await _userManager.FindByNameAsync(NewAdmin.UserName);
                if (existingUser != null)
                {
                    TempData["ErrorMessage"] = "Ya existe un usuario con ese nombre de usuario.";
                    await LoadUsers();
                    return Page();
                }

                // Crear nuevo usuario administrador
                var newUser = new ApplicationUser
                {
                    UserName = NewAdmin.UserName,
                    Email = NewAdmin.Email,
                    EmailConfirmed = true,
                    PrimerNombre = NewAdmin.PrimerNombre,
                    SegundoNombre = NewAdmin.SegundoNombre ?? "",
                    PrimerApellido = NewAdmin.PrimerApellido,
                    SegundoApellido = NewAdmin.SegundoApellido ?? "",
                    TipoDocumento = NewAdmin.TipoDocumento,
                    NumeroDocumento = NewAdmin.NumeroDocumento,
                    Direccion = NewAdmin.Direccion ?? "",
                    Ciudad = NewAdmin.Ciudad ?? "Bogotá",
                    Departamento = NewAdmin.Departamento ?? "Cundinamarca",
                    CodigoPostal = NewAdmin.CodigoPostal ?? "111071",
                    PhoneNumber = NewAdmin.PhoneNumber ?? "",
                    FechaRegistro = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(newUser, NewAdmin.Password);

                if (result.Succeeded)
                {
                    // Asignar rol de Admin
                    await _userManager.AddToRoleAsync(newUser, "Admin");
                    
                    TempData["SuccessMessage"] = $"Administrador '{NewAdmin.UserName}' creado exitosamente.";
                    
                    // Limpiar formulario
                    NewAdmin = new CreateAdminViewModel();
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    TempData["ErrorMessage"] = $"Error al crear administrador: {errors}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al crear administrador: {ex.Message}";
            }

            await LoadUsers();
            return Page();
        }

        public async Task<IActionResult> OnPostToggleUserRoleAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Usuario no encontrado.";
                    return RedirectToPage();
                }

                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                
                if (isAdmin)
                {
                    await _userManager.RemoveFromRoleAsync(user, "Admin");
                    if (!await _userManager.IsInRoleAsync(user, "User"))
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                    }
                    TempData["SuccessMessage"] = $"Se removió el rol de administrador a {user.UserName}.";
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, "Admin");
                    TempData["SuccessMessage"] = $"Se asignó el rol de administrador a {user.UserName}.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al cambiar rol: {ex.Message}";
            }

            return RedirectToPage();
        }

        private async Task LoadUsers()
        {
            Users = _userManager.Users.OrderBy(u => u.UserName).ToList();
            
            Admins = new List<ApplicationUser>();
            foreach (var user in Users)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    Admins.Add(user);
                }
            }
        }
    }

    public class CreateAdminViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PrimerNombre { get; set; } = string.Empty;
        public string? SegundoNombre { get; set; }
        public string PrimerApellido { get; set; } = string.Empty;
        public string? SegundoApellido { get; set; }
        public string TipoDocumento { get; set; } = "CC";
        public string NumeroDocumento { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public string? Ciudad { get; set; }
        public string? Departamento { get; set; }
        public string? CodigoPostal { get; set; }
        public string? PhoneNumber { get; set; }
    }
}