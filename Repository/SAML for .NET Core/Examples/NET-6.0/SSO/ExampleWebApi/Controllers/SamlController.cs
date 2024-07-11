using ComponentSpace.Saml2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ExampleWebApi.Controllers
{
    [Route("[controller]/[action]")]
    public class SamlController : Controller
    {
        private readonly ISamlServiceProvider _samlServiceProvider;
        private readonly IConfiguration _configuration;

        private readonly CookieOptions _cookieOptions = new CookieOptions()
        {
            // The cookie must be accessible to the JavaScript app.
            HttpOnly = false,

            // If SameSite is None then the cookie must also be marked as secure.
            Secure = true,

            // A SameSite mode of None is used to support the JavaScript app running under HTTP.
            // Using different schemes (JavaScript app running under HTTP and backend running under HTTPS) is considered cross-site.
            // In a production environment, where both apps are running under HTTPS and within the same domain,
            // Strict is recommended rather than None.
            SameSite = SameSiteMode.None
        };

        public SamlController(
            ISamlServiceProvider samlServiceProvider,
            IConfiguration configuration)
        {
            _samlServiceProvider = samlServiceProvider;
            _configuration = configuration;
        }

        public async Task<IActionResult> InitiateSingleSignOn(string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = "/";
            }

            if (!IsWhitelisted(returnUrl))
            {
                return BadRequest();
            }

            // To login automatically at the service provider, initiate single sign-on to the identity provider (SP-initiated SSO).            
            var partnerName = _configuration["PartnerName"];

            await _samlServiceProvider.InitiateSsoAsync(partnerName, returnUrl);

            return new EmptyResult();
        }

        public async Task<IActionResult> InitiateSingleLogout(string returnUrl)
        {
            if (string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = "/";
            }

            if (!IsWhitelisted(returnUrl))
            {
                return BadRequest();
            }

            Response.Cookies.Delete(_configuration["JWT:CookieName"], _cookieOptions);

            var ssoState = await _samlServiceProvider.GetStatusAsync();

            if (await ssoState.CanSloAsync())
            {
                // Request logout at the identity provider.
                await _samlServiceProvider.InitiateSloAsync(relayState: returnUrl);

                return new EmptyResult();
            }

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return new EmptyResult();
        }

        public async Task<IActionResult> AssertionConsumerService()
        {
            // Receive and process the SAML assertion contained in the SAML response.
            // The SAML response is received either as part of IdP-initiated or SP-initiated SSO.
            var ssoResult = await _samlServiceProvider.ReceiveSsoAsync();

            // Create and save a JWT as a cookie.
            var jwt = new JwtSecurityTokenHandler().WriteToken(CreateJwtSecurityToken(ssoResult));

            Response.Cookies.Append(_configuration["JWT:CookieName"], jwt, _cookieOptions);

            // Redirect to the specified URL.
            if (!string.IsNullOrEmpty(ssoResult.RelayState))
            {
                if (!IsWhitelisted(ssoResult.RelayState))
                {
                    return BadRequest();
                }

                return Redirect(ssoResult.RelayState);
            }

            return new EmptyResult();
        }

        public async Task<IActionResult> SingleLogoutService()
        {
            Response.Cookies.Delete(_configuration["JWT:CookieName"], _cookieOptions);

            // Receive the single logout request or response.
            // If a request is received then single logout is being initiated by the identity provider.
            // If a response is received then this is in response to single logout having been initiated by the service provider.
            var sloResult = await _samlServiceProvider.ReceiveSloAsync();

            if (sloResult.IsResponse)
            {
                // SP-initiated SLO has completed.
                if (!string.IsNullOrEmpty(sloResult.RelayState))
                {
                    if (!IsWhitelisted(sloResult.RelayState))
                    {
                        return BadRequest();
                    }

                    return Redirect(sloResult.RelayState);
                }
            }
            else
            {
                // Respond to the IdP-initiated SLO request indicating successful logout.
                await _samlServiceProvider.SendSloAsync();
            }

            return new EmptyResult();
        }

        private JwtSecurityToken CreateJwtSecurityToken(ISpSsoResult ssoResult)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, ssoResult.UserID)
            };

            if (ssoResult.Attributes != null)
            {
                var samlAttribute = ssoResult.Attributes.SingleOrDefault(a => a.Name == ClaimTypes.Email);

                if (samlAttribute != null)
                {
                    claims.Add(new Claim(JwtRegisteredClaimNames.Email, samlAttribute.ToString()));
                }

                samlAttribute = ssoResult.Attributes.SingleOrDefault(a => a.Name == ClaimTypes.GivenName);

                if (samlAttribute != null)
                {
                    claims.Add(new Claim(JwtRegisteredClaimNames.GivenName, samlAttribute.ToString()));
                }

                samlAttribute = ssoResult.Attributes.SingleOrDefault(a => a.Name == ClaimTypes.Surname);

                if (samlAttribute != null)
                {
                    claims.Add(new Claim(JwtRegisteredClaimNames.FamilyName, samlAttribute.ToString()));
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            return new JwtSecurityToken(
                _configuration["JWT:Issuer"],
                _configuration["JWT:Issuer"],
                claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: credentials);
        }

        private bool IsWhitelisted(string url)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.Relative))
            {
                return true;
            }

            var whitelist = _configuration["JWT:Whitelist"];

            if (string.IsNullOrEmpty(whitelist))
            {
                return true;
            }

            return whitelist.Contains(url);
        }
    }
}
