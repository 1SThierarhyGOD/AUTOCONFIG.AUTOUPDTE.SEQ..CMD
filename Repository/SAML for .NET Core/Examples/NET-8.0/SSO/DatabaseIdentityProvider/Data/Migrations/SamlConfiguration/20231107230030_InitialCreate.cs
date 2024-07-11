using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatabaseIdentityProvider.Data.Migrations.SamlConfiguration
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocalIdentityProviderConfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SingleSignOnServiceUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    DisableSchemaCheck = table.Column<bool>(type: "INTEGER", nullable: false),
                    ResolveToHttps = table.Column<bool>(type: "INTEGER", nullable: false),
                    SingleLogoutServiceUrl = table.Column<string>(type: "TEXT", nullable: true),
                    ArtifactResolutionServiceUrl = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalIdentityProviderConfiguration", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LocalServiceProviderConfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AssertionConsumerServiceUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    DisableSchemaCheck = table.Column<bool>(type: "INTEGER", nullable: false),
                    ResolveToHttps = table.Column<bool>(type: "INTEGER", nullable: false),
                    SingleLogoutServiceUrl = table.Column<string>(type: "TEXT", nullable: true),
                    ArtifactResolutionServiceUrl = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalServiceProviderConfiguration", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SamlConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    LocalIdentityProviderConfigurationId = table.Column<int>(type: "INTEGER", nullable: true),
                    LocalServiceProviderConfigurationId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SamlConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SamlConfigurations_LocalIdentityProviderConfiguration_LocalIdentityProviderConfigurationId",
                        column: x => x.LocalIdentityProviderConfigurationId,
                        principalTable: "LocalIdentityProviderConfiguration",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SamlConfigurations_LocalServiceProviderConfiguration_LocalServiceProviderConfigurationId",
                        column: x => x.LocalServiceProviderConfigurationId,
                        principalTable: "LocalServiceProviderConfiguration",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PartnerIdentityProviderConfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SingleSignOnServiceUrl = table.Column<string>(type: "TEXT", nullable: true),
                    SingleSignOnServiceBinding = table.Column<string>(type: "TEXT", nullable: true),
                    SignAuthnRequest = table.Column<bool>(type: "INTEGER", nullable: false),
                    ForceAuthn = table.Column<bool>(type: "INTEGER", nullable: false),
                    WantAssertionOrResponseSigned = table.Column<bool>(type: "INTEGER", nullable: false),
                    WantSamlResponseSigned = table.Column<bool>(type: "INTEGER", nullable: false),
                    WantAssertionSigned = table.Column<bool>(type: "INTEGER", nullable: false),
                    WantAssertionEncrypted = table.Column<bool>(type: "INTEGER", nullable: false),
                    WantNameIDEncrypted = table.Column<bool>(type: "INTEGER", nullable: false),
                    ProviderName = table.Column<string>(type: "TEXT", nullable: true),
                    RequestedAuthnContexts = table.Column<string>(type: "TEXT", nullable: true),
                    RequestedAuthnContextComparison = table.Column<string>(type: "TEXT", nullable: true),
                    ExpectedAuthnContext = table.Column<string>(type: "TEXT", nullable: true),
                    DisableIdPInitiatedSso = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisableAssertionReplayCheck = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisableRecipientCheck = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisableTimePeriodCheck = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisableAudienceRestrictionCheck = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisableAuthnContextCheck = table.Column<bool>(type: "INTEGER", nullable: false),
                    SamlConfigurationId = table.Column<int>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    AssertionConsumerServiceBinding = table.Column<string>(type: "TEXT", nullable: true),
                    SingleLogoutServiceUrl = table.Column<string>(type: "TEXT", nullable: true),
                    SingleLogoutServiceResponseUrl = table.Column<string>(type: "TEXT", nullable: true),
                    SingleLogoutServiceBinding = table.Column<string>(type: "TEXT", nullable: true),
                    ArtifactResolutionServiceUrl = table.Column<string>(type: "TEXT", nullable: true),
                    ArtifactEncoding = table.Column<string>(type: "TEXT", nullable: true),
                    LogoutRequestLifeTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    SignLogoutRequest = table.Column<bool>(type: "INTEGER", nullable: false),
                    SignLogoutResponse = table.Column<bool>(type: "INTEGER", nullable: false),
                    WantLogoutRequestSigned = table.Column<bool>(type: "INTEGER", nullable: false),
                    WantLogoutResponseSigned = table.Column<bool>(type: "INTEGER", nullable: false),
                    SignArtifactResolve = table.Column<bool>(type: "INTEGER", nullable: false),
                    SignArtifactResponse = table.Column<bool>(type: "INTEGER", nullable: false),
                    WantArtifactResolveSigned = table.Column<bool>(type: "INTEGER", nullable: false),
                    WantArtifactResponseSigned = table.Column<bool>(type: "INTEGER", nullable: false),
                    EncryptLogoutNameID = table.Column<bool>(type: "INTEGER", nullable: false),
                    IssuerFormat = table.Column<string>(type: "TEXT", nullable: true),
                    IssuerQualifier = table.Column<string>(type: "TEXT", nullable: true),
                    NameIDFormat = table.Column<string>(type: "TEXT", nullable: true),
                    NameIDQualifier = table.Column<string>(type: "TEXT", nullable: true),
                    DigestAlgorithm = table.Column<string>(type: "TEXT", nullable: true),
                    SignatureAlgorithm = table.Column<string>(type: "TEXT", nullable: true),
                    WantDigestAlgorithm = table.Column<string>(type: "TEXT", nullable: true),
                    WantSignatureAlgorithm = table.Column<string>(type: "TEXT", nullable: true),
                    KeyEncryptionAlgorithm = table.Column<string>(type: "TEXT", nullable: true),
                    KeyEncryptionDigestAlgorithm = table.Column<string>(type: "TEXT", nullable: true),
                    KeyEncryptionMaskGenerationFunction = table.Column<string>(type: "TEXT", nullable: true),
                    DataEncryptionAlgorithm = table.Column<string>(type: "TEXT", nullable: true),
                    ClockSkew = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    UseEmbeddedCertificate = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableSha1Support = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisableDestinationCheck = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisableInboundLogout = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisableOutboundLogout = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisableInResponseToCheck = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisablePendingLogoutCheck = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisableLogoutResponseStatusCheck = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisableClearAllSessionsOnLogout = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerIdentityProviderConfiguration", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartnerIdentityProviderConfiguration_SamlConfigurations_SamlConfigurationId",
                        column: x => x.SamlConfigurationId,
                        principalTable: "SamlConfigurations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PartnerServiceProviderConfiguration",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AssertionConsumerServiceUrl = table.Column<string>(type: "TEXT", nullable: true),
                    ValidAssertionConsumerServiceUrls = table.Column<string>(type: "TEXT", nullable: true),
                    WantAuthnRequestSigned = table.Column<bool>(type: "INTEGER", nullable: false),
                    SignSamlResponse = table.Column<bool>(type: "INTEGER", nullable: false),
                    SignAssertion = table.Column<bool>(type: "INTEGER", nullable: false),
                    EncryptAssertion = table.Column<bool>(type: "INTEGER", nullable: false),
                    EncryptNameID = table.Column<bool>(type: "INTEGER", nullable: false),
                    AssertionLifeTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    AuthnContext = table.Column<string>(type: "TEXT", nullable: true),
                    RelayState = table.Column<string>(type: "TEXT", nullable: true),
                    SamlConfigurationId = table.Column<int>(type: "INTEGER", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    AssertionConsumerServiceBinding = table.Column<string>(type: "TEXT", nullable: true),
                    SingleLogoutServiceUrl = table.Column<string>(type: "TEXT", nullable: true),
                    SingleLogoutServiceResponseUrl = table.Column<string>(type: "TEXT", nullable: true),
                    SingleLogoutServiceBinding = table.Column<string>(type: "TEXT", nullable: true),
                    ArtifactResolutionServiceUrl = table.Column<string>(type: "TEXT", nullable: true),
                    ArtifactEncoding = table.Column<string>(type: "TEXT", nullable: true),
                    LogoutRequestLifeTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    SignLogoutRequest = table.Column<bool>(type: "INTEGER", nullable: false),
                    SignLogoutResponse = table.Column<bool>(type: "INTEGER", nullable: false),
                    WantLogoutRequestSigned = table.Column<bool>(type: "INTEGER", nullable: false),
                    WantLogoutResponseSigned = table.Column<bool>(type: "INTEGER", nullable: false),
                    SignArtifactResolve = table.Column<bool>(type: "INTEGER", nullable: false),
                    SignArtifactResponse = table.Column<bool>(type: "INTEGER", nullable: false),
                    WantArtifactResolveSigned = table.Column<bool>(type: "INTEGER", nullable: false),
                    WantArtifactResponseSigned = table.Column<bool>(type: "INTEGER", nullable: false),
                    EncryptLogoutNameID = table.Column<bool>(type: "INTEGER", nullable: false),
                    IssuerFormat = table.Column<string>(type: "TEXT", nullable: true),
                    IssuerQualifier = table.Column<string>(type: "TEXT", nullable: true),
                    NameIDFormat = table.Column<string>(type: "TEXT", nullable: true),
                    NameIDQualifier = table.Column<string>(type: "TEXT", nullable: true),
                    DigestAlgorithm = table.Column<string>(type: "TEXT", nullable: true),
                    SignatureAlgorithm = table.Column<string>(type: "TEXT", nullable: true),
                    WantDigestAlgorithm = table.Column<string>(type: "TEXT", nullable: true),
                    WantSignatureAlgorithm = table.Column<string>(type: "TEXT", nullable: true),
                    KeyEncryptionAlgorithm = table.Column<string>(type: "TEXT", nullable: true),
                    KeyEncryptionDigestAlgorithm = table.Column<string>(type: "TEXT", nullable: true),
                    KeyEncryptionMaskGenerationFunction = table.Column<string>(type: "TEXT", nullable: true),
                    DataEncryptionAlgorithm = table.Column<string>(type: "TEXT", nullable: true),
                    ClockSkew = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    UseEmbeddedCertificate = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableSha1Support = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisableDestinationCheck = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisableInboundLogout = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisableOutboundLogout = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisableInResponseToCheck = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisablePendingLogoutCheck = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisableLogoutResponseStatusCheck = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisableClearAllSessionsOnLogout = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerServiceProviderConfiguration", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartnerServiceProviderConfiguration_SamlConfigurations_SamlConfigurationId",
                        column: x => x.SamlConfigurationId,
                        principalTable: "SamlConfigurations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Certificate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Use = table.Column<string>(type: "TEXT", nullable: true),
                    String = table.Column<string>(type: "TEXT", nullable: true),
                    FileName = table.Column<string>(type: "TEXT", nullable: true),
                    Password = table.Column<string>(type: "TEXT", nullable: true),
                    StoreName = table.Column<string>(type: "TEXT", nullable: true),
                    StoreLocation = table.Column<string>(type: "TEXT", nullable: true),
                    SerialNumber = table.Column<string>(type: "TEXT", nullable: true),
                    Thumbprint = table.Column<string>(type: "TEXT", nullable: true),
                    SubjectName = table.Column<string>(type: "TEXT", nullable: true),
                    Key = table.Column<string>(type: "TEXT", nullable: true),
                    LocalIdentityProviderConfigurationId = table.Column<int>(type: "INTEGER", nullable: true),
                    LocalServiceProviderConfigurationId = table.Column<int>(type: "INTEGER", nullable: true),
                    PartnerIdentityProviderConfigurationIdForLocalCert = table.Column<int>(type: "INTEGER", nullable: true),
                    PartnerIdentityProviderConfigurationIdForPartnerCert = table.Column<int>(type: "INTEGER", nullable: true),
                    PartnerServiceProviderConfigurationIdForLocalCert = table.Column<int>(type: "INTEGER", nullable: true),
                    PartnerServiceProviderConfigurationIdForPartnerCert = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certificate_LocalIdentityProviderConfiguration_LocalIdentityProviderConfigurationId",
                        column: x => x.LocalIdentityProviderConfigurationId,
                        principalTable: "LocalIdentityProviderConfiguration",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Certificate_LocalServiceProviderConfiguration_LocalServiceProviderConfigurationId",
                        column: x => x.LocalServiceProviderConfigurationId,
                        principalTable: "LocalServiceProviderConfiguration",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Certificate_PartnerIdentityProviderConfiguration_PartnerIdentityProviderConfigurationIdForLocalCert",
                        column: x => x.PartnerIdentityProviderConfigurationIdForLocalCert,
                        principalTable: "PartnerIdentityProviderConfiguration",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Certificate_PartnerIdentityProviderConfiguration_PartnerIdentityProviderConfigurationIdForPartnerCert",
                        column: x => x.PartnerIdentityProviderConfigurationIdForPartnerCert,
                        principalTable: "PartnerIdentityProviderConfiguration",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Certificate_PartnerServiceProviderConfiguration_PartnerServiceProviderConfigurationIdForLocalCert",
                        column: x => x.PartnerServiceProviderConfigurationIdForLocalCert,
                        principalTable: "PartnerServiceProviderConfiguration",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Certificate_PartnerServiceProviderConfiguration_PartnerServiceProviderConfigurationIdForPartnerCert",
                        column: x => x.PartnerServiceProviderConfigurationIdForPartnerCert,
                        principalTable: "PartnerServiceProviderConfiguration",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SamlMappingRule",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Rule = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Value = table.Column<string>(type: "TEXT", nullable: true),
                    PartnerIdentityProviderConfigurationId = table.Column<int>(type: "INTEGER", nullable: true),
                    PartnerServiceProviderConfigurationId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SamlMappingRule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SamlMappingRule_PartnerIdentityProviderConfiguration_PartnerIdentityProviderConfigurationId",
                        column: x => x.PartnerIdentityProviderConfigurationId,
                        principalTable: "PartnerIdentityProviderConfiguration",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SamlMappingRule_PartnerServiceProviderConfiguration_PartnerServiceProviderConfigurationId",
                        column: x => x.PartnerServiceProviderConfigurationId,
                        principalTable: "PartnerServiceProviderConfiguration",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Certificate_LocalIdentityProviderConfigurationId",
                table: "Certificate",
                column: "LocalIdentityProviderConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificate_LocalServiceProviderConfigurationId",
                table: "Certificate",
                column: "LocalServiceProviderConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificate_PartnerIdentityProviderConfigurationIdForLocalCert",
                table: "Certificate",
                column: "PartnerIdentityProviderConfigurationIdForLocalCert");

            migrationBuilder.CreateIndex(
                name: "IX_Certificate_PartnerIdentityProviderConfigurationIdForPartnerCert",
                table: "Certificate",
                column: "PartnerIdentityProviderConfigurationIdForPartnerCert");

            migrationBuilder.CreateIndex(
                name: "IX_Certificate_PartnerServiceProviderConfigurationIdForLocalCert",
                table: "Certificate",
                column: "PartnerServiceProviderConfigurationIdForLocalCert");

            migrationBuilder.CreateIndex(
                name: "IX_Certificate_PartnerServiceProviderConfigurationIdForPartnerCert",
                table: "Certificate",
                column: "PartnerServiceProviderConfigurationIdForPartnerCert");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerIdentityProviderConfiguration_Name",
                table: "PartnerIdentityProviderConfiguration",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerIdentityProviderConfiguration_SamlConfigurationId",
                table: "PartnerIdentityProviderConfiguration",
                column: "SamlConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerServiceProviderConfiguration_Name",
                table: "PartnerServiceProviderConfiguration",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerServiceProviderConfiguration_SamlConfigurationId",
                table: "PartnerServiceProviderConfiguration",
                column: "SamlConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_SamlConfigurations_LocalIdentityProviderConfigurationId",
                table: "SamlConfigurations",
                column: "LocalIdentityProviderConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_SamlConfigurations_LocalServiceProviderConfigurationId",
                table: "SamlConfigurations",
                column: "LocalServiceProviderConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_SamlConfigurations_Name",
                table: "SamlConfigurations",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SamlMappingRule_PartnerIdentityProviderConfigurationId",
                table: "SamlMappingRule",
                column: "PartnerIdentityProviderConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_SamlMappingRule_PartnerServiceProviderConfigurationId",
                table: "SamlMappingRule",
                column: "PartnerServiceProviderConfigurationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Certificate");

            migrationBuilder.DropTable(
                name: "SamlMappingRule");

            migrationBuilder.DropTable(
                name: "PartnerIdentityProviderConfiguration");

            migrationBuilder.DropTable(
                name: "PartnerServiceProviderConfiguration");

            migrationBuilder.DropTable(
                name: "SamlConfigurations");

            migrationBuilder.DropTable(
                name: "LocalIdentityProviderConfiguration");

            migrationBuilder.DropTable(
                name: "LocalServiceProviderConfiguration");
        }
    }
}
