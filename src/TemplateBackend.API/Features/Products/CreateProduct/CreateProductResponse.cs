namespace TemplateBackend.API.Features.Products.CreateProduct;

/// <summary>
/// Response model for creating a new product
/// </summary>
public class CreateProductResponse
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
    /// Product price
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Product category
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Product brand
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// Product's full name with brand
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Product's stock status
    /// </summary>
    public string StockStatus { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the product is in stock
    /// </summary>
    public bool IsInStock { get; set; }

    /// <summary>
    /// Date and time when the product was created
    /// </summary>
    public DateTime CreatedAt { get; set; }
} 