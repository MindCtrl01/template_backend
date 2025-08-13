namespace TemplateBackend.API.Features.Products.SearchProducts;

/// <summary>
/// Response model for searching products
/// </summary>
public class SearchProductsResponse
{
    /// <summary>
    /// List of products matching the search criteria
    /// </summary>
    public List<ProductDto> Products { get; set; } = new();

    /// <summary>
    /// Total count of products matching the search criteria
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Search term used
    /// </summary>
    public string SearchTerm { get; set; } = string.Empty;

    /// <summary>
    /// Applied filters
    /// </summary>
    public SearchFilters Filters { get; set; } = new();

    /// <summary>
    /// Sorting information
    /// </summary>
    public SortInfo SortInfo { get; set; } = new();
}

/// <summary>
/// Product DTO for search results
/// </summary>
public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public string? ImageUrl { get; set; }
    public int StockQuantity { get; set; }
    public string StockStatus { get; set; } = string.Empty;
    public bool IsInStock { get; set; }
    public decimal? Rating { get; set; }
    public int ReviewCount { get; set; }
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Search filters applied
/// </summary>
public class SearchFilters
{
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? StockStatus { get; set; }
}

/// <summary>
/// Sorting information
/// </summary>
public class SortInfo
{
    public string? SortBy { get; set; }
    public string SortDirection { get; set; } = "asc";
} 