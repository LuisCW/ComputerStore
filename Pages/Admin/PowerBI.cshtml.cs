using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComputerStore.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class PowerBIModel : PageModel
    {
        [BindProperty]
        public PowerBIConfiguration Config { get; set; } = new PowerBIConfiguration();

        public string Message { get; set; } = string.Empty;
        public bool IsConfigured { get; set; } = false;

        public void OnGet()
        {
            LoadConfiguration();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                Message = "Por favor corrige los errores en el formulario.";
                return Page();
            }

            try
            {
                SaveConfiguration();
                Message = "Configuración de Power BI guardada exitosamente.";
                IsConfigured = true;
            }
            catch (Exception ex)
            {
                Message = $"Error al guardar la configuración: {ex.Message}";
            }

            return Page();
        }

        public IActionResult OnPostTestConnection()
        {
            try
            {
                // Aquí implementarías la lógica para probar la conexión con Power BI
                var isValid = ValidatePowerBIConnection();
                
                if (isValid)
                {
                    Message = "? Conexión con Power BI exitosa.";
                }
                else
                {
                    Message = "? Error al conectar con Power BI. Verifica las credenciales.";
                }
            }
            catch (Exception ex)
            {
                Message = $"? Error de conexión: {ex.Message}";
            }

            return Page();
        }

        private void LoadConfiguration()
        {
            // Aquí cargarías la configuración desde la base de datos o archivo de configuración
            // Por ahora, valores por defecto
            Config = new PowerBIConfiguration
            {
                WorkspaceId = "",
                ReportId = "",
                ClientId = "",
                ClientSecret = "",
                TenantId = "",
                EmbedUrl = "",
                IsEnabled = false
            };

            IsConfigured = !string.IsNullOrEmpty(Config.ReportId);
        }

        private void SaveConfiguration()
        {
            // Aquí guardarías la configuración en la base de datos o archivo de configuración
            // Por ahora, simular guardado exitoso
            Console.WriteLine("Configuración de Power BI guardada:");
            Console.WriteLine($"Workspace ID: {Config.WorkspaceId}");
            Console.WriteLine($"Report ID: {Config.ReportId}");
            Console.WriteLine($"Embed URL: {Config.EmbedUrl}");
        }

        private bool ValidatePowerBIConnection()
        {
            // Aquí implementarías la validación real de Power BI
            // Por ahora, simular validación
            return !string.IsNullOrEmpty(Config.ClientId) && 
                   !string.IsNullOrEmpty(Config.ReportId);
        }
    }

    public class PowerBIConfiguration
    {
        public string WorkspaceId { get; set; } = string.Empty;
        public string ReportId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string TenantId { get; set; } = string.Empty;
        public string EmbedUrl { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = false;
    }
}