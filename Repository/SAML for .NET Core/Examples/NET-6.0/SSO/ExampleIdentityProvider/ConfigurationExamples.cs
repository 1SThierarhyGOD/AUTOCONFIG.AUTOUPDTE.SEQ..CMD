using ComponentSpace.Saml2.Configuration;
using ComponentSpace.Saml2.Configuration.Resolver;
using ComponentSpace.Saml2.Exceptions;

namespace ExampleIdentityProvider
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
                    LocalIdentityProviderConfiguration = new LocalIdentityProviderConfiguration()
                    {
                        Name = "https://ExampleIdentityProvider",
                        Description = "Example Identity Provider",
                        SingleSignOnServiceUrl = "https://localhost:44313/SAML/SingleSignOnService",
                        SingleLogoutServiceUrl = "https://localhost:44313/SAML/SingleLogoutService",
                        ArtifactResolutionServiceUrl = "https://localhost:44313/SAML/ArtifactResolutionService",
                        LocalCertificates = new List<Certificate>()
                        {
                            new Certificate()
                            {
                                FileName = "certificates/idp.pfx",
                                Password = "password"
                            }
                        }
                    },
                    PartnerServiceProviderConfigurations = new List<PartnerServiceProviderConfiguration>()
                    {
                        new PartnerServiceProviderConfiguration()
                        {
                            Name = "https://ExampleServiceProvider",
                            Description = "Example Service Provider",
                            AssertionConsumerServiceUrl = "https://localhost:44360/SAML/AssertionConsumerService",
                            SingleLogoutServiceUrl = "https://localhost:44360/SAML/SingleLogoutService",
                            ArtifactResolutionServiceUrl = "https://localhost:44360/SAML/ArtifactResolutionService",
                            PartnerCertificates = new List<Certificate>()
                            {
                                new Certificate()
                                {
                                    FileName = "certificates/sp.cer"
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
        public class ConfigurationResolver : AbstractSamlConfigurationResolver
        {
            public override Task<bool> IsLocalIdentityProviderAsync(string configurationName)
            {
                return Task.FromResult(true);
            }

            public override Task<LocalIdentityProviderConfiguration> GetLocalIdentityProviderConfigurationAsync(string configurationName)
            {
                var localIdentityProviderConfiguration = new LocalIdentityProviderConfiguration()
                {
                    Name = "https://ExampleIdentityProvider",
                    Description = "Example Identity Provider",
                    SingleSignOnServiceUrl = "https://localhost:44313/SAML/SingleSignOnService",
                    SingleLogoutServiceUrl = "https://localhost:44313/SAML/SingleLogoutService",
                    ArtifactResolutionServiceUrl = "https://localhost:44313/SAML/ArtifactResolutionService",
                    LocalCertificates = new List<Certificate>()
                    {
                        new Certificate()
                        {
                            FileName = "certificates/idp.pfx",
                            Password = "password"
                        }
                    }
                };

                return Task.FromResult(localIdentityProviderConfiguration);
            }

            public override Task<PartnerServiceProviderConfiguration> GetPartnerServiceProviderConfigurationAsync(string configurationName, string partnerName)
            {
                if (partnerName != "https://ExampleServiceProvider")
                {
                    throw new SamlConfigurationException($"The partner service provider {partnerName} is not configured.");
                }

                var partnerServiceProviderConfiguration = new PartnerServiceProviderConfiguration()
                {
                    Name = "https://ExampleServiceProvider",
                    Description = "Example Service Provider",
                    AssertionConsumerServiceUrl = "https://localhost:44360/SAML/AssertionConsumerService",
                    SingleLogoutServiceUrl = "https://localhost:44360/SAML/SingleLogoutService",
                    ArtifactResolutionServiceUrl = "https://localhost:44360/SAML/ArtifactResolutionService",
                    PartnerCertificates = new List<Certificate>()
                    {
                        new Certificate()
                        {
                            FileName = "certificates/sp.cer"
                        }
                    }
                };

                return Task.FromResult(partnerServiceProviderConfiguration);
            }

            public override Task<IList<string>> GetPartnerServiceProviderNamesAsync(string configurationName)
            {
                IList<string> partnerServiceProviderNames = new List<string> { "https://ExampleServiceProvider" };

                return Task.FromResult(partnerServiceProviderNames);
            }
        }
    }
}
