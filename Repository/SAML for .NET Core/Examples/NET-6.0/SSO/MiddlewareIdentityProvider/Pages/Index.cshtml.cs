using ComponentSpace.Saml2.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;

namespace MiddlewareIdentityProvider.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }

        public IActionResult OnGetInitiateSingleSignOn()
        {
            // Pass control to the SAML middleware for IdP-initiated SSO.
            var url = SamlMiddlewareDefaults.InitiateSingleSignOnPath;

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