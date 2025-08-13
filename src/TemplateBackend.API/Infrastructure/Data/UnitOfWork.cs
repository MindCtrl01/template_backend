using Microsoft.Extensions.DependencyInjection;

namespace TemplateBackend.API.Infrastructure.Data;

/// <summary>
/// Unit of work implementation for managing transactions and coordinating repositories
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ApplicationDbContext _context;

    public UnitOfWork(IServiceProvider serviceProvider, ApplicationDbContext context)
    {
        _serviceProvider = serviceProvider;
        _context = context;
    }

    public IRepository<T> Repository<T>() where T : class
    {
        return _serviceProvider.GetRequiredService<IRepository<T>>();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
} 