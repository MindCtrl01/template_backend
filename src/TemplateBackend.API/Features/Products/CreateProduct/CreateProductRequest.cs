using System.ComponentModel.DataAnnotations;

namespace TemplateBackend.API.Features.Products.CreateProduct;

/// <summary>
/// Request model for creating a new product
/// </summary>
public class CreateProductRequest
{
    /// <summary>
    /// Product name
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Product description
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Product price
    /// </summary>
    [Required]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    /// <summary>
    /// Product SKU (Stock Keeping Unit)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Product category
    /// </summary>
    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>
    /// Product brand
    /// </summary>
    [MaxLength(100)]
    public string? Brand { get; set; }

    /// <summary>
    /// Product image URL
    /// </summary>
    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Product weight in grams
    /// </summary>
    [Range(0, double.MaxValue)]
    public decimal? Weight { get; set; }

    /// <summary>
    /// Product dimensions (length x width x height in cm)
    /// </summary>
    [MaxLength(50)]
    public string? Dimensions { get; set; }

    /// <summary>
    /// Product stock quantity
    /// </summary>
    [Range(0, int.MaxValue)]
    public int StockQuantity { get; set; }

    /// <summary>
    /// Minimum stock level for reorder
    /// </summary>
    [Range(0, int.MaxValue)]
    public int MinStockLevel { get; set; }

    /// <summary>
    /// Product tags for search and categorization
    /// </summary>
    [MaxLength(500)]
    public string? Tags { get; set; }
} 