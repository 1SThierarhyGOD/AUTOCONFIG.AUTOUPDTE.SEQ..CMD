Introduction
============

This example uses an entity framework database for storing the SAML configuration.

Commands listed should be run from the Visual Studio Package Manager Console.
Alternatively, the equivalent dotnet ef commands may be run.

For more information, refer to:

https://docs.microsoft.com/en-us/ef/core/managing-schemas/

SQLite
======

The application is configured to use SQLite to store the SAML configuration.

The following connection string in appsettings.json specifies the SAML configuration database.

"SamlConfigurationConnection": "Data Source=SamlConfiguration.db"

The following code in Startup adds the SamlConfigurationContext.

// Add the SAML configuration database context.
builder.Services.AddDbContext<SamlConfigurationContext>(options =>
    options.UseSqlite(Configuration.GetConnectionString("SamlConfigurationConnection"),
        builder => builder.MigrationsAssembly("DatabaseIdentityProvider")));

The MigrationsAssembly method specifies that the migrations will be in the application's assembly 
rather than in the SamlConfigurationContext's assembly.

The following code specifies that SAML configuration will be retrieved from the database.

// Use the database configuration resolver.
builder.Services.AddSamlDatabaseConfigurationResolver();

SQLServer
=========

The application can be configured to use SQLServer to store the SAML configuration.

The following connection string in appsettings.json specifies the SAML configuration database.

"SamlConfigurationConnection": "Server=localhost;Database=SamlConfiguration;Trusted_Connection=True;MultipleActiveResultSets=true"

The following code in Startup adds the SamlConfigurationContext.

// Add the SAML configuration database context.
builder.Services.AddDbContext<SamlConfigurationContext>(options =>
    options.UseSqlServer(Configuration.GetConnectionString("SamlConfigurationConnection"),
        builder => builder.MigrationsAssembly("DatabaseIdentityProvider")));

The MigrationsAssembly method specifies that the migrations will be in the application's assembly 
rather than in the SamlConfigurationContext's assembly.

The following code specifies that SAML configuration will be retrieved from the database.

// Use the database configuration resolver.
builder.Services.AddSamlDatabaseConfigurationResolver();

Initial Migration
=================

To create the initial migration:

Add-Migration InitialCreate -Context SamlConfigurationContext -Project DatabaseIdentityProvider -StartupProject DatabaseIdentityProvider -OutputDir Data\Migrations\SamlConfiguration

This has already been done for the example project.

Database Creation
=================

To create the database:

Update-Database -Context SamlConfigurationContext -Project DatabaseIdentityProvider -StartupProject DatabaseIdentityProvider

This has already been done for the example project.

Data Seeding
============

To seed the database with SAML configuration:

Click the Seed Data button on the About page.

This has already been done for the example project.

Remove Migration
================

If necessary, to remove the latest migration:

Remove-Migration -Context SamlConfigurationContext -Project DatabaseIdentityProvider -StartupProject DatabaseIdentityProvider

List Migrations
===============

To list the migrations and see which have been applied:

Get-Migration -Context SamlConfigurationContext -Project DatabaseIdentityProvider -StartupProject DatabaseIdentityProvider

Script Migrations
=================

For production environments, it's recommended an SQL script is created, reviewed and run.

Script-Migration -Idempotent -Context SamlConfigurationContext -Project DatabaseIdentityProvider

Model Changes
=============

On any SAML package update, it's recommended to create a migration to pick up any changes to the SAML configuration model. 

If the model hasn't changed, the generated migration will be empty and may be removed.

Otherwise, the database should be updated using the migration.
