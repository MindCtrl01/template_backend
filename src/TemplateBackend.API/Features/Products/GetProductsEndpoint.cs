using FastEndpoints;
using TemplateBackend.API.Common.Models;
using TemplateBackend.API.Common.Services.Interfaces;
using TemplateBackend.API.Infrastructure.Data;

namespace TemplateBackend.API.Features.Products;

/// <summary>
/// Request model for getting products
/// </summary>
public class GetProductsRequest
{
    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Search term for filtering products
    /// </summary>
    public string? SearchTerm { get; set; }
}

/// <summary>
/// Response model for products
/// </summary>
public class GetProductsResponse
{
    /// <summary>
    /// List of products
    /// </summary>
    public List<ProductDto> Products { get; set; } = new();

    /// <summary>
    /// Total count of products
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
}

/// <summary>
/// Product DTO
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
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// FastEndpoint for getting products
/// </summary>
public class GetProductsEndpoint : Endpoint<GetProductsRequest, ApiResponse<GetProductsResponse>>
{
    private readonly IProductService _productService;

    public GetProductsEndpoint(IProductService productService)
    {
        _productService = productService;
    }

    public override void Configure()
    {
        Get("/api/products");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get all products";
            s.Description = "Returns a paginated list of products";
        });
    }

    public override async Task HandleAsync(GetProductsRequest req, CancellationToken ct)
    {
        var (products, totalCount) = await _productService.GetAllAsync(req.Page, req.PageSize);

        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Sku = p.Sku,
            Category = p.Category,
            Brand = p.Brand,
            ImageUrl = p.ImageUrl,
            StockQuantity = p.StockQuantity,
            StockStatus = p.StockStatus,
            IsInStock = p.IsInStock,
            CreatedAt = p.CreatedAt
        }).ToList();

        var response = new GetProductsResponse
        {
            Products = productDtos,
            TotalCount = totalCount,
            Page = req.Page,
            PageSize = req.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / req.PageSize)
        };

        await SendAsync(ApiResponse<GetProductsResponse>.SuccessResult(response, "Products retrieved successfully"), cancellation: ct);
    }
} 