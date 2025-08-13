using Microsoft.EntityFrameworkCore;
using TemplateBackend.API.Infrastructure.Data;
using TemplateBackend.API.Common.Services.Interfaces;

namespace TemplateBackend.API.Common.Services.Implementations;

/// <summary>
/// Product service implementation
/// </summary>
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IUnitOfWork unitOfWork, ILogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _unitOfWork.Repository<Product>().GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get product by ID: {ProductId}", id);
            return null;
        }
    }

    public async Task<(List<Product> Products, int TotalCount)> GetAllAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var skip = (page - 1) * pageSize;
            var products = await _unitOfWork.Repository<Product>().GetAllAsync();
            var totalCount = products.Count;
            var pagedProducts = products.Skip(skip).Take(pageSize).ToList();

            return (pagedProducts, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all products");
            return (new List<Product>(), 0);
        }
    }

    public async Task<ServiceResult> CreateAsync(Product product)
    {
        try
        {
            // Check if SKU already exists
            var existingProducts = await _unitOfWork.Repository<Product>().GetAllAsync();
            if (existingProducts.Any(p => p.Sku == product.Sku))
            {
                return ServiceResult.Failure($"Product with SKU '{product.Sku}' already exists.");
            }

            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Product>().AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Product created successfully: {ProductId}", product.Id);
            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create product");
            return ServiceResult.Failure("Failed to create product.");
        }
    }

    public async Task<ServiceResult> UpdateAsync(Product product)
    {
        try
        {
            var existingProduct = await _unitOfWork.Repository<Product>().GetByIdAsync(product.Id);
            if (existingProduct == null)
            {
                return ServiceResult.Failure("Product not found.");
            }

            // Check if SKU is being changed and if the new SKU already exists
            if (existingProduct.Sku != product.Sku)
            {
                var allProducts = await _unitOfWork.Repository<Product>().GetAllAsync();
                if (allProducts.Any(p => p.Id != product.Id && p.Sku == product.Sku))
                {
                    return ServiceResult.Failure($"Product with SKU '{product.Sku}' already exists.");
                }
            }

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.Sku = product.Sku;
            existingProduct.StockQuantity = product.StockQuantity;
            existingProduct.Weight = product.Weight;
            existingProduct.Dimensions = product.Dimensions;
            existingProduct.MinStockLevel = product.MinStockLevel;
            existingProduct.Tags = product.Tags;
            existingProduct.Rating = product.Rating;
            existingProduct.ReviewCount = product.ReviewCount;
            existingProduct.IsActive = product.IsActive;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Product>().UpdateAsync(existingProduct);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Product updated successfully: {ProductId}", product.Id);
            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update product: {ProductId}", product.Id);
            return ServiceResult.Failure("Failed to update product.");
        }
    }

    public async Task<ServiceResult> DeleteAsync(Guid id)
    {
        try
        {
            var product = await _unitOfWork.Repository<Product>().GetByIdAsync(id);
            if (product == null)
            {
                return ServiceResult.Failure("Product not found.");
            }

            // Check if product has stock
            if (product.StockQuantity > 0)
            {
                return ServiceResult.Failure("Cannot delete product with existing stock.");
            }

            // Soft delete
            product.IsDeleted = true;
            product.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Repository<Product>().UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Product deleted successfully: {ProductId}", id);
            return ServiceResult.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete product: {ProductId}", id);
            return ServiceResult.Failure("Failed to delete product.");
        }
    }

    public async Task<(List<Product> Products, int TotalCount)> SearchAsync(string searchTerm, int page = 1, int pageSize = 10)
    {
        try
        {
            var allProducts = await _unitOfWork.Repository<Product>().GetAllAsync();
            var searchLower = searchTerm.ToLowerInvariant();

            var filteredProducts = allProducts.Where(p =>
                !p.IsDeleted &&
                (p.Name.ToLowerInvariant().Contains(searchLower) ||
                 (p.Description != null && p.Description.ToLowerInvariant().Contains(searchLower)) ||
                 p.Sku.ToLowerInvariant().Contains(searchLower) ||
                 (p.Tags != null && p.Tags.ToLowerInvariant().Contains(searchLower)))
            ).ToList();

            var totalCount = filteredProducts.Count;
            var skip = (page - 1) * pageSize;
            var pagedProducts = filteredProducts.Skip(skip).Take(pageSize).ToList();

            return (pagedProducts, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search products");
            return (new List<Product>(), 0);
        }
    }
} 