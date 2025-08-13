namespace TemplateBackend.API.Features.Products.SearchProducts;

/// <summary>
/// Request model for searching products
/// </summary>
public class SearchProductsRequest
{
    /// <summary>
    /// Search term to look for in product name, description, SKU, category, brand, or tags
    /// </summary>
    public string SearchTerm { get; set; } = string.Empty;

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Filter by category
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Filter by brand
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// Minimum price filter
    /// </summary>
    public decimal? MinPrice { get; set; }

    /// <summary>
    /// Maximum price filter
    /// </summary>
    public decimal? MaxPrice { get; set; }

    /// <summary>
    /// Filter by stock status (In Stock, Low Stock, Out of Stock)
    /// </summary>
    public string? StockStatus { get; set; }

    /// <summary>
    /// Sort by field (Name, Price, CreatedAt, Rating)
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort direction (asc, desc)
    /// </summary>
    public string SortDirection { get; set; } = "asc";
} 