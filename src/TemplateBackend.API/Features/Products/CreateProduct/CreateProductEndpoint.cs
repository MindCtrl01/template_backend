using FastEndpoints;
using TemplateBackend.API.Common.Models;
using TemplateBackend.API.Common.Services.Interfaces;
using TemplateBackend.API.Infrastructure.Data;

namespace TemplateBackend.API.Features.Products.CreateProduct;

/// <summary>
/// FastEndpoint for creating a new product
/// </summary>
public class CreateProductEndpoint : Endpoint<CreateProductRequest, ApiResponse<CreateProductResponse>>
{
    private readonly IProductService _productService;
    private readonly ILogger<CreateProductEndpoint> _logger;

    public CreateProductEndpoint(IProductService productService, ILogger<CreateProductEndpoint> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/api/products");
        Summary(s =>
        {
            s.Summary = "Create product";
            s.Description = "Creates a new product in the system";
        });
    }

    public override async Task HandleAsync(CreateProductRequest req, CancellationToken ct)
    {
        try
        {
            var product = new Product
            {
                Name = req.Name,
                Description = req.Description,
                Price = req.Price,
                Sku = req.Sku,
                StockQuantity = req.StockQuantity,
                Weight = req.Weight,
                Dimensions = req.Dimensions,
                MinStockLevel = req.MinStockLevel,
                Tags = req.Tags,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _productService.CreateAsync(product);
            if (!result.IsSuccess)
            {
                await SendAsync(ApiResponse<CreateProductResponse>.ErrorResult(result.ErrorMessage ?? "Failed to create product"), cancellation: ct);
                return;
            }

            var response = new CreateProductResponse
            {
                Id = product.Id,
                Name = product.Name,
                Sku = product.Sku,
                Price = product.Price,
                CreatedAt = product.CreatedAt
            };

            await SendAsync(ApiResponse<CreateProductResponse>.SuccessResult(response, "Product created successfully"), cancellation: ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create product");
            await SendAsync(ApiResponse<CreateProductResponse>.ErrorResult("Failed to create product"), cancellation: ct);
        }
    }
} 