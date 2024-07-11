using ComponentSpace.Saml2;
using ComponentSpace.Saml2.Assertions;
using ComponentSpace.Saml2.XmlSecurity.Encryption;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

/// <summary>
/// Decrypts SAML v2.0 assertions, attributes and IDs.
/// 
/// Usage: dotnet Decrypt.dll <fileName> --certificate <certificateFileName> [--password <password>]
/// 
/// where the file contains an encrypted SAML assertion, attribute or ID.
/// 
/// SAML assertions, attributes and IDs are decrypted using the private key associated with the X.509 certificate.
/// </summary>
try
{
    var fileArgument = new Argument<FileInfo>(
        name: "file",
        description: "The XML file containing an encrypted SAML assertion, attribute or ID.");

    var certificateOption = new Option<FileInfo>(
        name: "--certificate",
        description: "The X.509 certificate file used to decrypt the assertion, attribute or ID.")
    {
        IsRequired = true
    };

    var passwordOption = new Option<string>(
        name: "--password",
        description: "The X.509 certificate file password.");

    var rootCommand = new RootCommand("Decrypt an encrypted SAML assertion, attribute or ID")
    {
        fileArgument,
        certificateOption,
        passwordOption
    };

    rootCommand.SetHandler((fileInfo, certificateFileInfo, password) =>
    {
        Decrypt(fileInfo, certificateFileInfo, password);
    },
    fileArgument, certificateOption, passwordOption);

    rootCommand.Invoke(args);
}

catch (Exception exception)
{
    Console.WriteLine(exception.ToString());
}

static void Decrypt(FileInfo fileInfo, FileInfo certificateFileInfo, string certificatePassword)
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

    if (!File.Exists(certificateFileInfo.FullName))
    {
        throw new ArgumentException($"The certificate file {certificateFileInfo.FullName} doesn't exist.");
    }

    var serviceCollection = new ServiceCollection();

    serviceCollection.AddLogging();
    serviceCollection.AddSaml();

    var serviceProvider = serviceCollection.BuildServiceProvider();
    var xmlEncryption = serviceProvider.GetRequiredService<IXmlEncryption>();

    if (xmlDocument?.DocumentElement?.NamespaceURI != SamlConstants.NamespaceUris.Assertion)
    {
        throw new ArgumentException($"Unexpected namespace URI: {xmlDocument?.DocumentElement?.NamespaceURI}");
    }

    EncryptedElementType encryptedElement = xmlDocument.DocumentElement.LocalName switch
    {
        ElementNames.EncryptedAssertion => new EncryptedAssertion(xmlDocument.DocumentElement),
        ElementNames.EncryptedAttribute => new EncryptedAttribute(xmlDocument.DocumentElement),
        ElementNames.EncryptedID => new EncryptedID(xmlDocument.DocumentElement),
        ElementNames.NewEncryptedID => new NewEncryptedID(xmlDocument.DocumentElement),
        _ => throw new ArgumentException($"Unexpected element name: {xmlDocument.DocumentElement.LocalName}"),
    };

    using var x509Certificate = new X509Certificate2(certificateFileInfo.FullName, certificatePassword);
    using var privateKey = x509Certificate.GetRSAPrivateKey();

    var plainTextElement = xmlEncryption.Decrypt(
        encryptedElement.EncryptedData,
        encryptedElement.EncryptedKeys,
        privateKey);

    Console.WriteLine(plainTextElement.OwnerDocument.OuterXml);
}

static class ElementNames
{
    public const string EncryptedAssertion = "EncryptedAssertion";
    public const string EncryptedAttribute = "EncryptedAttribute";
    public const string EncryptedID = "EncryptedID";
    public const string NewEncryptedID = "NewEncryptedID";
}
