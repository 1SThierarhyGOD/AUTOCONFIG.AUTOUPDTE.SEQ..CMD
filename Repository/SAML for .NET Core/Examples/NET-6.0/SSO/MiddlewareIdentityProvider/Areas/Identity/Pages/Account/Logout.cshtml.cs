// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using ComponentSpace.Saml2.Middleware;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace MiddlewareIdentityProvider.Areas.Identity.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(SignInManager<IdentityUser> signInManager, ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnGet(string returnUrl = null)
        {
            // If a redirect URL is included, this was invoked by the SAML middleware as part of SP-initiated SLO.
            if (returnUrl != null)
            {
                // Logout the user locally.
                await _signInManager.SignOutAsync();
                _logger.LogInformation("User logged out.");

                // Return control to the SAML middleware.
                return LocalRedirect(returnUrl);
            }

            return Page();
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            // Logout the user locally.
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");

            // Pass control to the SAML middleware for IdP-initiated SLO.
            var url = SamlMiddlewareDefaults.InitiateSingleLogoutPath;

            if (returnUrl != null)
            {
                url = QueryHelpers.AddQueryString(
                    SamlMiddlewareDefaults.InitiateSingleLogoutPath,
                    SamlMiddlewareDefaults.ReturnUrlParameter,
                    returnUrl);
            }

            if (!string.IsNullOrEmpty(Request.PathBase))
            {
                var stringBuilder = new StringBuilder();

                stringBuilder.Append(Request.PathBase);
                stringBuilder.Append(url);

                url = stringBuilder.ToString();
            }

            return LocalRedirect(url);
        }
    }
}
