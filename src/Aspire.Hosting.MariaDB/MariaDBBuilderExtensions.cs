// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Utils;
using Aspire.Hosting.MariaDB;

namespace Aspire.Hosting;

/// <summary>
/// Provides extension methods for adding MariaDB resources to an <see cref="IDistributedApplicationBuilder"/>.
/// </summary>
public static class MariaDBBuilderExtensions
{
    private const string PasswordEnvVarName = "MARIADB_ROOT_PASSWORD";

    /// <summary>
    /// Adds a MariaDB server resource to the application model. For local development a container is used. This version the package defaults to the 8.3.0 tag of the mysql container image
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="password">The parameter used to provide the root password for the MariaDB resource. If <see langword="null"/> a random password will be generated.</param>
    /// <param name="port">The host port for MariaDB.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<MariaDBServerResource> AddMariaDB(this IDistributedApplicationBuilder builder, string name, IResourceBuilder<ParameterResource>? password = null, int? port = null)
    {
        var passwordParameter = password?.Resource ?? ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(builder, $"{name}-password");

        var resource = new MariaDBServerResource(name, passwordParameter);
        return builder.AddResource(resource)
                      .WithEndpoint(port: port, targetPort: 3306, name: MariaDBServerResource.PrimaryEndpointName) // Internal port is always 3306.
                      .WithImage(MariaDBContainerImageTags.Image, MariaDBContainerImageTags.Tag)
                      .WithImageRegistry(MariaDBContainerImageTags.Registry)
                      .WithEnvironment(context =>
                      {
                          context.EnvironmentVariables[PasswordEnvVarName] = resource.PasswordParameter;
                      });
    }

    /// <summary>
    /// Adds a MariaDB database to the application model.
    /// </summary>
    /// <param name="builder">The MariaDB server resource builder.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="databaseName">The name of the database. If not provided, this defaults to the same value as <paramref name="name"/>.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<MariaDBDatabaseResource> AddDatabase(this IResourceBuilder<MariaDBServerResource> builder, string name, string? databaseName = null)
    {
        // Use the resource name as the database name if it's not provided
        databaseName ??= name;

        builder.Resource.AddDatabase(name, databaseName);
        var mySqlDatabase = new MariaDBDatabaseResource(name, databaseName, builder.Resource);
        return builder.ApplicationBuilder.AddResource(mySqlDatabase);
    }

    /// <summary>
    /// Adds a named volume for the data folder to a MariaDB container resource.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="name">The name of the volume. Defaults to an auto-generated name based on the application and resource names.</param>
    /// <param name="isReadOnly">A flag that indicates if this is a read-only volume.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<MariaDBServerResource> WithDataVolume(this IResourceBuilder<MariaDBServerResource> builder, string? name = null, bool isReadOnly = false)
        => builder.WithVolume(name ?? VolumeNameGenerator.CreateVolumeName(builder, "data"), "/var/lib/mysql", isReadOnly);

    /// <summary>
    /// Adds a bind mount for the data folder to a MariaDB container resource.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="source">The source directory on the host to mount into the container.</param>
    /// <param name="isReadOnly">A flag that indicates if this is a read-only mount.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<MariaDBServerResource> WithDataBindMount(this IResourceBuilder<MariaDBServerResource> builder, string source, bool isReadOnly = false)
        => builder.WithBindMount(source, "/var/lib/mysql", isReadOnly);

    /// <summary>
    /// Adds a bind mount for the init folder to a MariaDB container resource.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="source">The source directory on the host to mount into the container.</param>
    /// <param name="isReadOnly">A flag that indicates if this is a read-only mount.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<MariaDBServerResource> WithInitBindMount(this IResourceBuilder<MariaDBServerResource> builder, string source, bool isReadOnly = true)
        => builder.WithBindMount(source, "/docker-entrypoint-initdb.d", isReadOnly);
}
