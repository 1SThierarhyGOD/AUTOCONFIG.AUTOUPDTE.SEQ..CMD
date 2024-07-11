using ComponentSpace.Saml2;
using Microsoft.AspNetCore.Mvc;

namespace SamlProxy.Controllers
{
    [Route("saml/idp/[action]")]
    public class IdentityProviderController : Controller
    {
        private readonly ISamlIdentityProvider _samlIdentityProvider;
        private readonly ISamlServiceProvider _samlServiceProvider;
        private readonly IConfiguration _configuration;

        public IdentityProviderController(
            ISamlIdentityProvider samlIdentityProvider,
            ISamlServiceProvider samlServiceProvider,
            IConfiguration configuration)
        {
            _samlIdentityProvider = samlIdentityProvider;
            _samlServiceProvider = samlServiceProvider;
            _configuration = configuration;
        }

        public async Task<IActionResult> SingleSignOnService()
        {
            // Receive an authn request from a service provider (SP-initiated SSO).
            var idpSsoResult = await _samlIdentityProvider.ReceiveSsoAsync();

            // Determine the identity provider name.
            var partnerName = GetIdentityProviderName();

            // Initiate SSO to the identity provider.
            await _samlServiceProvider.InitiateSsoAsync(partnerName, null, idpSsoResult.SsoOptions);

            return new EmptyResult();
        }

        public async Task<IActionResult> SingleLogoutService()
        {
            // Receive a single logout request or response from a service provider.
            // If a request is received then initiate SLO to the identity provider.
            // If a response is received then complete the SP-initiated SLO.
            var sloResult = await _samlIdentityProvider.ReceiveSloAsync();

            if (sloResult.IsResponse)
            {
                if (sloResult.HasCompleted)
                {
                    // SP-initiated SLO has completed.
                    await _samlServiceProvider.SendSloAsync();
                }
            }
            else
            {
                // Determine the identity provider name.
                var partnerName = GetIdentityProviderName();

                // Initiate SLO to the identity provider.
                await _samlServiceProvider.InitiateSloAsync(partnerName, sloResult.LogoutReason, sloResult.RelayState);
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

        private string GetIdentityProviderName()
        {
            // In this example, the identity provider name is retrieved from a query string parameter or the configuration.
            var name = Request.Query["idp"];

            if (string.IsNullOrEmpty(name))
            {
                name = _configuration["PartnerIdentityProviderName"];
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("An identity provider name is required.");
            }

            return name;
        }
    }
}