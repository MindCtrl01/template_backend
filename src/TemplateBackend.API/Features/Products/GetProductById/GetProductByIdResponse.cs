namespace TemplateBackend.API.Features.Products.GetProductById;

/// <summary>
/// Response model for getting a product by ID
/// </summary>
public class GetProductByIdResponse
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
    /// Product description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Product price
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Product SKU
    /// </summary>
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Product category
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Product brand
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// Product image URL
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Product weight in grams
    /// </summary>
    public decimal? Weight { get; set; }

    /// <summary>
    /// Product dimensions
    /// </summary>
    public string? Dimensions { get; set; }

    /// <summary>
    /// Product stock quantity
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Minimum stock level for reorder
    /// </summary>
    public int MinStockLevel { get; set; }

    /// <summary>
    /// Product tags
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Product rating
    /// </summary>
    public decimal? Rating { get; set; }

    /// <summary>
    /// Number of reviews
    /// </summary>
    public int ReviewCount { get; set; }

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
    /// Indicates whether the product needs reordering
    /// </summary>
    public bool NeedsReorder { get; set; }

    /// <summary>
    /// Indicates whether the product is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Date and time when the product was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the product was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Creator information
    /// </summary>
    public CreatorInfo? CreatedBy { get; set; }
}

/// <summary>
/// Creator information
/// </summary>
public class CreatorInfo
{
    /// <summary>
    /// Creator's unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Creator's full name
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Creator's email
    /// </summary>
    public string Email { get; set; } = string.Empty;
} 