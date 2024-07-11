using ComponentSpace.Saml2.Utility;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CookieServiceProvider.Pages
{
    public class AboutModel : PageModel
    {
        public AboutModel(ILicense license)
        {
            string licenseType;

            if (license.IsLicensed)
            {
                licenseType = "Licensed";
            }
            else
            {
                licenseType = $"Evaluation (Expires {license.Expires.ToShortDateString()})";
            }

            ProductInformation = $"ComponentSpace.Saml2, Version={license.Version}, {licenseType}";
        }

        public string ProductInformation { get; set; }

        public void OnGet()
        {
        }
    }
}
