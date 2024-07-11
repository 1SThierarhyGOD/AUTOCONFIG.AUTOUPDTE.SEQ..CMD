using ComponentSpace.Saml2;
using ComponentSpace.Saml2.Protocols;
using ComponentSpace.Saml2.Utility;
using ComponentSpace.Saml2.XmlSecurity;
using ComponentSpace.Saml2.XmlSecurity.Signature;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

/// <summary>
/// Verifies XML signatures on SAML v2.0 assertions, messages and metadata.
/// 
/// Usage: dotnet VerifySignature.dll <fileName> [--certificate <certificateFileName>]
/// 
/// where the file contains a SAML assertion, message or metadata XML.
/// 
/// XML signatures are verified using the public key associated with the X.509 certificate.
/// </summary>
try
{
    var fileArgument = new Argument<FileInfo>(
        name: "file",
        description: "The XML file containing a signed SAML assertion, message or metadata.");

    var certificateOption = new Option<FileInfo>(
        name: "--certificate",
        description: "The X.509 certificate file used to verify the signature.");

    var rootCommand = new RootCommand("Verify an XML signature on a SAML assertion, message or metadata")
    {
        fileArgument,
        certificateOption
    };

    rootCommand.SetHandler((fileInfo, certificateFileInfo) =>
    {
        VerifySignature(fileInfo, certificateFileInfo);
    },
    fileArgument, certificateOption);

    rootCommand.Invoke(args);
}

catch (Exception exception)
{
    Console.WriteLine(exception.ToString());
}

static void VerifySignature(FileInfo fileInfo, FileInfo certificateFileInfo)
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

    var serviceProvider = serviceCollection.BuildServiceProvider();
    var xmlSignature = serviceProvider.GetRequiredService<IXmlSignature>();

    using var x509Certificate = LoadOptionalCertificate(certificateFileInfo);

    switch (xmlDocument?.DocumentElement?.NamespaceURI)
    {
        case SamlConstants.NamespaceUris.Assertion:
            VerifyAssertionSignature(xmlDocument.DocumentElement, x509Certificate, xmlSignature);
            break;

        case SamlConstants.NamespaceUris.Protocol:
            VerifyMessageSignature(xmlDocument.DocumentElement, x509Certificate, xmlSignature);
            break;

        case SamlConstants.NamespaceUris.Metadata:
            VerifyMetadataSignature(xmlDocument.DocumentElement, x509Certificate, xmlSignature);
            break;

        default:
            throw new ArgumentException($"Unexpected namespace URI: {xmlDocument?.DocumentElement?.NamespaceURI}");
    }
}

static X509Certificate2? LoadOptionalCertificate(FileInfo certificateFileInfo)
{
    if (certificateFileInfo is null)
    {
        return null;
    }

    if (!File.Exists(certificateFileInfo.FullName))
    {
        throw new ArgumentException($"The certificate file {certificateFileInfo.FullName} doesn't exist.");
    }

    return new X509Certificate2(certificateFileInfo.FullName);
}

static void VerifyAssertionSignature(XmlElement xmlElement, X509Certificate2? x509Certificate, IXmlSignature xmlSignature)
{
    Console.WriteLine("Verifying the SAML assertion signature.");
    VerifyXmlSignature(xmlElement, x509Certificate, xmlSignature);
}

static void VerifyMessageSignature(XmlElement xmlElement, X509Certificate2? x509Certificate, IXmlSignature xmlSignature)
{
    Console.WriteLine("Verifying the SAML message signature.");
    VerifyXmlSignature(xmlElement, x509Certificate, xmlSignature);

    if (SamlResponse.IsValid(xmlElement))
    {
        var samlResponse = new SamlResponse(xmlElement);

        foreach (var samlAssertionElement in samlResponse.GetSignedAssertions())
        {
            VerifyAssertionSignature(samlAssertionElement, x509Certificate, xmlSignature);
        }
    }
}

static void VerifyMetadataSignature(XmlElement xmlElement, X509Certificate2? x509Certificate, IXmlSignature xmlSignature)
{
    Console.Error.WriteLine("Verifying the SAML metadata signature.");
    VerifyXmlSignature(xmlElement, x509Certificate, xmlSignature);
}

static void VerifyXmlSignature(XmlElement xmlElement, X509Certificate2? x509Certificate, IXmlSignature xmlSignature)
{
    if (XmlSecurityUtility.IsSigned(xmlElement))
    {
        bool verified = false;

        using (var publicKey = x509Certificate?.GetPublicAsymmetricAlgorithm())
        {
            verified = xmlSignature.Verify(xmlElement, publicKey);
        }

        Console.WriteLine($"Signature verified: {verified}");
        Console.WriteLine($"Signature algorithm: {XmlSecurityUtility.GetSignatureAlgorithm(xmlElement)}");

        if (!verified)
        {
            if (x509Certificate != null)
            {
                Console.WriteLine($"Supplied certificate: Subject={x509Certificate.Subject}, Serial Number={x509Certificate.SerialNumber}, Thumbprint={x509Certificate.Thumbprint}");
            }

            var certificateBytes = XmlSecurityUtility.GetCertificate(xmlElement);

            if (certificateBytes != null)
            {
                var embeddedX509Certificate = new X509Certificate2(certificateBytes);

                Console.WriteLine($"Embedded certificate: Subject={embeddedX509Certificate.Subject}, Serial Number={embeddedX509Certificate.SerialNumber}, Thumbprint={embeddedX509Certificate.Thumbprint}");

                if (x509Certificate != null && x509Certificate.Thumbprint != embeddedX509Certificate.Thumbprint)
                {
                    Console.WriteLine("The wrong certificate is being used for the verification.");
                }
                else
                {
                    Console.WriteLine("The XML has been altered after signing.");
                }
            }
        }
    }
    else
    {
        Console.WriteLine("The XML isn't signed.");
    }
}
