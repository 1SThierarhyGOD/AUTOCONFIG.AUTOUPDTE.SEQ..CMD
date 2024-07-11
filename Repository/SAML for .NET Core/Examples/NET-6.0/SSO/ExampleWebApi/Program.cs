using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

const string DefaultCorsPolicyName = "Default CORS Policy";

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((hostBuilderContext, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(hostBuilderContext.Configuration));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // SameSiteMode.None is required to support SAML SSO.
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

// Optionally add support for JWT bearer tokens.
// This is required only if JWT bearer tokens are used to authorize access to a web API.
// It's not required for SAML SSO.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents()
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.ContainsKey(builder.Configuration["JWT:CookieName"]))
                {
                    context.Token = context.Request.Cookies[builder.Configuration["JWT:CookieName"]];
                }

                return Task.CompletedTask;
            },

            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    // Send a signal to the client that the JWT has expired:
                    // - Add a header to the response to indicate the token has expired
                    // - Use it to perform a desired action on the client
                    // This is just an example of one approach to handle this event on the client.
                    context.Response.Headers.Add("token-expired", "true");
                    context.Response.Headers.Add("access-control-expose-headers", "token-expired");
                }

                return Task.CompletedTask;
            }
        };
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Issuer"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
        };
    });

// Optionally add cross-origin request sharing services.
// This is only required for the web API.
// It's not required for SAML SSO.
builder.Services.AddCors(options =>
{
    options.AddPolicy(DefaultCorsPolicyName,
    builder =>
    {
        builder.WithOrigins("http://localhost:4200", "https://localhost:4200");
        builder.AllowAnyMethod();
        builder.AllowAnyHeader();
        builder.AllowCredentials();
    });
});

// Add SAML SSO services.
builder.Services.AddSaml(builder.Configuration.GetSection("SAML"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.UseCors(DefaultCorsPolicyName);

app.MapControllers();

app.Run();
