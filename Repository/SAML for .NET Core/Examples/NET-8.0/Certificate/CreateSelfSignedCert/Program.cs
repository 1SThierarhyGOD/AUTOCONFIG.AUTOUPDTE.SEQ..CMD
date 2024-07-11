using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

/// <summary>
/// Creates a self-signed X.509 certificate.
/// 
/// Usage: dotnet CreateSelfSignedCert.dll
/// </summary>
try
{
    Console.Write("Subject distinguished name (eg CN=test): ");
    var subjectName = Console.ReadLine();

    if (string.IsNullOrEmpty(subjectName))
    {
        throw new ArgumentException("A subject distinguished name must be specified.");
    }

    try
    {
        new X500DistinguishedName(subjectName);
    }

    catch (Exception exception)
    {
        throw new ArgumentException("The subject must be an X.500 distinguished name (eg CN=test).", exception);
    }

    Console.Write("Optional subject alternative name (eg test): ");
    var subjectAlternativeName = Console.ReadLine();

    var keySizeInBits = 2048;
    Console.Write($"Key Size in bits [{keySizeInBits}]: ");
    var input = Console.ReadLine();

    if (!string.IsNullOrEmpty(input) && !int.TryParse(input, out keySizeInBits))
    {
        throw new ArgumentException("The key size must be an integer.");
    }

    var yearsBeforeExpiring = 5;
    Console.Write($"Number of years before expiring [{yearsBeforeExpiring}]: ");
    input = Console.ReadLine();

    if (!string.IsNullOrEmpty(input) && !int.TryParse(input, out yearsBeforeExpiring))
    {
        throw new ArgumentException("The number of years must be an integer.");
    }

    using var privateKey = RSA.Create(keySizeInBits);

    var certificateRequest = new CertificateRequest(subjectName, privateKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

    certificateRequest.CertificateExtensions.Add(
        new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment, false));

    if (!string.IsNullOrEmpty(subjectAlternativeName))
    {
        var subjectAlternativeNameBuilder = new SubjectAlternativeNameBuilder();

        subjectAlternativeNameBuilder.AddDnsName(subjectAlternativeName);
        certificateRequest.CertificateExtensions.Add(subjectAlternativeNameBuilder.Build());
    }

    var notBefore = DateTimeOffset.UtcNow;
    var notAfter = notBefore.AddYears(yearsBeforeExpiring);

    using var x509Certificate = certificateRequest.CreateSelfSigned(notBefore, notAfter);

    Console.Write("Certificate file name (eg test.cer): ");
    var fileName = Console.ReadLine();

    if (string.IsNullOrEmpty(fileName))
    {
        throw new ArgumentException("A file name must be specified.");
    }

    var stringBuilder = new StringBuilder();

    stringBuilder.AppendLine("-----BEGIN CERTIFICATE-----");
    stringBuilder.AppendLine(Convert.ToBase64String(x509Certificate.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks));
    stringBuilder.AppendLine("-----END CERTIFICATE-----");

    File.WriteAllText(fileName, stringBuilder.ToString());
    Console.WriteLine($"The certificate has been saved to {fileName}.");

    Console.Write("Private key file name (eg test.pfx): ");
    fileName = Console.ReadLine();

    if (string.IsNullOrEmpty(fileName))
    {
        throw new ArgumentException("A file name must be specified.");
    }

    Console.Write("Private key password: ");
    var password = Console.ReadLine();

    if (string.IsNullOrEmpty(password))
    {
        throw new ArgumentException("A password must be specified.");
    }

    File.WriteAllBytes(fileName, x509Certificate.Export(X509ContentType.Pfx, password));
    Console.WriteLine($"The private key has been saved to {fileName}.");
}

catch (Exception exception)
{
    Console.WriteLine(exception.ToString());
}
