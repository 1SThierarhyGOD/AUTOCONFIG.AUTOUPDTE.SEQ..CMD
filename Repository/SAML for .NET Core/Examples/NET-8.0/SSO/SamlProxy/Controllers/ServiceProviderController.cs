using ComponentSpace.Saml2;
using Microsoft.AspNetCore.Mvc;

namespace SamlProxy.Controllers
{
    [Route("saml/sp/[action]")]
    public class ServiceProviderController : Controller
    {
        private readonly ISamlIdentityProvider _samlIdentityProvider;
        private readonly ISamlServiceProvider _samlServiceProvider;
        private readonly IConfiguration _configuration;

        public ServiceProviderController(
            ISamlIdentityProvider samlIdentityProvider,
            ISamlServiceProvider samlServiceProvider,
            IConfiguration configuration)
        {
            _samlIdentityProvider = samlIdentityProvider;
            _samlServiceProvider = samlServiceProvider;
            _configuration = configuration;
        }

        public async Task<IActionResult> AssertionConsumerService()
        {
            // Receive a SAML response from an identity provider either as part of IdP-initiated or SP-initiated SSO.
            var ssoResult = await _samlServiceProvider.ReceiveSsoAsync();

            if (ssoResult.IsInResponseTo)
            {
                // Complete SP-initiated SSO to the service provider.
                await _samlIdentityProvider.SendSsoAsync(ssoResult.UserID, ssoResult.Attributes, ssoResult.AuthnContext);
            }
            else
            {
                // Determine the service provider name.
                var partnerName = GetServiceProviderName();

                // Initiate SSO to the service provider.
                await _samlIdentityProvider.InitiateSsoAsync(partnerName, ssoResult.UserID, ssoResult.Attributes, ssoResult.RelayState, ssoResult.AuthnContext);
            }

            return new EmptyResult();
        }

        public async Task<IActionResult> SingleLogoutService()
        {
            // Receive a single logout request or response from an identity provider.
            // If a request is received then initiate SLO to the identity provider.
            // If a response is received then complete the SP-initiated SLO.
            var sloResult = await _samlServiceProvider.ReceiveSloAsync();

            if (sloResult.IsResponse)
            {
                // Respond to the SP-initiated SLO request indicating successful logout.
                await _samlIdentityProvider.SendSloAsync();
            }
            else
            {
                // Request logout at the service provider(s).
                await _samlIdentityProvider.InitiateSloAsync(sloResult.LogoutReason, sloResult.RelayState);
            }

            return new EmptyResult();
        }

        public async Task<IActionResult> ArtifactResolutionService()
        {
            // Resolve the HTTP artifact.
            // This is only required if supporting the HTTP-Artifact binding.
            await _samlServiceProvider.ResolveArtifactAsync();

            return new EmptyResult();
        }

        private string GetServiceProviderName()
        {
            // In this example, the service provider name is retrieved from a query string parameter or the configuration.
            var name = Request.Query["sp"].ToString();

            if (string.IsNullOrEmpty(name))
            {
                name = _configuration["PartnerServiceProviderName"];
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("A service provider name is required.");
            }

            return name;
        }
    }
}