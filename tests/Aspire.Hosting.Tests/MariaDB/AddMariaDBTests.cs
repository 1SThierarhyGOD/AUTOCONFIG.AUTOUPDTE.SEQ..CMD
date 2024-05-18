// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.Sockets;
using Aspire.Hosting.MariaDB;
using Aspire.Hosting.Tests.Utils;
using Aspire.Hosting.Utils;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Aspire.Hosting.Tests.MariaDB;

public class AddMariaDBTests
{
    [Fact]
    public async Task AddMariaDBContainerWithDefaultsAddsAnnotationMetadata()
    {
        var appBuilder = DistributedApplication.CreateBuilder();
        appBuilder.AddMariaDB("mariadb");

        using var app = appBuilder.Build();

        var appModel = app.Services.GetRequiredService<DistributedApplicationModel>();

        var containerResource = Assert.Single(appModel.Resources.OfType<MariaDBServerResource>());
        Assert.Equal("mariadb", containerResource.Name);

        var containerAnnotation = Assert.Single(containerResource.Annotations.OfType<ContainerImageAnnotation>());
        Assert.Equal(MariaDBContainerImageTags.Tag, containerAnnotation.Tag);
        Assert.Equal(MariaDBContainerImageTags.Image, containerAnnotation.Image);
        Assert.Equal(MariaDBContainerImageTags.Registry, containerAnnotation.Registry);

        var endpoint = Assert.Single(containerResource.Annotations.OfType<EndpointAnnotation>());
        Assert.Equal(3306, endpoint.TargetPort);
        Assert.False(endpoint.IsExternal);
        Assert.Equal("tcp", endpoint.Name);
        Assert.Null(endpoint.Port);
        Assert.Equal(ProtocolType.Tcp, endpoint.Protocol);
        Assert.Equal("tcp", endpoint.Transport);
        Assert.Equal("tcp", endpoint.UriScheme);

        var config = await EnvironmentVariableEvaluator.GetEnvironmentVariablesAsync(containerResource);

        Assert.Collection(config,
            env =>
            {
                Assert.Equal("MARIADB_ROOT_PASSWORD", env.Key);
                Assert.False(string.IsNullOrEmpty(env.Value));
            });
    }

    [Fact]
    public async Task AddMariaDBAddsAnnotationMetadata()
    {
        var appBuilder = DistributedApplication.CreateBuilder();
        appBuilder.Configuration["Parameters:pass"] = "pass";

        var pass = appBuilder.AddParameter("pass");
        appBuilder.AddMariaDB("mariadb", pass, 1234);

        using var app = appBuilder.Build();

        var appModel = app.Services.GetRequiredService<DistributedApplicationModel>();

        var containerResource = Assert.Single(appModel.GetContainerResources());
        Assert.Equal("mariadb", containerResource.Name);

        var containerAnnotation = Assert.Single(containerResource.Annotations.OfType<ContainerImageAnnotation>());
        Assert.Equal(MariaDBContainerImageTags.Tag, containerAnnotation.Tag);
        Assert.Equal(MariaDBContainerImageTags.Image, containerAnnotation.Image);
        Assert.Equal(MariaDBContainerImageTags.Registry, containerAnnotation.Registry);

        var endpoint = Assert.Single(containerResource.Annotations.OfType<EndpointAnnotation>());
        Assert.Equal(3306, endpoint.TargetPort);
        Assert.False(endpoint.IsExternal);
        Assert.Equal("tcp", endpoint.Name);
        Assert.Equal(1234, endpoint.Port);
        Assert.Equal(ProtocolType.Tcp, endpoint.Protocol);
        Assert.Equal("tcp", endpoint.Transport);
        Assert.Equal("tcp", endpoint.UriScheme);

        var config = await EnvironmentVariableEvaluator.GetEnvironmentVariablesAsync(containerResource);

        Assert.Collection(config,
            env =>
            {
                Assert.Equal("MARIADB_ROOT_PASSWORD", env.Key);
                Assert.Equal("pass", env.Value);
            });
    }

    [Fact]
    public async Task MariaDBCreatesConnectionString()
    {
        var appBuilder = DistributedApplication.CreateBuilder();
        appBuilder.AddMariaDB("mariadb")
            .WithEndpoint("tcp", e => e.AllocatedEndpoint = new AllocatedEndpoint(e, "localhost", 2000));

        using var app = appBuilder.Build();

        var appModel = app.Services.GetRequiredService<DistributedApplicationModel>();

        var connectionStringResource = Assert.Single(appModel.Resources.OfType<IResourceWithConnectionString>());
        var connectionString = await connectionStringResource.GetConnectionStringAsync();

        Assert.Equal("Server={mariadb.bindings.tcp.host};Port={mariadb.bindings.tcp.port};User ID=root;Password={mariadb-password.value}", connectionStringResource.ConnectionStringExpression.ValueExpression);
        Assert.StartsWith("Server=localhost;Port=2000;User ID=root;Password=", connectionString);
    }

    [Fact]
    public async Task MariaDBCreatesConnectionStringWithDatabase()
    {
        var appBuilder = DistributedApplication.CreateBuilder();
        appBuilder.AddMariaDB("mariadb")
            .WithEndpoint("tcp", e => e.AllocatedEndpoint = new AllocatedEndpoint(e, "localhost", 2000))
            .AddDatabase("db");

        using var app = appBuilder.Build();

        var appModel = app.Services.GetRequiredService<DistributedApplicationModel>();

        var mySqlResource = Assert.Single(appModel.Resources.OfType<MariaDBServerResource>());
        var mySqlConnectionStringResource = (IResourceWithConnectionString)mySqlResource;
        var mySqlConnectionString = await mySqlConnectionStringResource.GetConnectionStringAsync();
        var mySqlDatabaseResource = Assert.Single(appModel.Resources.OfType<MariaDBDatabaseResource>());
        var mySqlDatabaseConnectionStringResource = (IResourceWithConnectionString)mySqlDatabaseResource;
        var dbConnectionString = await mySqlDatabaseConnectionStringResource.GetConnectionStringAsync();

        Assert.Equal(mySqlConnectionString + ";Database=db", dbConnectionString);
        Assert.Equal("{mariadb.connectionString};Database=db", mySqlDatabaseResource.ConnectionStringExpression.ValueExpression);
    }

