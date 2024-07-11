using ComponentSpace.Saml2.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CookieIdentityProvider.Pages
{
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
        {
            // Logout the user locally.
            await HttpContext.SignOutAsync();

            if (returnUrl == null)
            {
                // Pass control to the SAML middleware for IdP-initiated SLO.
                returnUrl = SamlMiddlewareDefaults.InitiateSingleLogoutPath;
            }

            return LocalRedirect(returnUrl);
        }
    }
}
