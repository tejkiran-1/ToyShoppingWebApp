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
// CONFIGURE AZURE KEY VAULT (PRIORITY 1)
// ==========================================
var keyVaultUrl = "https://tej-keyvault.vault.azure.net/";
try
{
    var credential = new DefaultAzureCredential();
    var secretClient = new SecretClient(new Uri(keyVaultUrl), credential);
    
    // Test connection
    var testSecret = secretClient.GetSecret("ConnectionStrings--DefaultConnection");
    Console.WriteLine("✅ Azure Key Vault connected successfully");
    
    // Add Key Vault to configuration
    builder.Configuration.AddAzureKeyVault(secretClient, new AzureKeyVaultConfigurationOptions());
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️  Could not connect to Azure Key Vault: {ex.Message}");
    Console.WriteLine("Will fall back to environment variables or appsettings.json");
}

// ==========================================
// LOAD SECRETS (Key Vault → Environment → Config)
// ==========================================

// Azure Key Vault converts -- to : automatically
// So "Jwt--Secret" becomes accessible as "Jwt:Secret"
string? connectionString = 
    builder.Configuration["ConnectionStrings:DefaultConnection"]
    ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

string? jwtSecret = 
    builder.Configuration["Jwt:Secret"]  // Key Vault: Jwt--Secret (converts to Jwt:Secret)
    ?? builder.Configuration["JwtSettings:Secret"]
    ?? Environment.GetEnvironmentVariable("JwtSettings__Secret");

// Validate
if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("❌ Connection string not found!");
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required");
}

if (string.IsNullOrEmpty(jwtSecret))
{
    Console.WriteLine("❌ JWT secret not found!");
    throw new InvalidOperationException("Jwt:Secret is required");
}

Console.WriteLine("✅ Secrets loaded successfully");

// ==========================================
// 1. ADD SERVICES
// ==========================================

builder.Services.AddControllers();

builder.Services.AddDbContext<ToyShoppingDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sqlOptions => sqlOptions.MigrationsAssembly("ToyShoppingWebApp.API"))
);

builder.Services.AddScoped<IToyRepository, ToyRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

builder.Services.AddScoped<IToyService, ToyService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

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
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "ToyShoppingApp",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? "ToyShoppingAppUsers",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddHttpContextAccessor();

// Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "ToyShoppingWebApp")
    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    options.HttpStatusCode = 429;
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule { Endpoint = "*", Limit = 100, Period = "1m" }
    };
});

// ==========================================
// 2. BUILD APP
// ==========================================

var app = builder.Build();

// ==========================================
// 3. MIDDLEWARE PIPELINE
// ==========================================

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseIpRateLimiting();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAngular");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ==========================================
// 4. RUN
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