    [Fact]
    public async Task VerifyManifest()
    {
        using var appBuilder = TestDistributedApplicationBuilder.Create();
        var mariadb = appBuilder.AddMariaDB("mariadb");
        var db = mariadb.AddDatabase("db");

        var mySqlManifest = await ManifestUtils.GetManifest(mariadb.Resource);
        var dbManifest = await ManifestUtils.GetManifest(db.Resource);

        var expectedManifest = $$"""
            {
              "type": "container.v0",
              "connectionString": "Server={mariadb.bindings.tcp.host};Port={mariadb.bindings.tcp.port};User ID=root;Password={mariadb-password.value}",
              "image": "{{MariaDBContainerImageTags.Registry}}/{{MariaDBContainerImageTags.Image}}:{{MariaDBContainerImageTags.Tag}}",
              "env": {
                "MARIADB_ROOT_PASSWORD": "{mariadb-password.value}"
              },
              "bindings": {
                "tcp": {
                  "scheme": "tcp",
                  "protocol": "tcp",
                  "transport": "tcp",
                  "targetPort": 3306
                }
              }
            }
            """;
        Assert.Equal(expectedManifest, mySqlManifest.ToString());

        expectedManifest = """
            {
              "type": "value.v0",
              "connectionString": "{mariadb.connectionString};Database=db"
            }
            """;
        Assert.Equal(expectedManifest, dbManifest.ToString());
    }

    [Fact]
    public async Task VerifyManifestWithPasswordParameter()
    {
        using var appBuilder = TestDistributedApplicationBuilder.Create();
        var pass = appBuilder.AddParameter("pass");

        var mariadb = appBuilder.AddMariaDB("mariadb", pass);
        var serverManifest = await ManifestUtils.GetManifest(mariadb.Resource);

        var expectedManifest = $$"""
            {
              "type": "container.v0",
              "connectionString": "Server={mariadb.bindings.tcp.host};Port={mariadb.bindings.tcp.port};User ID=root;Password={pass.value}",
              "image": "{{MariaDBContainerImageTags.Registry}}/{{MariaDBContainerImageTags.Image}}:{{MariaDBContainerImageTags.Tag}}",
              "env": {
                "MARIADB_ROOT_PASSWORD": "{pass.value}"
              },
              "bindings": {
                "tcp": {
                  "scheme": "tcp",
                  "protocol": "tcp",
                  "transport": "tcp",
                  "targetPort": 3306
                }
              }
            }
            """;
        Assert.Equal(expectedManifest, serverManifest.ToString());
    }

    [Fact]
    public void ThrowsWithIdenticalChildResourceNames()
    {
        using var builder = TestDistributedApplicationBuilder.Create();

        var db = builder.AddMariaDB("mariadb1");
        db.AddDatabase("db");

        Assert.Throws<DistributedApplicationException>(() => db.AddDatabase("db"));
    }

    [Fact]
    public void ThrowsWithIdenticalChildResourceNamesDifferentParents()
    {
        using var builder = TestDistributedApplicationBuilder.Create();

        builder.AddMariaDB("mariadb1")
            .AddDatabase("db");

        var db = builder.AddMariaDB("mariadb2");
        Assert.Throws<DistributedApplicationException>(() => db.AddDatabase("db"));
    }

    [Fact]
    public void CanAddDatabasesWithDifferentNamesOnSingleServer()
    {
        using var builder = TestDistributedApplicationBuilder.Create();

        var mariadb1 = builder.AddMariaDB("mariadb1");

        var db1 = mariadb1.AddDatabase("db1", "customers1");
        var db2 = mariadb1.AddDatabase("db2", "customers2");

        Assert.Equal(["db1", "db2"], mariadb1.Resource.Databases.Keys);
        Assert.Equal(["customers1", "customers2"], mariadb1.Resource.Databases.Values);

        Assert.Equal("customers1", db1.Resource.DatabaseName);
        Assert.Equal("customers2", db2.Resource.DatabaseName);

        Assert.Equal("{mariadb1.connectionString};Database=customers1", db1.Resource.ConnectionStringExpression.ValueExpression);
        Assert.Equal("{mariadb1.connectionString};Database=customers2", db2.Resource.ConnectionStringExpression.ValueExpression);
    }

    [Fact]
    public void CanAddDatabasesWithTheSameNameOnMultipleServers()
    {
        using var builder = TestDistributedApplicationBuilder.Create();

        var db1 = builder.AddMariaDB("mariadb1")
            .AddDatabase("db1", "imports");

        var db2 = builder.AddMariaDB("mariadb2")
            .AddDatabase("db2", "imports");

        Assert.Equal("imports", db1.Resource.DatabaseName);
        Assert.Equal("imports", db2.Resource.DatabaseName);

        Assert.Equal("{mariadb1.connectionString};Database=imports", db1.Resource.ConnectionStringExpression.ValueExpression);
        Assert.Equal("{mariadb2.connectionString};Database=imports", db2.Resource.ConnectionStringExpression.ValueExpression);
    }
}
