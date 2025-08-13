namespace TemplateBackend.API.Features.Products.DeleteProduct;

/// <summary>
/// Request model for deleting a product
/// </summary>
public class DeleteProductRequest
{
    /// <summary>
    /// Product's unique identifier
    /// </summary>
    public Guid Id { get; set; }
} 