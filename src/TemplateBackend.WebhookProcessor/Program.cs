using Microsoft.EntityFrameworkCore;
using Serilog;
using TemplateBackend.WebhookProcessor.Data;
using TemplateBackend.WebhookProcessor.Services.Interfaces;
using TemplateBackend.WebhookProcessor.Services.Implementations;
using Microsoft.Extensions.Hosting;
using TemplateBackend.WebhookProcessor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/payment-processor-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Services.AddSerilog();

try
{
    Log.Information("Starting Webhook Processor Service");

    // Add web API services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Configure database
    builder.Services.AddDbContext<WebhookDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // Configure settings
    builder.Services.Configure<WebhookProcessorSettings>(
        builder.Configuration.GetSection("WebhookProcessorSettings"));

    // Register services
    builder.Services.AddScoped<IWebhookProcessorService, WebhookProcessorService>();
    builder.Services.AddScoped<IWebhookHandler, StripeWebhookHandler>();
    builder.Services.AddScoped<IWebhookHandler, PayPalWebhookHandler>();

    // Register background service
    builder.Services.AddHostedService<WebhookProcessorBackgroundService>();

    var app = builder.Build();

    // Apply database migrations
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<WebhookDbContext>();
        context.Database.Migrate();
    }

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Webhook Processor Service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
} 