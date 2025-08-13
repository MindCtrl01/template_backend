using Microsoft.Extensions.DependencyInjection;
using TemplateBackend.API.Common.Services.Interfaces;
using TemplateBackend.API.Common.Services.Implementations;
using TemplateBackend.API.Infrastructure.Data;

namespace TemplateBackend.API.Common.Extensions;

/// <summary>
/// Extension methods for IServiceCollection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds application services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Add repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Add services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IEmailMongoService, EmailMongoService>();
        services.AddScoped<IOTPService, OTPService>();
        services.AddScoped<IPaymentService, PaymentQueueService>();
        services.AddScoped<IPaymentErrorService, PaymentErrorService>();

        // Add PayPal service
        services.AddHttpClient<IPayPalService, PayPalService>();
        services.AddScoped<IPayPalService, PayPalService>();

        return services;
    }
} 