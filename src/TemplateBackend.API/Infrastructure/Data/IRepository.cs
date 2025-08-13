using System.Linq.Expressions;

namespace TemplateBackend.API.Infrastructure.Data;

/// <summary>
/// Generic repository interface for common CRUD operations
/// </summary>
/// <typeparam name="T">The entity type</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Gets an entity by its ID
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <returns>The entity if found, null otherwise</returns>
    Task<T?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all entities
    /// </summary>
    /// <returns>All entities</returns>
    Task<List<T>> GetAllAsync();

    /// <summary>
    /// Gets entities with pagination
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated entities</returns>
    Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize);

    /// <summary>
    /// Finds entities based on a predicate
    /// </summary>
    /// <param name="predicate">The predicate to filter entities</param>
    /// <returns>Entities matching the predicate</returns>
    Task<T?> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Finds all entities based on a predicate
    /// </summary>
    /// <param name="predicate">The predicate to filter entities</param>
    /// <returns>All entities matching the predicate</returns>
    Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Adds a new entity
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <returns>The added entity</returns>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Updates an existing entity
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <returns>The updated entity</returns>
    Task<T> UpdateAsync(T entity);

    /// <summary>
    /// Deletes an entity
    /// </summary>
    /// <param name="entity">The entity to delete</param>
    /// <returns>True if deleted successfully, false otherwise</returns>
    Task<bool> DeleteAsync(T entity);

    /// <summary>
    /// Deletes an entity by ID
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <returns>True if deleted successfully, false otherwise</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Checks if an entity exists
    /// </summary>
    /// <param name="id">The entity ID</param>
    /// <returns>True if exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid id);

    /// <summary>
    /// Gets the count of entities
    /// </summary>
    /// <returns>The total count</returns>
    Task<int> CountAsync();
} 