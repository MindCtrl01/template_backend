using TemplateBackend.API.Infrastructure.Identity;

namespace TemplateBackend.API.Infrastructure.Data;

/// <summary>
/// Product entity representing items in the system
/// </summary>
public class Product : IAuditableEntity
{
    /// <summary>
    /// Unique identifier for the product
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
    /// Product SKU (Stock Keeping Unit)
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
    /// Product dimensions (length x width x height in cm)
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
    /// Product tags for search and categorization
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Product rating (1-5 stars)
    /// </summary>
    public decimal? Rating { get; set; }

    /// <summary>
    /// Number of reviews for the product
    /// </summary>
    public int ReviewCount { get; set; }

    /// <summary>
    /// Indicates whether the product is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Date and time when the product was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Date and time when the product was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Indicates whether the product is deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// ID of the user who created the product
    /// </summary>
    public Guid CreatedById { get; set; }

    /// <summary>
    /// Navigation property to the user who created the product
    /// </summary>
    public virtual User? CreatedBy { get; set; }

    /// <summary>
    /// Product's full name with brand
    /// </summary>
    public string FullName => !string.IsNullOrEmpty(Brand) ? $"{Brand} - {Name}" : Name;

    /// <summary>
    /// Product's stock status
    /// </summary>
    public string StockStatus
    {
        get
        {
            if (StockQuantity <= 0)
                return "Out of Stock";
            if (StockQuantity <= MinStockLevel)
                return "Low Stock";
            return "In Stock";
        }
    }

    /// <summary>
    /// Indicates whether the product is in stock
    /// </summary>
    public bool IsInStock => StockQuantity > 0;

    /// <summary>
    /// Indicates whether the product needs reordering
    /// </summary>
    public bool NeedsReorder => StockQuantity <= MinStockLevel;
} 