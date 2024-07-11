using IdentityServer;
using IdentityServer4;
using IdentityServerHost.Quickstart.UI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add IdentityServer4.
var identityServerBuilder = builder.Services.AddIdentityServer(options =>
{
    options.Events.RaiseErrorEvents = true;
    options.Events.RaiseInformationEvents = true;
    options.Events.RaiseFailureEvents = true;
    options.Events.RaiseSuccessEvents = true;

    options.EmitStaticAudienceClaim = true;
})
    .AddTestUsers(TestUsers.Users);

identityServerBuilder.AddInMemoryIdentityResources(Config.IdentityResources);
identityServerBuilder.AddInMemoryApiScopes(Config.ApiScopes);
identityServerBuilder.AddInMemoryClients(Config.Clients);

identityServerBuilder.AddDeveloperSigningCredential();

builder.Services.AddControllersWithViews();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // SameSiteMode.None is required to support SAML SSO.
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

// Add SAML SSO services.
builder.Services.AddSaml(builder.Configuration.GetSection("SAML"));

// Add SAML authentication services. This is only required if acting as the SAML service provider.
builder.Services.AddAuthentication().AddSaml(options =>
{
    // Use the identity server authentication scheme for login.
    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
    options.SignOutScheme= IdentityServerConstants.ExternalCookieAuthenticationScheme;

    // Distinguish between the IdP and SP endpoints.
    options.AssertionConsumerServicePath = "/SAML/SP/AssertionConsumerService";
    options.SingleLogoutServicePath = "/SAML/SP/SingleLogoutService";
});

// Add the SAML middleware services. This is only required if acting as the SAML identity provider.
builder.Services.AddSamlMiddleware(options =>
{
    // Use the configured partner service provider name for IdP-initiated SSO.
    options.PartnerName = (httpContext) => builder.Configuration["PartnerName"];

    // The identity server login and logout endpoints.
    options.LoginUrl = (ctx) => "/Account/Login";
    options.LogoutUrl = (ctx) => "/Account/Logout";

    // Distinguish between the IdP and SP endpoints.
    options.SingleSignOnServicePath = "/SAML/IDP/SingleSignOnService";
    options.SingleLogoutServicePath = "/SAML/IDP/SingleLogoutService";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseIdentityServer();
app.UseAuthorization();

// Use SAML middleware. This is only required if acting as the SAML identity provider.
app.UseSaml();

app.UseEndpoints(endpoints =>
{
    endpoints.MapDefaultControllerRoute();
});

app.Run();
