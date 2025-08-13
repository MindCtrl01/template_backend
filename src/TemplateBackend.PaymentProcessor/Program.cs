using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using TemplateBackend.API.Common.Interfaces;
using TemplateBackend.API.Common.Implementations;
using TemplateBackend.API.Common.Services.Interfaces;
using TemplateBackend.API.Common.Services.Implementations;
using TemplateBackend.API.Infrastructure.MongoDB;
using TemplateBackend.PaymentProcessor;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/payment-processor-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Services.AddSerilog();

// Configure services
builder.Services.Configure<PaymentSettings>(builder.Configuration.GetSection("PaymentSettings"));
builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("KafkaSettings"));
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDBSettings"));

// Add MongoDB services
builder.Services.AddScoped(typeof(IMongoDBService<>), typeof(MongoDBService<>));

// Add message queue
builder.Services.AddSingleton<IMessageQueue, KafkaMessageQueue>();

// Add payment services
builder.Services.AddScoped<IPaymentErrorService, PaymentErrorService>();
builder.Services.AddScoped<PaymentQueueService>();

// Add payment processor service
builder.Services.AddHostedService<PaymentProcessorService>();

var host = builder.Build();

Log.Information("Payment Processor Service starting...");

try
{
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Payment Processor Service failed to start");
}
finally
{
    Log.CloseAndFlush();
} 