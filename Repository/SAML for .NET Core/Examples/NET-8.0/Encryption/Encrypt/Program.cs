using ComponentSpace.Saml2;
using ComponentSpace.Saml2.Assertions;
using ComponentSpace.Saml2.XmlSecurity.Encryption;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

/// <summary>
/// Encrypts SAML v2.0 assertions, attributes and IDs.
/// 
/// Usage: dotnet Encrypt.dll <fileName> --certificate <certificateFileName> [--keyalgorithm <keyAlgorithm>] [--dataalgorithm <dataAlgorithm>]
/// 
/// where the file contains a SAML assertion, attribute or ID,
/// the key encryption method defaults to http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p
/// and the data encryption method defaults to "http://www.w3.org/2001/04/xmlenc#aes256-cbc".
/// 
/// SAML assertions attributes and IDs are encrypted using the public key associated with the X.509 certificate.
/// </summary>
try
{
    var fileArgument = new Argument<FileInfo>(
        name: "file",
        description: "The XML file containing a SAML assertion, attribute or ID.");

    var certificateOption = new Option<FileInfo>(
        name: "--certificate",
        description: "The X.509 certificate file used to encrypt the XML.")
    {
        IsRequired = true
    };

    var keyAlgorithmOption = new Option<string>(
        name: "--keyalgorithm",
        description: "The key encryption algorithm.",
        getDefaultValue: () => SamlConstants.KeyEncryptionAlgorithms.RSA_OAEP_MGF1P);

    var dataAlgorithmOption = new Option<string>(
        name: "--dataalgorithm",
        description: "The data encryption algorithm.",
        getDefaultValue: () => SamlConstants.DataEncryptionAlgorithms.AES_256);

    var rootCommand = new RootCommand("Encrypt a SAML assertion, attribute or ID")
    {
        fileArgument,
        certificateOption,
        keyAlgorithmOption,
        dataAlgorithmOption
    };

    rootCommand.SetHandler((fileInfo, certificateFileInfo, keyAlgorithmOption, dataAlgorithmOption) =>
    {
        Encrypt(fileInfo, certificateFileInfo, keyAlgorithmOption, dataAlgorithmOption);
    },
    fileArgument, certificateOption, keyAlgorithmOption, dataAlgorithmOption);

    rootCommand.Invoke(args);
}

catch (Exception exception)
{
    Console.WriteLine(exception.ToString());
}

static void Encrypt(FileInfo fileInfo, FileInfo certificateFileInfo, string keyAlgorithm, string dataAlgorithm)
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

    if (string.IsNullOrEmpty(keyAlgorithm))
    {
        keyAlgorithm = SamlConstants.KeyEncryptionAlgorithms.RSA_OAEP_MGF1P;
    }

    if (string.IsNullOrEmpty(dataAlgorithm))
    {
        dataAlgorithm = SamlConstants.DataEncryptionAlgorithms.AES_256;
    }

    var serviceCollection = new ServiceCollection();

    serviceCollection.AddLogging();
    serviceCollection.AddSaml();

    var serviceProvider = serviceCollection.BuildServiceProvider();
    var xmlEncryption = serviceProvider.GetRequiredService<IXmlEncryption>();

    using var x509Certificate = new X509Certificate2(certificateFileInfo.FullName);
    using var publicKey = x509Certificate.GetRSAPublicKey();

    XmlElement encryptedDataElement = xmlEncryption.Encrypt(
        xmlDocument.DocumentElement,
        publicKey,
        keyAlgorithm,
        dataAlgorithm,
        x509Certificate);

    if (xmlDocument?.DocumentElement?.NamespaceURI != SamlConstants.NamespaceUris.Assertion)
    {
        throw new ArgumentException($"Unexpected namespace URI: {xmlDocument?.DocumentElement?.NamespaceURI}");
    }

    var encryptedXmlDocument = new XmlDocument()
    {
        PreserveWhitespace = true
    };

    XmlElement encryptedElement;

    switch (xmlDocument.DocumentElement.LocalName)
    {
        case ElementNames.Assertion:
            var encryptedAssertion = new EncryptedAssertion()
            {
                EncryptedData = encryptedDataElement
            };

            encryptedElement = encryptedAssertion.ToXml(encryptedXmlDocument);
            break;

        case ElementNames.Attribute:
            var encryptedAttribute = new EncryptedAttribute()
            {
                EncryptedData = encryptedDataElement
            };

            encryptedElement = encryptedAttribute.ToXml(encryptedXmlDocument);
            break;

        case ElementNames.NameID:
            var encryptedID = new EncryptedID()
            {
                EncryptedData = encryptedDataElement
            };

            encryptedElement = encryptedID.ToXml(encryptedXmlDocument);
            break;

        case ElementNames.NewID:
            var newEncryptedID = new NewEncryptedID()
            {
                EncryptedData = encryptedDataElement
            };

            encryptedElement = newEncryptedID.ToXml(encryptedXmlDocument);
            break;

        default:
            throw new ArgumentException($"Unexpected element name: {xmlDocument.DocumentElement.LocalName}");
    }

    encryptedXmlDocument.AppendChild(encryptedElement);

    Console.WriteLine(encryptedElement.OwnerDocument.OuterXml);
}

static class ElementNames
{
    public const string Assertion = "Assertion";
    public const string Attribute = "Attribute";
    public const string NameID = "NameID";
    public const string NewID = "NewID";
}
