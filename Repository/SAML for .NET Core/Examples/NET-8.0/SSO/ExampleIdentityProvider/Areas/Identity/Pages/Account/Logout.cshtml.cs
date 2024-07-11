// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using ComponentSpace.Saml2;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ExampleIdentityProvider.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;
        private readonly ISamlIdentityProvider _samlIdentityProvider;

        public LogoutModel(SignInManager<IdentityUser> signInManager, ILogger<LogoutModel> logger, ISamlIdentityProvider samlIdentityProvider)
        {
            _signInManager = signInManager;
            _logger = logger;
            _samlIdentityProvider = samlIdentityProvider;
        }

        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");

            var ssoState = await _samlIdentityProvider.GetStatusAsync();

            if (await ssoState.CanSloAsync())
            {
                // Initiate SAML logout.
                return RedirectToAction("InitiateSingleLogout", "Saml");
            }

            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // This needs to be a redirect so that the browser performs a new
                // request and the identity for the user gets updated.
                return RedirectToPage();
            }
        }
    }
}
