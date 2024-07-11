using ComponentSpace.Saml2.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExampleWebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SamlLicenseController : ControllerBase
    {
        public class SamlLicenseInformation
        {
            public string? Name { get; set; }
            public string? Version { get; set; }
            public bool IsLicensed { get; set; }
            public DateTime Expires { get; set; }
            public string? DisplayMessage { get; set; }
        }

        private readonly ILicense _license;

        public SamlLicenseController(ILicense license)
        {
            _license = license;
        }

        [HttpGet]
        public ActionResult<SamlLicenseInformation> Get()
        {
            return new SamlLicenseInformation()
            {
                Name = _license.Name,
                Version = _license.Version.ToString(),
                IsLicensed = _license.IsLicensed,
                Expires = _license.Expires,
                DisplayMessage = GetDisplayMessage()
            };
        }

        private string GetDisplayMessage()
        {
            string licenseType;

            if (_license.IsLicensed)
            {
                licenseType = "Licensed";
            }
            else
            {
                licenseType = $"Evaluation (Expires {_license.Expires.ToShortDateString()})";
            }

            return $"ComponentSpace.Saml2, Version={_license.Version}, {licenseType}";
        }
    }
}
