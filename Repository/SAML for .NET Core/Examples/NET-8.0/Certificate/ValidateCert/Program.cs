using ComponentSpace.Saml2.Certificates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.Security.Cryptography.X509Certificates;

/// <summary>
/// Validates an X.509 certificate.
/// 
/// Usage: dotnet ValidateCert.dll <fileName> [--password <password>]
/// 
/// where the file contains an X.509 certificate to be validated.
/// </summary>
try
{
    var fileArgument = new Argument<FileInfo>(
        name: "file",
        description: "The X.509 certificate file.");

    var passwordOption = new Option<string>(
        name: "--password",
        description: "The X.509 certificate file password.");

    var rootCommand = new RootCommand("Validate an X.509 certificate")
    {
        fileArgument,
        passwordOption
    };

    rootCommand.SetHandler((fileInfo, password) =>
    {
        ValidateCert(fileInfo, password);
    },
    fileArgument, passwordOption);

    rootCommand.Invoke(args);
}

catch (Exception exception)
{
    Console.WriteLine(exception.ToString());
}

static void ValidateCert(FileInfo certificateFileInfo, string password)
{
    if (!File.Exists(certificateFileInfo.FullName))
    {
        throw new ArgumentException($"The file {certificateFileInfo.FullName} doesn't exist.");
    }

    var x509Certificate = new X509Certificate2(certificateFileInfo.FullName, password, X509KeyStorageFlags.EphemeralKeySet);

    var serviceCollection = new ServiceCollection();

    serviceCollection.AddLogging(builder =>
    {
        builder.SetMinimumLevel(LogLevel.Debug);
        builder.AddConsole();
    });

    serviceCollection.Configure<CertificateValidationOptions>(options =>
    {
        options.EnableChainCheck = true;
    });

    serviceCollection.AddSaml();

    using var serviceProvider = serviceCollection.BuildServiceProvider();

    foreach (var certificateValidator in serviceProvider.GetServices<ICertificateValidator>())
    {
        certificateValidator.Validate(x509Certificate);
    }
}
