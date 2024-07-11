using ComponentSpace.Saml2.Metadata;
using ComponentSpace.Saml2.Metadata.Export;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

/// <summary>
/// Creates local identity provider or service provider SAML metadata for distribution to a partner provider.
/// 
/// Usage: dotnet CreateMetadata.dll
/// </summary>
try
{
    Console.Write("Create Identity Provider or Service Provider metadata (IdP | SP): ");

    var entityDescriptor = (Console.ReadLine()?.ToLower()) switch
    {
        "idp" => CreateIdentityProviderMetadata(),
        "sp" => CreateServiceProviderMetadata(),
        _ => throw new ArgumentException("The provider type must either be \"IdP\" or \"SP\"."),
    };

    SaveMetadata(entityDescriptor);
}

catch (Exception exception)
{
    Console.WriteLine(exception.ToString());
}

static EntityDescriptor CreateIdentityProviderMetadata()
{
    var entityID = GetEntityID();

    Console.Write("X.509 signature certificate .CER file [None]: ");
    var fileName = Console.ReadLine();

    Console.Write("Single Sign-On Service URL: ");
    var singleSignOnServiceUrl = Console.ReadLine();

    if (string.IsNullOrEmpty(singleSignOnServiceUrl))
    {
        throw new ArgumentException("A single sign-on service URL must be specified.");
    }

    Console.Write("Single Logout Service URL [None]: ");
    var singleLogoutServiceUrl = Console.ReadLine();

    Console.Write("Name ID Format [None]: ");
    var nameIDFormat = Console.ReadLine();

    var wantAuthnRequestsSigned = GetBoolean("Want authn requests signed? [True]: ");

    using var signatureCertificate = LoadOptionalCertificate(fileName);

    var signatureCertificates = new List<X509Certificate2>();

    if (signatureCertificate != null)
    {
        signatureCertificates.Add(signatureCertificate);
    }

    var metadataExporter = new MetadataExporter()
    {
        EntityID = entityID,
    };

    metadataExporter.IdentityProviderMetadataExporters.Add(new IdentityProviderMetadataExporter()
    {
        SignatureCertificates = signatureCertificates,
        SingleSignOnServiceUrl = singleSignOnServiceUrl,
        SingleLogoutServiceUrl = singleLogoutServiceUrl,
        NameIdFormats = !string.IsNullOrEmpty(nameIDFormat) ? new List<string>() { nameIDFormat } : null,
        WantAuthnRequestsSigned = wantAuthnRequestsSigned ?? true
    });

    return metadataExporter.Export();
}

static EntityDescriptor CreateServiceProviderMetadata()
{
    var entityID = GetEntityID();

    Console.Write("X.509 signature certificate .CER file [None]: ");
    var signatureFileName = Console.ReadLine();

    Console.Write("X.509 encryption certificate .CER file [None]: ");
    var encryptionFileName = Console.ReadLine();

    Console.Write("Assertion Consumer Service URL: ");
    var assertionConsumerServiceUrl = Console.ReadLine();

    if (string.IsNullOrEmpty(assertionConsumerServiceUrl))
    {
        throw new ArgumentException("An assertion consumer service URL must be specified.");
    }

    Console.Write("Single Logout Service URL [None]: ");
    var singleLogoutServiceUrl = Console.ReadLine();

    Console.Write("Name ID Format [None]: ");
    var nameIDFormat = Console.ReadLine();

    var authnRequestsSigned = GetBoolean("Authn requests signed? [True]: ");
    var wantAssertionsSigned = GetBoolean("Want assertions signed? [True]: ");

    using var signatureCertificate = LoadOptionalCertificate(signatureFileName);

    var signatureCertificates = new List<X509Certificate2>();

    if (signatureCertificate != null)
    {
        signatureCertificates.Add(signatureCertificate);
    }

    using var encryptionCertificate = LoadOptionalCertificate(encryptionFileName);

    var encryptionCertificates = new List<X509Certificate2>();

    if (encryptionCertificate != null)
    {
        encryptionCertificates.Add(encryptionCertificate);
    }

    var metadataExporter = new MetadataExporter()
    {
        EntityID = entityID,
    };

    metadataExporter.ServiceProviderMetadataExporters.Add(new ServiceProviderMetadataExporter()
    {
        SignatureCertificates = signatureCertificates,
        EncryptionCertificates = encryptionCertificates,
        AssertionConsumerServiceUrl = assertionConsumerServiceUrl,
        SingleLogoutServiceUrl = singleLogoutServiceUrl,
        NameIdFormats = !string.IsNullOrEmpty(nameIDFormat) ? new List<string>() { nameIDFormat } : null,
        AuthnRequestsSigned = authnRequestsSigned ?? true,
        WantAssertionsSigned = wantAssertionsSigned ?? true
    });

    return metadataExporter.Export();
}

static string GetEntityID()
{
    Console.Write("Entity ID: ");
    var entityID = Console.ReadLine();

    if (string.IsNullOrEmpty(entityID))
    {
        throw new ArgumentException("An entity ID must be specified.");
    }

    return entityID;
}

static bool? GetBoolean(string prompt)
{
    bool? booleanValue = null;

    Console.Write(prompt);
    var inputText = Console.ReadLine();

    if (!string.IsNullOrEmpty(inputText))
    {
        try
        {
            booleanValue = Boolean.Parse(inputText);
        }

        catch (Exception exception)
        {
            throw new ArgumentException("A boolean value is required.", exception);
        }
    }

    return booleanValue;
}

static X509Certificate2? LoadOptionalCertificate(string? fileName)
{
    if (string.IsNullOrEmpty(fileName))
    {
        return null;
    }

    if (!File.Exists(fileName))
    {
        throw new ArgumentException(string.Format("The X.509 certificate file {0} doesn't exist.", fileName));
    }

    return new X509Certificate2(fileName);
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
