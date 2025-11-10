using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ComputerStore.Pages.Admin.Analytics
{
    [Authorize(Roles = "Admin")]
    public class TrafficModel : PageModel
    {
        public void OnGet() { }
    }
}
