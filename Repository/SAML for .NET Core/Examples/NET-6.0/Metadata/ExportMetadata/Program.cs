using ComponentSpace.Saml2.Configuration.Serialization;
using ComponentSpace.Saml2.Metadata;
using ComponentSpace.Saml2.Metadata.Export;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Xml;

/// <summary>
/// Exports the SAML configuration as SAML metadata.
/// 
/// Usage: dotnet ExportMetadata.dll
/// </summary>
try
{
    var (fileName, jsonPath, configurationName, partnerName) = GetConfigurationParameters();

    // All certificate file paths are relative to the SAML configuration file directory.
    var workingDirectory = Path.GetDirectoryName(Path.GetFullPath(fileName));

    var serviceCollection = new ServiceCollection();
    var configuration = new ConfigurationBuilder().Build();

    serviceCollection.AddLogging();
    serviceCollection.AddSaml(samlConfigurations =>
    {
        samlConfigurations.Configurations = ConfigurationDeserializer.Deserialize(fileName, jsonPath).Configurations;
    });

    serviceCollection.AddSingleton<IConfiguration>(configuration);

    var serviceProvider = serviceCollection.BuildServiceProvider();
    var configurationToMetadata = serviceProvider.GetRequiredService<IConfigurationToMetadata>();

    var entityDescriptor = await configurationToMetadata.ExportAsync(configurationName, partnerName, workingDirectory);

    SaveMetadata(entityDescriptor);
}

catch (Exception exception)
{
    Console.WriteLine(exception.ToString());
}

static (string fileName, string jsonPath, string? configurationName, string? partnerName) GetConfigurationParameters()
{
    Console.Write("SAML configuration file to export [appsettings.json]: ");

    var fileName = Console.ReadLine();

    if (string.IsNullOrEmpty(fileName))
    {
        fileName = "appsettings.json";
    }

    Console.Write("Configuration JSON path [SAML]: ");

    var jsonPath = Console.ReadLine();

    if (string.IsNullOrEmpty(jsonPath))
    {
        jsonPath = "SAML";
    }

    Console.Write("SAML configuration name [none]: ");

    var configurationName = Console.ReadLine();

    Console.Write("Partner name [none]: ");

    var partnerName = Console.ReadLine();

    return (fileName, jsonPath, configurationName, partnerName);
}

static void SaveMetadata(EntityDescriptor entityDescriptor)
{
    Console.Write("SAML metadata file [saml-metadata.xml]: ");

    var fileName = Console.ReadLine();

    if (string.IsNullOrEmpty(fileName))
    {
        fileName = "saml-metadata.xml";
    }

    using XmlTextWriter xmlTextWriter = new XmlTextWriter(fileName, null)
    {
        Formatting = Formatting.Indented
    };

    entityDescriptor.ToXml().OwnerDocument.Save(xmlTextWriter);
}
