using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FunApp.Pages.Admin
{
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
            // Questions are loaded dynamically via SignalR in the client script.
        }
    }
}
