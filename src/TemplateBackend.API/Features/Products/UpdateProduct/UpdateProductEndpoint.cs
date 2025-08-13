using FastEndpoints;
using TemplateBackend.API.Common.Models;
using TemplateBackend.API.Common.Services.Interfaces;
using TemplateBackend.API.Infrastructure.Data;

namespace TemplateBackend.API.Features.Products.UpdateProduct;

/// <summary>
/// FastEndpoint for updating a product
/// </summary>
public class UpdateProductEndpoint : Endpoint<UpdateProductRequest, ApiResponse<UpdateProductResponse>>
{
    private readonly IProductService _productService;
    private readonly IRepository<Product> _productRepository;

    public UpdateProductEndpoint(
        IProductService productService,
        IRepository<Product> productRepository)
    {
        _productService = productService;
        _productRepository = productRepository;
    }

    public override void Configure()
    {
        Put("/api/products/{Id}");
        Roles("Admin", "Manager");
        Summary(s =>
        {
            s.Summary = "Update a product";
            s.Description = "Updates an existing product in the system";
            s.ExampleRequest = new UpdateProductRequest
            {
                Id = Guid.NewGuid(),
                Name = "Updated Product Name",
                Description = "Updated product description",
                Price = 149.99m,
                Category = "Electronics",
                Brand = "Updated Brand",
                StockQuantity = 50,
                MinStockLevel = 5,
                IsActive = true
            };
        });
    }

    public override async Task HandleAsync(UpdateProductRequest req, CancellationToken ct)
    {
        // Check if product exists
        var existingProduct = await _productService.GetByIdAsync(req.Id);
        if (existingProduct == null)
        {
            await SendAsync(ApiResponse<UpdateProductResponse>.ErrorResult("Product not found"), cancellation: ct);
            return;
        }

        // Update product properties
        existingProduct.Name = req.Name;
        existingProduct.Description = req.Description;
        existingProduct.Price = req.Price;
        existingProduct.Category = req.Category;
        existingProduct.Brand = req.Brand;
        existingProduct.ImageUrl = req.ImageUrl;
        existingProduct.Weight = req.Weight;
        existingProduct.Dimensions = req.Dimensions;
        existingProduct.StockQuantity = req.StockQuantity;
        existingProduct.MinStockLevel = req.MinStockLevel;
        existingProduct.Tags = req.Tags;
        existingProduct.Rating = req.Rating;
        existingProduct.ReviewCount = req.ReviewCount;
        existingProduct.IsActive = req.IsActive;

        var result = await _productService.UpdateAsync(existingProduct);
        if (!result.IsSuccess)
        {
            await SendAsync(ApiResponse<UpdateProductResponse>.ErrorResult(result.ErrorMessage), cancellation: ct);
            return;
        }

        var response = new UpdateProductResponse
        {
            Id = existingProduct.Id,
            Name = existingProduct.Name,
            Description = existingProduct.Description,
            Price = existingProduct.Price,
            Sku = existingProduct.Sku,
            Category = existingProduct.Category,
            Brand = existingProduct.Brand,
            ImageUrl = existingProduct.ImageUrl,
            Weight = existingProduct.Weight,
            Dimensions = existingProduct.Dimensions,
            StockQuantity = existingProduct.StockQuantity,
            MinStockLevel = existingProduct.MinStockLevel,
            Tags = existingProduct.Tags,
            Rating = existingProduct.Rating,
            ReviewCount = existingProduct.ReviewCount,
            FullName = existingProduct.FullName,
            StockStatus = existingProduct.StockStatus,
            IsInStock = existingProduct.IsInStock,
            NeedsReorder = existingProduct.NeedsReorder,
            IsActive = existingProduct.IsActive,
            UpdatedAt = existingProduct.UpdatedAt
        };

        await SendAsync(ApiResponse<UpdateProductResponse>.SuccessResult(response, "Product updated successfully"), cancellation: ct);
    }
} 