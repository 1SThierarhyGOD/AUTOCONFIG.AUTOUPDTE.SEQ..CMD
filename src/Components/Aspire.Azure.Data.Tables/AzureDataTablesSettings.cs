// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Azure.Core;

namespace Aspire.Azure.Data.Tables;

/// <summary>
/// Provides the client configuration settings for connecting to Azure Tables.
/// </summary>
public sealed class AzureDataTablesSettings
{
    /// <summary>
    /// A <see cref="Uri"/> referencing the table service account.
    /// This is likely to be similar to "https://{account_name}.table.core.windows.net/" or "https://{account_name}.table.cosmos.azure.com/".
    /// </summary>
    public Uri? ServiceUri { get; set; }

    /// <summary>
    /// Gets or sets the credential used to authenticate to the Azure Tables.
    /// </summary>
    public TokenCredential? Credential { get; set; }

    /// <summary>
    /// <para>Gets or sets a boolean value that indicates whether the health check is enabled or not.</para>
    /// <para>Enabled by default.</para>
    /// </summary>
    public bool HealthChecks { get; set; } = true;

    /// <summary>
    /// <para>Gets or sets a boolean value that indicates whether the OpenTelemetry tracing is enabled or not.</para>
    /// <para>Disabled by default.</para>
    /// </summary>
    /// <remarks>
    /// ActivitySource support in Azure SDK is experimental, the shape of Activities may change in the future without notice.
    /// It can be enabled by setting "Azure.Experimental.EnableActivitySource" <see cref="AppContext"/> switch to true.
    /// Or by setting "AZURE_EXPERIMENTAL_ENABLE_ACTIVITY_SOURCE" environment variable to "true".
    /// </remarks>
    public bool Tracing { get; set; }
}
