using TemplateBackend.API.Infrastructure.MongoDB;

namespace TemplateBackend.API.Infrastructure.MongoDB;

/// <summary>
/// Generic MongoDB service interface
/// </summary>
/// <typeparam name="T">Document type</typeparam>
public interface IMongoDBService<T> where T : class
{
    /// <summary>
    /// Creates a single document
    /// </summary>
    /// <param name="document">Document to create</param>
    /// <returns>Created document</returns>
    Task<T> CreateAsync(T document);

    /// <summary>
    /// Creates multiple documents in bulk
    /// </summary>
    /// <param name="documents">Documents to create</param>
    /// <returns>Created documents</returns>
    Task<List<T>> CreateManyAsync(IEnumerable<T> documents);

    /// <summary>
    /// Gets a document by ID
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <returns>Document or null if not found</returns>
    Task<T?> GetByIdAsync(string id);

    /// <summary>
    /// Gets all documents with pagination
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Documents and total count</returns>
    Task<List<T>> GetAllAsync(int page = 1, int pageSize = 10);


    /// <summary>
    /// Gets all documents with pagination
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Documents and total count</returns>
    Task<(List<T> Documents, int TotalCount)> GetAllWithTotalAsync(int page = 1, int pageSize = 10);
    /// <summary>
    /// Updates a document by ID
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <param name="document">Updated document</param>
    /// <returns>Updated document or null if not found</returns>
    Task<T?> UpdateAsync(string id, T document);

    /// <summary>
    /// Deletes a document by ID
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteAsync(string id);

    /// <summary>
    /// Finds documents with optional filtering and sorting
    /// </summary>
    /// <param name="filter">Optional filter function</param>
    /// <param name="sort">Optional sort function</param>
    /// <param name="limit">Maximum number of documents to return</param>
    /// <returns>List of documents</returns>
    Task<List<T>> FindAsync(Func<T, bool>? filter = null, Func<T, object>? sort = null, int limit = 50);

    /// <summary>
    /// Counts documents with optional filtering
    /// </summary>
    /// <param name="filter">Optional filter function</param>
    /// <returns>Number of documents</returns>
    Task<long> CountAsync(Func<T, bool>? filter = null);

    /// <summary>
    /// Checks if a document exists by ID
    /// </summary>
    /// <param name="id">Document ID</param>
    /// <returns>True if document exists</returns>
    Task<bool> ExistsAsync(string id);
} 