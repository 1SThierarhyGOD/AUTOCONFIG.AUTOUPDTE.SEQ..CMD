using ComponentSpace.Saml2.Configuration;
using ComponentSpace.Saml2.Configuration.Database;
using ComponentSpace.Saml2.Utility;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DatabaseServiceProvider.Pages
{
    public class AboutModel : PageModel
    {
        private readonly SamlConfigurationContext samlConfigurationContext;
        private readonly ILicense license;

        public AboutModel(SamlConfigurationContext samlConfigurationContext, ILicense license)
        {
            this.samlConfigurationContext = samlConfigurationContext;
            this.license = license;
        }

        public bool DatabaseSeeded { get; set; }

        public string? ProductInformation { get; set; }

        public void OnGet()
        {
            DatabaseSeeded = samlConfigurationContext.SamlConfigurations.Count() > 0;

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

        public void OnPost()
        {
            if (samlConfigurationContext.SamlConfigurations.Count() > 0)
            {
                return;
            }

            var samlConfiguration = new SamlConfiguration()
            {
                LocalServiceProviderConfiguration = new LocalServiceProviderConfiguration()
                {
                    Name = "https://DatabaseServiceProvider",
                    Description = "Database Service Provider",
                    AssertionConsumerServiceUrl = "https://localhost:44304/SAML/AssertionConsumerService",
                    SingleLogoutServiceUrl = "https://localhost:44304/SAML/SingleLogoutService",
                    ArtifactResolutionServiceUrl = "https://localhost:44304/SAML/ArtifactResolutionService",
                    LocalCertificates = new List<Certificate>()
                    {
                        new Certificate()
                        {
                            FileName = "certificates/sp.pfx",
                            Password = "password"
                        }
                    }
                },
                PartnerIdentityProviderConfigurations = new List<PartnerIdentityProviderConfiguration>()
                {
                    new PartnerIdentityProviderConfiguration()
                    {
                        Name = "https://DatabaseIdentityProvider",
                        Description = "Database Identity Provider",
                        SingleSignOnServiceUrl = "https://localhost:44306/SAML/SingleSignOnService",
                        SingleLogoutServiceUrl = "https://localhost:44306/SAML/SingleLogoutService",
                        ArtifactResolutionServiceUrl = "https://localhost:44306/SAML/ArtifactResolutionService",
                        PartnerCertificates = new List<Certificate>()
                        {
                            new Certificate()
                            {
                                FileName = "certificates/idp.cer"
                            }
                        }
                    }
                }
            };

            samlConfigurationContext.SamlConfigurations.Add(samlConfiguration);
            samlConfigurationContext.SaveChanges();
            DatabaseSeeded = true;
        }
    }
}
