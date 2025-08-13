using FastEndpoints;
using TemplateBackend.API.Common.Models;
using TemplateBackend.API.Common.Services.Interfaces;
using TemplateBackend.API.Infrastructure.Data;

namespace TemplateBackend.API.Features.Products.GetProductById;

/// <summary>
/// FastEndpoint for getting a product by ID
/// </summary>
public class GetProductByIdEndpoint : Endpoint<GetProductByIdRequest, ApiResponse<GetProductByIdResponse>>
{
    private readonly IProductService _productService;

    public GetProductByIdEndpoint(IProductService productService)
    {
        _productService = productService;
    }

    public override void Configure()
    {
        Get("/api/products/{Id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get product by ID";
            s.Description = "Returns a specific product by its ID";
            s.ExampleRequest = new GetProductByIdRequest { Id = Guid.NewGuid() };
        });
    }

    public override async Task HandleAsync(GetProductByIdRequest req, CancellationToken ct)
    {
        var product = await _productService.GetByIdAsync(req.Id);
        if (product == null)
        {
            await SendAsync(ApiResponse<GetProductByIdResponse>.ErrorResult("Product not found"), cancellation: ct);
            return;
        }

        var response = new GetProductByIdResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Sku = product.Sku,
            Category = product.Category,
            Brand = product.Brand,
            ImageUrl = product.ImageUrl,
            Weight = product.Weight,
            Dimensions = product.Dimensions,
            StockQuantity = product.StockQuantity,
            MinStockLevel = product.MinStockLevel,
            Tags = product.Tags,
            Rating = product.Rating,
            ReviewCount = product.ReviewCount,
            FullName = product.FullName,
            StockStatus = product.StockStatus,
            IsInStock = product.IsInStock,
            NeedsReorder = product.NeedsReorder,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            CreatedBy = product.CreatedBy != null ? new CreatorInfo
            {
                Id = product.CreatedBy.Id,
                FullName = product.CreatedBy.FullName,
                Email = product.CreatedBy.Email ?? string.Empty
            } : null
        };

        await SendAsync(ApiResponse<GetProductByIdResponse>.SuccessResult(response, "Product retrieved successfully"), cancellation: ct);
    }
} 