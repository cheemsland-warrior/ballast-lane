using API.Extensions;
//using Application.Core;
//using Application.Core.Services.Implementations.OrderServices;
//using Application.Core.Services.Interfaces.OrderServices;
using AutoMapper;
//using Domain.Models.AccountModels.AppUserModels;
//using Domain.Models.AccountModels.JWT;
// using HealthChecks.UI.Client; // removed: HealthChecks UI not used in simplified setup
using Microsoft.Extensions.FileProviders;
using Microsoft.EntityFrameworkCore;
using BallastLaneApi.Data;
using BallastLaneApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
//using Persistence.Contexts;
//using Persistence.Seeds;
using Serilog;
using Serilog.Events;
using System.IdentityModel.Tokens.Jwt;

// ==========================================
// 1. BOOTSTRAP CONFIGURATION & EARLY LOGGING
// ==========================================
var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>(optional: true, reloadOnChange: true)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    Log.Information("Starting web host");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for application logging
    builder.Host.UseSerilog();

    // ==========================================
    // 2. CONFIGURE SERVICES (Dependency Injection)
    // ==========================================
    const string myAllowSpecificOrigins = "_myAllowSpecificOrigins";

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(myAllowSpecificOrigins, corsBuilder =>
        {
            corsBuilder.WithOrigins("https://localhost:4200", "http://localhost:4200")
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials();
        });
    });

    //builder.Services.AddCookiesPolicyExtension();

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.NumberHandling =
                System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;
        });

    
    builder.Services.AddAutoMapperWithValidation(true);

 
    builder.Services.AddSwaggerExtension();
    // Configure Entity Framework and application services
    var conn = builder.Configuration.GetConnectionString("DefaultConnection")
               ?? "Host=localhost;Port=5432;Database=yourappdb;Username=postgres;Password=Secret";
    builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(conn));
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IPotholeService, PotholeService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    // JWT Authentication
    var jwtKey = builder.Configuration["Jwt:Key"] ?? "please-change-this-secret-in-production";
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true
            };
        });
    builder.Services.AddAuthorization();
    JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();


    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        using var validationScope = app.Services.CreateScope();
        var mapper = validationScope.ServiceProvider.GetRequiredService<IMapper>();
        try
        {
            mapper.ConfigurationProvider.CompileMappings();
            Console.WriteLine("✓ AutoMapper configuration validated successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ AutoMapper configuration validation failed: {ex.Message}");
            throw;
        }
    }

    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        try
        {
            //var storeContext = services.GetRequiredService<StoreContext>();
            //await storeContext.Database.MigrateAsync();
            //await StoreContextSeed.SeedAsync(storeContext, loggerFactory);

            //var userManager = services.GetRequiredService<UserManager<AppUser>>();
            //var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
            //await StoreContextUsersSeed.SeedUsersAsync(userManager, roleManager);
        }
        catch (Exception exception)
        {
            var logger = loggerFactory.CreateLogger("Program");
            logger.LogError(exception, "An error occurred during migration");
        }
    }

    // ==========================================
    // 4. CONFIGURE HTTP PIPELINE (Middleware)
    // ==========================================
    app.UseCookiePolicy();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwaggerDocumentation();

        
    }
    else
    {
        app.Use(async (context, next) =>
        {
            context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000");
            await next.Invoke();
        });
    }

    app.UseStatusCodePagesWithReExecute("/errors/{0}");
    app.UseHttpsRedirection();
   
    app.UseSerilogRequestLogging(options =>
    {
        options.GetLevel = (httpContext, elapsed, ex) => LogEventLevel.Information;
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        };
    });

    app.UseRouting();
    //app.AddSecurityExtension();
    app.UseCors(myAllowSpecificOrigins);

    app.UseAuthentication();
    app.UseAuthorization();

    Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

    // Health checks UI removed for simplified local setup.
    // If you want a basic readiness endpoint, uncomment below:
    // app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

    app.MapControllers().RequireCors(myAllowSpecificOrigins);
    app.MapFallbackToController("Index", "Fallback").RequireCors(myAllowSpecificOrigins);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}