namespace TemplateBackend.API.Infrastructure.Data;

/// <summary>
/// Unit of work interface for managing transactions and coordinating repositories
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Gets a repository for the specified entity type
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <returns>The repository instance</returns>
    IRepository<T> Repository<T>() where T : class;

    /// <summary>
    /// Commits all changes made within the unit of work
    /// </summary>
    /// <returns>The number of affected rows</returns>
    Task<int> SaveChangesAsync();
} 