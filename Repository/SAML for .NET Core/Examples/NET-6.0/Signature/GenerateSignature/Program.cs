using ComponentSpace.Saml2;
using ComponentSpace.Saml2.Utility;
using ComponentSpace.Saml2.XmlSecurity;
using ComponentSpace.Saml2.XmlSecurity.Signature;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

/// <summary>
/// Generates XML signatures on SAML v2.0 assertions, messages and metadata.
/// 
/// Usage: dotnet GenerateSignature.dll <fileName> --certificate <certificateFileName> [--password <password>] [--digest <digestAlgorithm>] [--signature <signatureAlgorithm>] 
/// 
/// where the file contains a SAML assertion, message or metadata XML.
/// 
/// XML signatures are generated using the private key associated with the X.509 certificate.
/// </summary>
try
{
    var fileArgument = new Argument<FileInfo>(
        name: "file",
        description: "The XML file containing a SAML assertion, message or metadata.");

    var certificateOption = new Option<FileInfo>(
        name: "--certificate",
        description: "The X.509 certificate file used to generate the signature.")
    {
        IsRequired = true
    };

    var passwordOption = new Option<string>(
        name: "--password",
        description: "The X.509 certificate file password.");

    var digestOption = new Option<string>(
        name: "--digest",
        description: "The digest algorithm.",
        getDefaultValue: () => SamlConstants.DigestAlgorithms.SHA256);

    var signatureOption = new Option<string>(
        name: "--signature",
        description: "The signature algorithm.",
        getDefaultValue: () => SamlConstants.SignatureAlgorithms.RSA_SHA256);

    var rootCommand = new RootCommand("Generate an XML signature on a SAML assertion, message or metadata")
    {
        fileArgument,
        certificateOption,
        passwordOption,
        digestOption,
        signatureOption
    };

    rootCommand.SetHandler((fileInfo, certificateFileInfo, certificatePassword, digestAlgorithm, signatureAlgorithm) =>
    {
        GenerateSignature(fileInfo, certificateFileInfo, certificatePassword, digestAlgorithm, signatureAlgorithm);
    },
    fileArgument, certificateOption, passwordOption, digestOption, signatureOption);

    rootCommand.Invoke(args);
}

catch (Exception exception)
{
    Console.WriteLine(exception.ToString());
}

static void GenerateSignature(FileInfo fileInfo, FileInfo certificateFileInfo, string certificatePassword, string digestAlgorithm, string signatureAlgorithm)
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

    if (string.IsNullOrEmpty(digestAlgorithm))
    {
        digestAlgorithm = SamlConstants.DigestAlgorithms.SHA256;
    }

    if (string.IsNullOrEmpty(signatureAlgorithm))
    {
        signatureAlgorithm = SamlConstants.SignatureAlgorithms.RSA_SHA256;
    }

    var serviceCollection = new ServiceCollection();

    serviceCollection.AddLogging();
    serviceCollection.AddSaml();

    var serviceProvider = serviceCollection.BuildServiceProvider();
    var xmlSignature = serviceProvider.GetRequiredService<IXmlSignature>();

    using (var x509Certificate = new X509Certificate2(certificateFileInfo.FullName, certificatePassword))
    {
        switch (xmlDocument?.DocumentElement?.NamespaceURI)
        {
            case SamlConstants.NamespaceUris.Assertion:
                GenerateAssertionSignature(xmlDocument.DocumentElement, x509Certificate, digestAlgorithm, signatureAlgorithm, xmlSignature);
                break;

            case SamlConstants.NamespaceUris.Protocol:
                GenerateMessageSignature(xmlDocument.DocumentElement, x509Certificate, digestAlgorithm, signatureAlgorithm, xmlSignature);
                break;

            case SamlConstants.NamespaceUris.Metadata:
                GenerateMetadataSignature(xmlDocument.DocumentElement, x509Certificate, digestAlgorithm, signatureAlgorithm, xmlSignature);
                break;

            default:
                throw new ArgumentException($"Unexpected namespace URI: {xmlDocument?.DocumentElement?.NamespaceURI}");
        }
    }

    Console.WriteLine(xmlDocument.OuterXml);
}

static void GenerateAssertionSignature(XmlElement xmlElement, X509Certificate2 x509Certificate, string digestAlgorithm, string signatureAlgorithm, IXmlSignature xmlSignature)
{
    var signatureElement = GenerateXmlSignature(
        xmlElement,
        x509Certificate,
        digestAlgorithm,
        signatureAlgorithm,
        SamlConstants.InclusiveNamespacesPrefixLists.Assertion,
        xmlSignature);

    xmlElement.InsertAfter(signatureElement, xmlElement.FirstChild);
}

static void GenerateMessageSignature(XmlElement xmlElement, X509Certificate2 x509Certificate, string digestAlgorithm, string signatureAlgorithm, IXmlSignature xmlSignature)
{
    var signatureElement = GenerateXmlSignature(
        xmlElement,
        x509Certificate,
        digestAlgorithm,
        signatureAlgorithm,
        SamlConstants.InclusiveNamespacesPrefixLists.Protocol,
        xmlSignature);

    xmlElement.InsertAfter(signatureElement, xmlElement.FirstChild);
}

static void GenerateMetadataSignature(XmlElement xmlElement, X509Certificate2 x509Certificate, string digestAlgorithm, string signatureAlgorithm, IXmlSignature xmlSignature)
{
    var signatureElement = GenerateXmlSignature(
        xmlElement,
        x509Certificate,
        digestAlgorithm,
        signatureAlgorithm,
        SamlConstants.InclusiveNamespacesPrefixLists.Metadata,
        xmlSignature);

    xmlElement.InsertBefore(signatureElement, xmlElement.FirstChild);
}

static XmlElement GenerateXmlSignature(
    XmlElement xmlElement,
    X509Certificate2 x509Certificate,
    string digestAlgorithm,
    string signatureAlgorithm,
    string inclusiveNamespacesPrefixList,
    IXmlSignature xmlSignature)
{
    if (XmlSecurityUtility.IsSigned(xmlElement))
    {
        throw new ArgumentException("The XML is already signed.");
    }

    using var privateKey = x509Certificate.GetPrivateAsymmetricAlgorithm();

    return xmlSignature.Generate(xmlElement, privateKey, digestAlgorithm, signatureAlgorithm, inclusiveNamespacesPrefixList, x509Certificate);
}
