using ComponentSpace.Saml2.Configuration;
using ComponentSpace.Saml2.Configuration.Resolver;
using ComponentSpace.Saml2.Exceptions;

namespace ExampleServiceProvider
{
    public static class ConfigurationExamples
    {
        // This method demonstrates loading SAML configuration programmatically 
        // rather than through appsettings.json or another JSON configuration file.
        // This is useful if configuration is stored in a custom database, for example.
        // The SAML configuration is registered by calling:
        // builder.Services.AddSaml(config => ConfigurationExamples.ConfigureSaml(config));
        public static void ConfigureSaml(SamlConfigurations samlConfigurations)
        {
            samlConfigurations.Configurations = new List<SamlConfiguration>()
            {
                new SamlConfiguration()
                {
                    LocalServiceProviderConfiguration = new LocalServiceProviderConfiguration()
                    {
                        Name = "https://ExampleServiceProvider",
                        Description = "Example Service Provider",
                        AssertionConsumerServiceUrl = "https://localhost:44360/SAML/AssertionConsumerService",
                        SingleLogoutServiceUrl = "https://localhost:44360/SAML/SingleLogoutService",
                        ArtifactResolutionServiceUrl = "https://localhost:44360/SAML/ArtifactResolutionService",
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
                            Name = "https://ExampleIdentityProvider",
                            Description = "Example Identity Provider",
                            SingleSignOnServiceUrl = "https://localhost:44313/SAML/SingleSignOnService",
                            SingleLogoutServiceUrl = "https://localhost:44313/SAML/SingleLogoutService",
                            ArtifactResolutionServiceUrl = "https://localhost:44313/SAML/ArtifactResolutionService",
                            PartnerCertificates = new List<Certificate>()
                            {
                                new Certificate()
                                {
                                    FileName = "certificates/idp.cer"
                                }
                            }
                        }
                    }
                }
            };
        }

        // This class demonstrates loading SAML configuration dynamically using a custom configuration resolver.
        // Hard-coded configuration is returned in this example but more typically configuration would be read from a custom database.
        // The configurationName parameter specifies the SAML configuration in a multi-tenancy application but is not used in this example.
        // The custom configuration resolver is registered by calling:
        // builder.Services.AddSaml().AddConfigurationResolver<ConfigurationExamples.CustomConfigurationResolver>();
        // Alternatively, it can be cached by calling:
        // builder.Services.AddSaml().AddCachedConfigurationResolver<ConfigurationExamples.CustomConfigurationResolver>();
        public class CustomConfigurationResolver : AbstractSamlConfigurationResolver
        {
            public override Task<bool> IsLocalServiceProviderAsync(string configurationName)
            {
                return Task.FromResult(true);
            }

            public override Task<LocalServiceProviderConfiguration> GetLocalServiceProviderConfigurationAsync(string configurationName)
            {
                var localServiceProviderConfiguration = new LocalServiceProviderConfiguration()
                {
                    Name = "https://ExampleServiceProvider",
                    Description = "Example Service Provider",
                    AssertionConsumerServiceUrl = "https://localhost:44360/SAML/AssertionConsumerService",
                    SingleLogoutServiceUrl = "https://localhost:44360/SAML/SingleLogoutService",
                    ArtifactResolutionServiceUrl = "https://localhost:44360/SAML/ArtifactResolutionService",
                    LocalCertificates = new List<Certificate>()
                    {
                        new Certificate()
                        {
                            FileName = "certificates/sp.pfx",
                            Password = "password"
                        }
                    }
                };

                return Task.FromResult(localServiceProviderConfiguration);
            }

            public override Task<PartnerIdentityProviderConfiguration> GetPartnerIdentityProviderConfigurationAsync(string configurationName, string partnerName)
            {
                if (partnerName != "https://ExampleIdentityProvider")
                {
                    throw new SamlConfigurationException($"The partner identity provider {partnerName} is not configured.");
                }

                var partnerIdentityProviderConfiguration = new PartnerIdentityProviderConfiguration()
                {
                    Name = "https://ExampleIdentityProvider",
                    Description = "Example Identity Provider",
                    SingleSignOnServiceUrl = "https://localhost:44313/SAML/SingleSignOnService",
                    SingleLogoutServiceUrl = "https://localhost:44313/SAML/SingleLogoutService",
                    ArtifactResolutionServiceUrl = "https://localhost:44313/SAML/ArtifactResolutionService",
                    PartnerCertificates = new List<Certificate>()
                    {
                        new Certificate()
                        {
                            FileName = "certificates/idp.cer"
                        }
                    }
                };

                return Task.FromResult(partnerIdentityProviderConfiguration);
            }

            public override Task<IList<string>> GetPartnerIdentityProviderNamesAsync(string configurationName)
            {
                IList<string> partnerIdentityProviderNames = new List<string> { "https://ExampleIdentityProvider" };

                return Task.FromResult(partnerIdentityProviderNames);
            }
        }
    }
}
