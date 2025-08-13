using FastEndpoints;
using TemplateBackend.API.Common.Models;
using TemplateBackend.API.Common.Services.Interfaces;
using TemplateBackend.API.Infrastructure.Data;

namespace TemplateBackend.API.Features.Products.SearchProducts;

/// <summary>
/// FastEndpoint for searching products
/// </summary>
public class SearchProductsEndpoint : Endpoint<SearchProductsRequest, ApiResponse<SearchProductsResponse>>
{
    private readonly IProductService _productService;

    public SearchProductsEndpoint(IProductService productService)
    {
        _productService = productService;
    }

    public override void Configure()
    {
        Get("/api/products/search");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Search products";
            s.Description = "Search products with filters and pagination";
            s.ExampleRequest = new SearchProductsRequest
            {
                SearchTerm = "laptop",
                Page = 1,
                PageSize = 10,
                Category = "Electronics",
                MinPrice = 100,
                MaxPrice = 1000,
                SortBy = "Price",
                SortDirection = "asc"
            };
        });
    }

    public override async Task HandleAsync(SearchProductsRequest req, CancellationToken ct)
    {
        // Basic search implementation
        var (products, totalCount) = await _productService.SearchAsync(req.SearchTerm, req.Page, req.PageSize);

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
            Rating = p.Rating,
            ReviewCount = p.ReviewCount,
            FullName = p.FullName,
            CreatedAt = p.CreatedAt
        }).ToList();

        var response = new SearchProductsResponse
        {
            Products = productDtos,
            TotalCount = totalCount,
            Page = req.Page,
            PageSize = req.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / req.PageSize),
            SearchTerm = req.SearchTerm,
            Filters = new SearchFilters
            {
                Category = req.Category,
                Brand = req.Brand,
                MinPrice = req.MinPrice,
                MaxPrice = req.MaxPrice,
                StockStatus = req.StockStatus
            },
            SortInfo = new SortInfo
            {
                SortBy = req.SortBy,
                SortDirection = req.SortDirection
            }
        };

        await SendAsync(ApiResponse<SearchProductsResponse>.SuccessResult(response, "Products search completed"), cancellation: ct);
    }
} 