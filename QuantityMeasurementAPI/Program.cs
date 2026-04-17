using Microsoft.EntityFrameworkCore;
using QuantityMeasurementAPI.Data;
using QuantityMeasurementAPI.Extensions;
using BusinessLayer.Interfaces;
using BusinessLayer.Services;
using RepositoryLayer.Interfaces;
using RepositoryLayer.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connStr))
    throw new InvalidOperationException("DefaultConnection is not configured. Set ConnectionStrings__DefaultConnection in Railway Variables.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connStr));

builder.Services.AddMemoryCache();

// Add business layer services
builder.Services.AddScoped<IQuantityMeasurementRepository, QuantityMeasurementDatabaseRepository>();
builder.Services.AddScoped<IQuantityMeasurementService, QuantityMeasurementService>();

// Add API services (Swagger, JWT, CORS, Auth)
builder.Services.AddApiServices(builder.Configuration);

// Add application services (AuthService, JwtService, GoogleAuthService)
builder.Services.AddApplicationServices();

var app = builder.Build();

// Only redirect to HTTPS in production
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

// Mount Routing, CORS, Authentication, Authorization, Exception Handler
app.UseApiMiddleware();

// Mount Swagger UI
app.UseApiDocumentation();

app.MapControllers();

app.MapGet("/", () => Results.Ok(new { 
    message = "API is running!", 
    timestamp = DateTime.UtcNow
}));

app.Run();