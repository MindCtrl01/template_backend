using FastEndpoints;
using TemplateBackend.API.Common.Models;
using TemplateBackend.API.Common.Services.Interfaces;
using TemplateBackend.API.Infrastructure.Data;

namespace TemplateBackend.API.Features.Products.DeleteProduct;

/// <summary>
/// FastEndpoint for deleting a product
/// </summary>
public class DeleteProductEndpoint : Endpoint<DeleteProductRequest, ApiResponse<DeleteProductResponse>>
{
    private readonly IProductService _productService;
    private readonly IRepository<Product> _productRepository;

    public DeleteProductEndpoint(
        IProductService productService,
        IRepository<Product> productRepository)
    {
        _productService = productService;
        _productRepository = productRepository;
    }

    public override void Configure()
    {
        Delete("/api/products/{Id}");
        Roles("Admin");
        Summary(s =>
        {
            s.Summary = "Delete a product";
            s.Description = "Deletes a product from the system (soft delete)";
            s.ExampleRequest = new DeleteProductRequest { Id = Guid.NewGuid() };
        });
    }

    public override async Task HandleAsync(DeleteProductRequest req, CancellationToken ct)
    {
        // Check if product exists
        var existingProduct = await _productService.GetByIdAsync(req.Id);
        if (existingProduct == null)
        {
            await SendAsync(ApiResponse<DeleteProductResponse>.ErrorResult("Product not found"), cancellation: ct);
            return;
        }

        // Check if product has active orders or inventory
        if (existingProduct.StockQuantity > 0)
        {
            await SendAsync(ApiResponse<DeleteProductResponse>.ErrorResult("Cannot delete product with existing stock. Please update stock quantity to 0 first."), cancellation: ct);
            return;
        }

        var result = await _productService.DeleteAsync(req.Id);
        if (!result.IsSuccess)
        {
            await SendAsync(ApiResponse<DeleteProductResponse>.ErrorResult(result.ErrorMessage), cancellation: ct);
            return;
        }

        var response = new DeleteProductResponse
        {
            Id = existingProduct.Id,
            Name = existingProduct.Name,
            Sku = existingProduct.Sku,
            IsDeleted = true,
            DeletedAt = DateTime.UtcNow
        };

        await SendAsync(ApiResponse<DeleteProductResponse>.SuccessResult(response, "Product deleted successfully"), cancellation: ct);
    }
} 