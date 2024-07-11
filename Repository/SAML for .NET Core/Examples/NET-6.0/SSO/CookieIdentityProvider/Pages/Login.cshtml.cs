using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace CookieIdentityProvider.Pages
{
    // Disable the antiforgery token so simultaneous login and SSO from multiple browser tabs may be tested.
    // This should be left enabled in production environments.
    [IgnoreAntiforgeryToken]
    public class LoginModel : PageModel
    {
        [BindProperty]
        [Required]
        public string? Username { get; set; }

        [BindProperty]
        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= "/";

            if (ModelState.IsValid && Username != null)
            {
                // Any user name and password is acceptable for this demonstration.
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, Username),
                    new Claim(ClaimTypes.Name, Username),
                };

                var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                return LocalRedirect(returnUrl);
            }

            return Page();
        }
    }
}
