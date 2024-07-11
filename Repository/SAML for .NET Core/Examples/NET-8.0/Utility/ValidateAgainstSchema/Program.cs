using ComponentSpace.Saml2.Utility;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.Xml;

/// <summary>
/// Validates SAML message, assertion and metadata XML against the SAML XML schemas.
/// 
/// Usage: dotnet ValidateAgainstSchema.dll <fileName> [--extension <extensionSchema>]
/// 
/// where the file contains a SAML message, assertion or metadata,
/// and optional extension XML schemas may be specified.
/// </summary>
try
{
    var fileArgument = new Argument<FileInfo>(
        name: "file",
        description: "The XML file containing a SAML message, assertion, or metadata.");

    var extensionOption = new Option<IList<FileInfo>>(     
        name: "--extension",
        description: "An optional extension XML schema.");

    extensionOption.Arity = ArgumentArity.OneOrMore;

    var rootCommand = new RootCommand("Validates a SAML message, assertion or metadata XML against the SAML XML schemas")
    {
        fileArgument,
        extensionOption
    };

    rootCommand.SetHandler(Validate, fileArgument, extensionOption);

    rootCommand.Invoke(args);
}

catch (Exception exception)
{
    Console.WriteLine(exception.ToString());
}

static void Validate(FileInfo fileInfo, IList<FileInfo> extensionFileInfos)
{
    if (!File.Exists(fileInfo.FullName))
    {
        throw new ArgumentException($"The file {fileInfo.FullName} doesn't exist.");
    }

    var xmlDocument = new XmlDocument
    {
        PreserveWhitespace = true
    };

    xmlDocument.Load(fileInfo.FullName);

    var serviceCollection = new ServiceCollection();

    serviceCollection.AddLogging();
    serviceCollection.AddSaml();

    var extensionSchemaFileNames = new List<string>();

    foreach (var extensionFileInfo in extensionFileInfos)
    {
        extensionSchemaFileNames.Add(extensionFileInfo.FullName);
    }

    serviceCollection.Configure<SamlSchemaValidatorOptions>(options => options.ExtensionSchemaFileNames = extensionSchemaFileNames);

    var serviceProvider = serviceCollection.BuildServiceProvider();
    var samlSchemaValidator = serviceProvider.GetRequiredService<ISamlSchemaValidator>();

    var validated = samlSchemaValidator.Validate(xmlDocument.DocumentElement);

    Console.WriteLine($"Validated: {validated}");

    if (!validated)
    {
        foreach (var error in samlSchemaValidator.Errors)
        {
            Console.WriteLine(error);
        }

        foreach (var warning in samlSchemaValidator.Warnings)
        {
            Console.WriteLine(warning);
        }
    }
}
