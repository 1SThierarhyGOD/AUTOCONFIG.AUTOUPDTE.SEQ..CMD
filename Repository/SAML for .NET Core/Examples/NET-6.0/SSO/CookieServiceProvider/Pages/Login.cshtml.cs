using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CookieServiceProvider.Pages
{
    public class LoginModel : PageModel
    {
        public IActionResult OnGet(string? returnUrl = null)
        {
            returnUrl ??= "/";

            var authenticationProperties = new AuthenticationProperties()
            {
                RedirectUri = returnUrl
            };

            return new ChallengeResult(authenticationProperties);
        }
    }
}
