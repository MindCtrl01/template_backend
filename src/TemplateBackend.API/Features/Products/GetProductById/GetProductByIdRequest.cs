namespace TemplateBackend.API.Features.Products.GetProductById;

/// <summary>
/// Request model for getting a product by ID
/// </summary>
public class GetProductByIdRequest
{
    /// <summary>
    /// Product's unique identifier
    /// </summary>
    public Guid Id { get; set; }
} 