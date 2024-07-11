using ComponentSpace.Saml2;
using ComponentSpace.Saml2.Assertions;
using ComponentSpace.Saml2.Metadata.Export;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Xml;

namespace DatabaseIdentityProvider.Controllers
{
    [Route("[controller]/[action]")]
    public class SamlController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ISamlIdentityProvider _samlIdentityProvider;
        private readonly IConfigurationToMetadata _configurationToMetadata;
        private readonly IConfiguration _configuration;

        public SamlController(
            SignInManager<IdentityUser> signInManager,
            ISamlIdentityProvider samlIdentityProvider,
            IConfigurationToMetadata configurationToMetadata,
            IConfiguration configuration)
        {
            _signInManager = signInManager;
            _samlIdentityProvider = samlIdentityProvider;
            _configurationToMetadata = configurationToMetadata;
            _configuration = configuration;
        }

        [Authorize]
        public async Task<IActionResult> InitiateSingleSignOn()
        {
            // Get the name of the logged in user.
            var userName = User?.Identity?.Name;

            // For demonstration purposes, include some claims.
            var attributes = new List<SamlAttribute>()
            {
                new SamlAttribute(ClaimTypes.Email, User?.FindFirst(ClaimTypes.Email)?.Value),
                new SamlAttribute(ClaimTypes.GivenName, User?.FindFirst(ClaimTypes.GivenName)?.Value),
                new SamlAttribute(ClaimTypes.Surname, User?.FindFirst(ClaimTypes.Surname)?.Value),
            };

            var partnerName = _configuration["PartnerName"];
            var relayState = _configuration["RelayState"];

            // Initiate single sign-on to the service provider (IdP-initiated SSO)
            // by sending a SAML response containing a SAML assertion to the SP.
            // The optional relay state normally specifies the target URL once SSO completes.
            await _samlIdentityProvider.InitiateSsoAsync(partnerName, userName, attributes, relayState);

            return new EmptyResult();
        }

        public async Task<IActionResult> InitiateSingleLogout(string? returnUrl = null)
        {
            // Request logout at the service provider(s).
            await _samlIdentityProvider.InitiateSloAsync(relayState: returnUrl);

            return new EmptyResult();
        }

        public async Task<IActionResult> SingleSignOnService()
        {
            // Receive the authn request from the service provider (SP-initiated SSO).
            await _samlIdentityProvider.ReceiveSsoAsync();

            // If the user is logged in at the identity provider, complete SSO immediately.
            // Otherwise have the user login before completing SSO.
            if (User.Identity is not null && User.Identity.IsAuthenticated)
            {
                await CompleteSsoAsync();

                return new EmptyResult();
            }
            else
            {
                return RedirectToAction("SingleSignOnServiceCompletion");
            }
        }

        [Authorize]
        public async Task<IActionResult> SingleSignOnServiceCompletion()
        {
            await CompleteSsoAsync();

            return new EmptyResult();
        }

        public async Task<IActionResult> SingleLogoutService()
        {
            // Receive the single logout request or response.
            // If a request is received then single logout is being initiated by a partner service provider.
            // If a response is received then this is in response to single logout having been initiated by the identity provider.
            var sloResult = await _samlIdentityProvider.ReceiveSloAsync();

            if (sloResult.IsResponse)
            {
                if (sloResult.HasCompleted)
                {
                    // IdP-initiated SLO has completed.
                    if (!string.IsNullOrEmpty(sloResult.RelayState))
                    {
                        return LocalRedirect(sloResult.RelayState);
                    }

                    return RedirectToPage("/Index");
                }
            }
            else
            {
                // Logout locally.
                await _signInManager.SignOutAsync();

                // Respond to the SP-initiated SLO request indicating successful logout.
                await _samlIdentityProvider.SendSloAsync();
            }

            return new EmptyResult();
        }

        public async Task<IActionResult> ArtifactResolutionService()
        {
            // Resolve the HTTP artifact.
            // This is only required if supporting the HTTP-Artifact binding.
            await _samlIdentityProvider.ResolveArtifactAsync();

            return new EmptyResult();
        }

        public async Task<IActionResult> ExportMetadata()
        {
            var entityDescriptor = await _configurationToMetadata.ExportAsync();
            var xmlElement = entityDescriptor.ToXml();

            Response.ContentType = "text/xml";
            Response.Headers.Add("Content-Disposition", "attachment; filename=\"metadata.xml\"");

            var xmlWriterSettings = new XmlWriterSettings()
            {
                Async = true,
                Encoding = Encoding.UTF8,
                Indent = true,
                OmitXmlDeclaration = true
            };

            using (var xmlWriter = XmlWriter.Create(Response.Body, xmlWriterSettings))
            {
                xmlElement.WriteTo(xmlWriter);
                await xmlWriter.FlushAsync();
            }

            return new EmptyResult();
        }

        private Task CompleteSsoAsync()
        {
            // Get the name of the logged in user.
            var userName = User?.Identity?.Name;

            // For demonstration purposes, include some claims.
            var attributes = new List<SamlAttribute>()
            {
                new SamlAttribute(ClaimTypes.Email, User?.FindFirst(ClaimTypes.Email)?.Value),
                new SamlAttribute(ClaimTypes.GivenName, User?.FindFirst(ClaimTypes.GivenName)?.Value),
                new SamlAttribute(ClaimTypes.Surname, User?.FindFirst(ClaimTypes.Surname)?.Value),
            };

            // The user is logged in at the identity provider.
            // Respond to the authn request by sending a SAML response containing a SAML assertion to the SP.
            return _samlIdentityProvider.SendSsoAsync(userName, attributes);
        }
    }
}
