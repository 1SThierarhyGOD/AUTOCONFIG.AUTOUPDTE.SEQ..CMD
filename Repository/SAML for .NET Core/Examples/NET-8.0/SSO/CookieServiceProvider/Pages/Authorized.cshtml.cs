using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CookieServiceProvider.Pages
{
    [Authorize]
    public class AuthorizedModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
