using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CookieServiceProvider.Pages
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
            var authenticationProperties = new AuthenticationProperties()
            {
                RedirectUri = "/"
            };

            return new ChallengeResult(authenticationProperties);
        }
    }
}