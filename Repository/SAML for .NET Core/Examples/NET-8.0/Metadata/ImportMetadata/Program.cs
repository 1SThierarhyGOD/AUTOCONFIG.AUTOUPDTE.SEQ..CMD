using ComponentSpace.Saml2.Configuration;
using ComponentSpace.Saml2.Configuration.Serialization;
using ComponentSpace.Saml2.Metadata.Import;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Imports SAML metadata into the SAML configuration.
/// 
/// Usage: dotnet ImportMetadata.dll
/// </summary>
try
{
    Console.Write("SAML metadata file or URL to import: ");
    var metadataLocation = Console.ReadLine();

    if (string.IsNullOrEmpty(metadataLocation))
    {
        throw new ArgumentException("A metadata URL or file must be specified.");
    }

    var serviceCollection = new ServiceCollection();

    serviceCollection.AddLogging();

    // By default, certificates are imported as files. To import them as base-64 encoded strings, use the CertificateStringImporter.
    //serviceCollection.AddTransient<ICertificateImporter, CertificateStringImporter>();

    serviceCollection.AddSaml();

    var serviceProvider = serviceCollection.BuildServiceProvider();
    var metadataToConfiguration = serviceProvider.GetRequiredService<IMetadataToConfiguration>();

    var samlConfiguration = await metadataToConfiguration.ImportAsync(metadataLocation);

    SaveConfiguration(samlConfiguration);
}

catch (Exception exception)
{
    Console.WriteLine(exception.ToString());
}

static void SaveConfiguration(SamlConfiguration samlConfiguration)
{
    Console.Write("SAML configuration file [saml.json]: ");

    var fileName = Console.ReadLine();

    if (string.IsNullOrEmpty(fileName))
    {
        fileName = "saml.json";
    }

    File.WriteAllText(fileName, ConfigurationSerializer.Serialize(samlConfiguration));
}
