using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;

namespace Getir.Application.Services.Products;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<ProductResponse>>> GetProductsByMerchantAsync(
        Guid merchantId,
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        var products = await _unitOfWork.Repository<Product>().GetPagedAsync(
            filter: p => p.MerchantId == merchantId && p.IsActive,
            orderBy: p => p.DisplayOrder,
            ascending: true,
            page: query.Page,
            pageSize: query.PageSize,
            include: "Merchant",
            cancellationToken: cancellationToken);

        var total = await _unitOfWork.ReadRepository<Product>()
            .CountAsync(p => p.MerchantId == merchantId && p.IsActive, cancellationToken);

        var response = products.Select(p => new ProductResponse(
            p.Id,
            p.MerchantId,
            p.Merchant.Name,
            p.ProductCategoryId,
            p.ProductCategory?.Name,
            p.Name,
            p.Description,
            p.ImageUrl,
            p.Price,
            p.DiscountedPrice,
            p.StockQuantity,
            p.Unit,
            p.IsAvailable
        )).ToList();

        var pagedResult = PagedResult<ProductResponse>.Create(response, total, query.Page, query.PageSize);
        
        return Result.Ok(pagedResult);
    }

    public async Task<Result<ProductResponse>> GetProductByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Repository<Product>()
            .GetAsync(p => p.Id == id, include: "Merchant", cancellationToken: cancellationToken);

        if (product == null)
        {
            return Result.Fail<ProductResponse>("Product not found", "NOT_FOUND_PRODUCT");
        }

        var response = new ProductResponse(
            product.Id,
            product.MerchantId,
            product.Merchant.Name,
            product.ProductCategoryId,
            product.ProductCategory?.Name,
            product.Name,
            product.Description,
            product.ImageUrl,
            product.Price,
            product.DiscountedPrice,
            product.StockQuantity,
            product.Unit,
            product.IsAvailable
        );

        return Result.Ok(response);
    }

    public async Task<Result<ProductResponse>> CreateProductAsync(
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var merchantExists = await _unitOfWork.ReadRepository<Merchant>()
            .AnyAsync(m => m.Id == request.MerchantId, cancellationToken);

        if (!merchantExists)
        {
            return Result.Fail<ProductResponse>("Merchant not found", "NOT_FOUND_MERCHANT");
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            MerchantId = request.MerchantId,
            ProductCategoryId = request.ProductCategoryId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            DiscountedPrice = request.DiscountedPrice,
            StockQuantity = request.StockQuantity,
            Unit = request.Unit,
            IsAvailable = request.StockQuantity > 0,
            IsActive = true,
            DisplayOrder = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Product>().AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var createdProduct = await _unitOfWork.Repository<Product>()
            .GetAsync(p => p.Id == product.Id, include: "Merchant", cancellationToken: cancellationToken);

        var response = new ProductResponse(
            createdProduct!.Id,
            createdProduct.MerchantId,
            createdProduct.Merchant.Name,
            createdProduct.ProductCategoryId,
            createdProduct.ProductCategory?.Name,
            createdProduct.Name,
            createdProduct.Description,
            createdProduct.ImageUrl,
            createdProduct.Price,
            createdProduct.DiscountedPrice,
            createdProduct.StockQuantity,
            createdProduct.Unit,
            createdProduct.IsAvailable
        );

        return Result.Ok(response);
    }

    public async Task<Result<ProductResponse>> UpdateProductAsync(
        Guid id,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Repository<Product>()
            .GetByIdAsync(id, cancellationToken);

        if (product == null)
        {
            return Result.Fail<ProductResponse>("Product not found", "NOT_FOUND_PRODUCT");
        }

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.DiscountedPrice = request.DiscountedPrice;
        product.StockQuantity = request.StockQuantity;
        product.Unit = request.Unit;
        product.IsAvailable = request.IsAvailable;
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Product>().Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updatedProduct = await _unitOfWork.Repository<Product>()
            .GetAsync(p => p.Id == id, include: "Merchant", cancellationToken: cancellationToken);

        var response = new ProductResponse(
            updatedProduct!.Id,
            updatedProduct.MerchantId,
            updatedProduct.Merchant.Name,
            updatedProduct.ProductCategoryId,
            updatedProduct.ProductCategory?.Name,
            updatedProduct.Name,
            updatedProduct.Description,
            updatedProduct.ImageUrl,
            updatedProduct.Price,
            updatedProduct.DiscountedPrice,
            updatedProduct.StockQuantity,
            updatedProduct.Unit,
            updatedProduct.IsAvailable
        );

        return Result.Ok(response);
    }

    public async Task<Result> DeleteProductAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Repository<Product>()
            .GetByIdAsync(id, cancellationToken);

        if (product == null)
        {
            return Result.Fail("Product not found", "NOT_FOUND_PRODUCT");
        }

        // Soft delete
        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Product>().Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
