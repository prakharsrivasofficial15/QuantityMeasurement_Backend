using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuantityMeasurementAPI.Configurations;
using QuantityMeasurementAPI.Services;
using QuantityMeasurementAPI.Services.Auth;
using QuantityMeasurementAPI.Middleware;
using System.Text;

namespace QuantityMeasurementAPI.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add configurations
            services.Configure<JwtConfiguration>(configuration.GetSection("Jwt"));
            services.Configure<GoogleAuthConfiguration>(configuration.GetSection("GoogleAuth"));

            // Add authentication
            services.AddAuthentication("Bearer")
                .AddJwtBearer(options =>
                {
                    var jwtKey = configuration["Jwt:Key"];
                    if (string.IsNullOrEmpty(jwtKey))
                    {
                        // Fallback key for development
                        jwtKey = "ThisIsASecureSecretKeyForJWTTokenGenerationThatIsAtLeast256BitsLong1234567890";
                    }
                    
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)),
                        ValidateIssuer = true,
                        ValidIssuer = configuration["Jwt:Issuer"] ?? "QuantityMeasurementAPI",
                        ValidateAudience = true,
                        ValidAudience = configuration["Jwt:Audience"] ?? "QuantityMeasurementClient",
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            // Add controllers
            services.AddControllers();

            // Add Swagger
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Quantity Measurement API", Version = "v1" });
                
                // Add JWT authentication to Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // Add CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register application services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IGoogleAuthService, GoogleAuthService>();

            return services;
        }

        public static IApplicationBuilder UseApiMiddleware(this IApplicationBuilder app)
        {
            // Use built-in middleware first
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();

            // Use custom middleware last
            app.UseMiddleware<GlobalExceptionHandler>();

            return app;
        }

        public static IApplicationBuilder UseApiDocumentation(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Quantity Measurement API V1");
                c.RoutePrefix = string.Empty;
            });

            return app;
        }
    }
}
