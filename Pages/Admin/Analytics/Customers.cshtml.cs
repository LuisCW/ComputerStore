using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComputerStore.Pages.Admin.Analytics
{
    [Authorize(Roles = "Admin")]
    public class CustomersModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
