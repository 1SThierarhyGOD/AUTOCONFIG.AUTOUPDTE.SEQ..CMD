using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiddlewareServiceProvider.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((hostBuilderContext, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(hostBuilderContext.Configuration));

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // SameSiteMode.None is required to support SAML SSO.
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    // Use a unique identity cookie name rather than sharing the cookie across applications in the domain.
    options.Cookie.Name = "MiddlewareServiceProvider.Identity";

    // SameSiteMode.None is required to support SAML logout.
    options.Cookie.SameSite = SameSiteMode.None;
});

// Add SAML SSO services.
builder.Services.AddSaml(builder.Configuration.GetSection("SAML"));

// Add SAML authentication services.
builder.Services.AddAuthentication().AddSaml(options =>
{
    options.PartnerName = (httpContext) => builder.Configuration["PartnerName"];
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCookiePolicy(); 
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
