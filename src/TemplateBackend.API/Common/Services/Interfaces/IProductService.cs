using TemplateBackend.API.Infrastructure.Data;

namespace TemplateBackend.API.Common.Services.Interfaces;

/// <summary>
/// Product service interface
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Gets a product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product or null if not found</returns>
    Task<Product?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all products with pagination
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Tuple of products and total count</returns>
    Task<(List<Product> Products, int TotalCount)> GetAllAsync(int page = 1, int pageSize = 10);

    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="product">Product to create</param>
    /// <returns>Service result</returns>
    Task<ServiceResult> CreateAsync(Product product);

    /// <summary>
    /// Updates a product
    /// </summary>
    /// <param name="product">Product to update</param>
    /// <returns>Service result</returns>
    Task<ServiceResult> UpdateAsync(Product product);

    /// <summary>
    /// Deletes a product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Service result</returns>
    Task<ServiceResult> DeleteAsync(Guid id);

    /// <summary>
    /// Searches products
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Tuple of products and total count</returns>
    Task<(List<Product> Products, int TotalCount)> SearchAsync(string searchTerm, int page = 1, int pageSize = 10);
}

/// <summary>
/// Service result for operations
/// </summary>
public class ServiceResult
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Error message if operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Success message if operation succeeded
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Creates a successful service result
    /// </summary>
    /// <param name="message">Optional success message</param>
    /// <returns>Successful service result</returns>
    public static ServiceResult Success(string? message = null)
    {
        return new ServiceResult
        {
            IsSuccess = true,
            Message = message
        };
    }

    /// <summary>
    /// Creates a failed service result
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    /// <returns>Failed service result</returns>
    public static ServiceResult Failure(string errorMessage)
    {
        return new ServiceResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
} 