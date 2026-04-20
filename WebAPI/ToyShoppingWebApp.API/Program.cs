using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ToyShoppingWebApp.Application.Data;
using ToyShoppingWebApp.Application.Repositories.Implementations;
using ToyShoppingWebApp.Application.Repositories.Interfaces;
using ToyShoppingWebApp.Application.Services.Implementations;
using ToyShoppingWebApp.Application.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 0. LOAD SECRETS FROM ENVIRONMENT VARIABLES
// ==========================================

// Read from environment variables (for local dev and production)
string? connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");
string? jwtSecret = Environment.GetEnvironmentVariable("JwtSettings__Secret") 
    ?? builder.Configuration["JwtSettings:Secret"];

// Validate secrets are provided
if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(jwtSecret))
{
    Console.WriteLine("⚠️  SECRETS NOT FOUND!");
    Console.WriteLine("Set environment variables:");
    Console.WriteLine("  export ConnectionStrings__DefaultConnection=\"your-connection-string\"");
    Console.WriteLine("  export JwtSettings__Secret=\"your-jwt-secret\"");
    throw new InvalidOperationException("Required configuration values are missing. See output above.");
}

Console.WriteLine("✅ Secrets loaded from environment variables");

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

// ==========================================
// 2. BUILD THE APPLICATION
// ==========================================

var app = builder.Build();

// ==========================================
// 3. CONFIGURE MIDDLEWARE PIPELINE (ORDER MATTERS!)
// ==========================================

// Global exception handling should be first
// (we'll add custom middleware later)

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

app.Run();
