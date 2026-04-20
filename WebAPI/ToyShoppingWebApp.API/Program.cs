using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ToyShoppingWebApp.Application.Data;
using ToyShoppingWebApp.Application.Repositories.Implementations;
using ToyShoppingWebApp.Application.Repositories.Interfaces;
using ToyShoppingWebApp.Application.Services.Implementations;
using ToyShoppingWebApp.Application.Services.Interfaces;
using ToyShoppingWebApp.API.Middleware;
using Serilog;
using AspNetCoreRateLimit;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Extensions.AspNetCore.Configuration.Secrets;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// LOAD SECRETS FROM AZURE KEY VAULT
// ==========================================

// Use Key Vault if available (production), otherwise fall back to environment variables
var keyVaultUrl = "https://tej-keyvault.vault.azure.net/";
try
{
    var secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
    builder.Configuration.AddAzureKeyVault(secretClient, new AzureKeyVaultConfigurationOptions());
    Console.WriteLine("✅ Azure Key Vault configuration added successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️  Azure Key Vault not available: {ex.Message}");
    Console.WriteLine("Falling back to environment variables or config files");
}

// ==========================================
// 0. LOAD SECRETS (FROM KEY VAULT → ENV VARS → CONFIG FILES)
// ==========================================

// Priority: Key Vault > Environment Variables > appsettings.json
// Key Vault secret names: ConnectionStrings--DefaultConnection, Jwt--Secret
string? connectionString = builder.Configuration["ConnectionStrings--DefaultConnection"]  // Key Vault uses -- instead of :
    ?? builder.Configuration["ConnectionStrings:DefaultConnection"]
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
string? jwtSecret = builder.Configuration["Jwt--Secret"]  // Key Vault: Jwt--Secret
    ?? builder.Configuration["JwtSettings:Secret"]
    ?? Environment.GetEnvironmentVariable("JwtSettings__Secret");

// Validate secrets are provided
if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(jwtSecret))
{
    Console.WriteLine("❌ SECRETS NOT FOUND!");
    Console.WriteLine("Checked in order:");
    Console.WriteLine("  1. Key Vault: ConnectionStrings--DefaultConnection, Jwt--Secret");
    Console.WriteLine("  2. Environment: ConnectionStrings__DefaultConnection, JwtSettings__Secret");
    Console.WriteLine("  3. Config: ConnectionStrings:DefaultConnection, JwtSettings:Secret");
    throw new InvalidOperationException("Required configuration values are missing.");
}

Console.WriteLine("✅ Secrets loaded successfully from Key Vault, environment, or config");

// ==========================================
// 1. ADD SERVICES TO DEPENDENCY INJECTION CONTAINER
// ==========================================

// Add controllers
builder.Services.AddControllers();

// Add DbContext (Scoped = new instance per HTTP request)
builder.Services.AddDbContext<ToyShoppingDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sqlOptions => sqlOptions.MigrationsAssembly("ToyShoppingWebApp.API"))
);
// How DbContext is Scoped? Does it not required any keyword like AddScoped?
// Answer: AddDbContext<T>() internally registers the DbContext as Scoped by default. This means that a new instance of ToyShoppingDbContext will be created for each HTTP request and shared across all services that require it during that request. You do not need to explicitly call AddScoped<ToyShoppingDbContext>() because AddDbContext<T>() already handles that for you.


// Add repositories and services
builder.Services.AddScoped<IToyRepository, ToyRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

// Add services
builder.Services.AddScoped<IToyService, ToyService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret!)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // No tolerance for expired tokens
    };
});

// Add Authorization
builder.Services.AddAuthorization();

// Add CORS (Cross-Origin Resource Sharing)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200") // Angular dev server
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Add HttpContextAccessor (for accessing user context in services)
builder.Services.AddHttpContextAccessor();

// Add Serilog for structured logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "ToyShoppingWebApp")
    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Add Rate Limiting (DDoS protection)
builder.Services.AddMemoryCache();
builder.Services.AddInMemoryRateLimiting(); builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    options.HttpStatusCode = 429;
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Limit = 100,
            Period = "1m"
        }
    };
});

// ==========================================
// 2. BUILD THE APPLICATION
// ==========================================

var app = builder.Build();

// ==========================================
// 3. CONFIGURE MIDDLEWARE PIPELINE (ORDER MATTERS!)
// ==========================================

// Global exception middleware (MUST BE FIRST)
app.UseMiddleware<GlobalExceptionMiddleware>();

// Security headers middleware
app.UseMiddleware<SecurityHeadersMiddleware>();

// Rate limiting middleware
app.UseIpRateLimiting();

// HTTPS redirection
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Enable CORS
app.UseCors("AllowAngular");

// Routing (must come before MapControllers)
app.UseRouting();

// Authentication (extract JWT claims)
app.UseAuthentication();

// Authorization (check role-based access)
app.UseAuthorization();

// Map controller endpoints
app.MapControllers();

// ==========================================
// 4. RUN THE APPLICATION
// ==========================================

try
{
    Log.Information("🚀 Starting ToyShoppingWebApp API...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Application terminated unexpectedly");
}
finally
{
    Log.Information("🛑 Shutting down ToyShoppingWebApp API");
    Log.CloseAndFlush();
}
