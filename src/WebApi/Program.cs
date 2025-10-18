// Third-party namespaces
using AspNetCoreRateLimit;
using FluentValidation;
using Serilog;
using Microsoft.EntityFrameworkCore;

// Application namespaces
using Getir.Application.Common;
using Getir.Application.DTO;

// Infrastructure namespaces
using Getir.Infrastructure.Persistence;

// WebApi namespaces
using Getir.WebApi.Configuration;
using Getir.WebApi.Extensions;
using Getir.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ============= SERILOG =============
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// ============= DATABASE =============
// Skip DbContext registration in Testing environment (handled by test setup)
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3, // Retry sayısını azalttık
                    maxRetryDelay: TimeSpan.FromSeconds(10), // Retry delay'i azalttık
                    errorNumbersToAdd: null);
                
                // Connection pooling optimization
                sqlOptions.CommandTimeout(ApplicationConstants.DatabaseCommandTimeoutSeconds);
        });

    // Query optimization
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    options.EnableServiceProviderCaching();
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableDetailedErrors();
        options.EnableSensitiveDataLogging();
    }
    });
}

// ============= DEPENDENCY INJECTION =============
// Infrastructure Services (repositories, security, caching, file storage)
builder.Services.AddInfrastructureServices();

// Application Services (business logic)
builder.Services.AddApplicationServices();

// SignalR Services (real-time communication)
builder.Services.AddSignalRServices();

// Application Insights (temporarily disabled)
// builder.Services.AddApplicationInsightsConfiguration(builder.Configuration);

// CSRF Protection
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.SuppressXFrameOptionsHeader = false;
});

// File Upload Settings
builder.Services.Configure<FileUploadSettings>(builder.Configuration.GetSection("FileUpload"));

// ============= SERVER CONFIGURATION =============
// Kestrel Server: Request size limit (prevents memory buffering)
builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = ApplicationConstants.MaxRequestSizeBytes; // 10MB
});

// IIS Server: Request size limit
builder.Services.Configure<Microsoft.AspNetCore.Builder.IISServerOptions>(options =>
{
    options.MaxRequestBodySize = ApplicationConstants.MaxRequestSizeBytes; // 10MB
});

// Form Options: Multipart form data limit (file uploads)
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = ApplicationConstants.MaxRequestSizeBytes; // 10MB
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

// ============= CONFIGURATION =============
// Add Controllers with JSON options (camelCase for mobile compatibility)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = false; // Production performance
    });

builder.Services.AddAuthConfiguration(builder.Configuration);
builder.Services.AddApiKeyConfiguration(builder.Configuration);
builder.Services.AddHealthChecksConfiguration(builder.Configuration);

// Health check services
builder.Services.AddHttpClient<Getir.WebApi.HealthChecks.ExternalApiHealthCheck>();
builder.Services.AddVersioningConfiguration();
builder.Services.AddValidationConfiguration();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddRateLimitConfiguration(builder.Configuration);
builder.Services.AddMetricsConfiguration();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddValidatorsFromAssemblyContaining<Getir.Application.Validators.CreateSpecialHolidayRequestValidator>();

// SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(ApplicationConstants.SignalRClientTimeoutSeconds);
});

// CORS for SignalR
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRCorsPolicy", policy =>
    {
        policy.SetIsOriginAllowed(_ => true) // Allow all origins for SignalR WebSocket
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ============= BUILD APP =============
var app = builder.Build();

// ============= MIDDLEWARE PIPELINE =============
// Order is important! Exception handler MUST be first to catch all errors
app.UseMiddleware<GlobalExceptionMiddleware>();  // 1. Global exception handler (ApiResponse format)
app.UseMiddleware<RequestIdMiddleware>();        // 2. Request ID tracking

app.UseSerilogRequestLogging();

app.UseMiddleware<ValidationMiddleware>();       // 3. Validation (after logging)

// Skip SecurityAuditMiddleware in Testing environment (requires Session)
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseMiddleware<SecurityAuditMiddleware>();
}

// Skip rate limiting in Testing environment
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseMiddleware<RateLimitMiddleware>();
    app.UseIpRateLimiting();
    app.UseClientRateLimiting();
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwaggerConfiguration();

// ============= SECURITY =============
app.UseHttpsRedirection();

// Request size limiting is now handled by Kestrel/IIS native configuration
// See builder.Services.Configure<KestrelServerOptions> above
// This prevents memory buffering and provides better performance

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:;";
    context.Response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
    context.Response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";
    context.Response.Headers["Cross-Origin-Embedder-Policy"] = "require-corp";
    context.Response.Headers["Cross-Origin-Opener-Policy"] = "same-origin";
    context.Response.Headers["Cross-Origin-Resource-Policy"] = "same-origin";
    
    await next();
});

// CORS for SignalR
app.UseCors("SignalRCorsPolicy");


app.UseAuthentication();
app.UseAuthorization();

// ============= CONTROLLERS =============
app.MapControllers();

// ============= SIGNALR HUBS =============
app.MapHub<Getir.WebApi.Hubs.NotificationHub>("/hubs/notifications");
app.MapHub<Getir.WebApi.Hubs.OrderHub>("/hubs/orders");
app.MapHub<Getir.WebApi.Hubs.CourierHub>("/hubs/courier");
app.MapHub<Getir.WebApi.Hubs.RealtimeTrackingHub>("/hubs/realtime-tracking");

app.UseHealthChecksConfiguration();
app.UseMetricsConfiguration();

// ============= RUN =============
try
{
    Log.Information("Starting Getir API...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible for integration tests
public partial class Program { }
