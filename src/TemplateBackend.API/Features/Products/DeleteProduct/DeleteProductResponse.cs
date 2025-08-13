namespace TemplateBackend.API.Features.Products.DeleteProduct;

/// <summary>
/// Response model for deleting a product
/// </summary>
public class DeleteProductResponse
{
    /// <summary>
    /// Product's unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Product name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Product SKU
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the product was successfully deleted
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Date and time when the product was deleted
    /// </summary>
    public DateTime DeletedAt { get; set; }
} 