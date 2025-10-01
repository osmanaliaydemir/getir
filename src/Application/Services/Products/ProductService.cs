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

    // Merchant-specific methods
    public async Task<Result<PagedResult<ProductResponse>>> GetMyProductsAsync(
        Guid merchantOwnerId,
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        // Get merchant owned by this user
        var merchant = await _unitOfWork.ReadRepository<Merchant>()
            .FirstOrDefaultAsync(m => m.OwnerId == merchantOwnerId, cancellationToken: cancellationToken);

        if (merchant == null)
        {
            return Result.Fail<PagedResult<ProductResponse>>("Merchant not found", "NOT_FOUND_MERCHANT");
        }

        var products = await _unitOfWork.Repository<Product>().GetPagedAsync(
            filter: p => p.MerchantId == merchant.Id,
            orderBy: p => p.DisplayOrder,
            ascending: query.IsAscending,
            page: query.Page,
            pageSize: query.PageSize,
            include: "ProductCategory",
            cancellationToken: cancellationToken);

        var total = await _unitOfWork.ReadRepository<Product>()
            .CountAsync(p => p.MerchantId == merchant.Id, cancellationToken);

        var responses = products.Select(p => new ProductResponse(
            p.Id,
            p.MerchantId,
            merchant.Name,
            p.ProductCategoryId,
            p.ProductCategory?.Name,
            p.Name,
            p.Description,
            p.ImageUrl,
            p.Price,
            p.DiscountedPrice,
            p.StockQuantity,
            p.Unit,
            p.IsAvailable)).ToList();

        var pagedResult = PagedResult<ProductResponse>.Create(
            responses,
            total,
            query.Page,
            query.PageSize);

        return Result.Ok(pagedResult);
    }

    public async Task<Result<ProductResponse>> CreateMyProductAsync(
        CreateProductRequest request,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Verify merchant ownership
        var merchant = await _unitOfWork.ReadRepository<Merchant>()
            .FirstOrDefaultAsync(m => m.Id == request.MerchantId && m.OwnerId == merchantOwnerId, 
                cancellationToken: cancellationToken);

        if (merchant == null)
        {
            return Result.Fail<ProductResponse>("Merchant not found or access denied", "NOT_FOUND_MERCHANT");
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
            IsAvailable = true,
            IsActive = true,
            DisplayOrder = 0, // Will be set by merchant
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Product>().AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new ProductResponse(
            product.Id,
            product.MerchantId,
            merchant.Name,
            product.ProductCategoryId,
            null, // Will be loaded if needed
            product.Name,
            product.Description,
            product.ImageUrl,
            product.Price,
            product.DiscountedPrice,
            product.StockQuantity,
            product.Unit,
            product.IsAvailable);

        return Result.Ok(response);
    }

    public async Task<Result<ProductResponse>> UpdateMyProductAsync(
        Guid id,
        UpdateProductRequest request,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var product = await _unitOfWork.ReadRepository<Product>()
            .FirstOrDefaultAsync(p => p.Id == id, 
                include: "Merchant,ProductCategory", 
                cancellationToken: cancellationToken);

        if (product == null)
        {
            return Result.Fail<ProductResponse>("Product not found", "NOT_FOUND_PRODUCT");
        }

        // Verify merchant ownership
        if (product.Merchant.OwnerId != merchantOwnerId)
        {
            return Result.Fail<ProductResponse>("Access denied", "FORBIDDEN_PRODUCT");
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
            product.IsAvailable);

        return Result.Ok(response);
    }

    public async Task<Result> DeleteMyProductAsync(
        Guid id,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.ReadRepository<Product>()
            .FirstOrDefaultAsync(p => p.Id == id, 
                include: "Merchant", 
                cancellationToken: cancellationToken);

        if (product == null)
        {
            return Result.Fail("Product not found", "NOT_FOUND_PRODUCT");
        }

        // Verify merchant ownership
        if (product.Merchant.OwnerId != merchantOwnerId)
        {
            return Result.Fail("Access denied", "FORBIDDEN_PRODUCT");
        }

        // Soft delete
        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Product>().Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result> UpdateProductStockAsync(
        Guid id,
        int newStockQuantity,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.ReadRepository<Product>()
            .FirstOrDefaultAsync(p => p.Id == id, 
                include: "Merchant", 
                cancellationToken: cancellationToken);

        if (product == null)
        {
            return Result.Fail("Product not found", "NOT_FOUND_PRODUCT");
        }

        // Verify merchant ownership
        if (product.Merchant.OwnerId != merchantOwnerId)
        {
            return Result.Fail("Access denied", "FORBIDDEN_PRODUCT");
        }

        product.StockQuantity = newStockQuantity;
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Product>().Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result> ToggleProductAvailabilityAsync(
        Guid id,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.ReadRepository<Product>()
            .FirstOrDefaultAsync(p => p.Id == id, 
                include: "Merchant", 
                cancellationToken: cancellationToken);

        if (product == null)
        {
            return Result.Fail("Product not found", "NOT_FOUND_PRODUCT");
        }

        // Verify merchant ownership
        if (product.Merchant.OwnerId != merchantOwnerId)
        {
            return Result.Fail("Access denied", "FORBIDDEN_PRODUCT");
        }

        product.IsAvailable = !product.IsAvailable;
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Product>().Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result> BulkUpdateProductOrderAsync(
        List<UpdateProductOrderRequest> requests,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requests);

        if (!requests.Any())
        {
            return Result.Fail("No products to update", "EMPTY_REQUEST");
        }

        var productIds = requests.Select(r => r.ProductId).ToList();
        var products = await _unitOfWork.Repository<Product>().ListAsync(
            filter: p => productIds.Contains(p.Id),
            include: "Merchant",
            cancellationToken: cancellationToken);

        if (products.Count != requests.Count)
        {
            return Result.Fail("Some products not found", "NOT_FOUND_PRODUCTS");
        }

        // Verify all products belong to the merchant
        if (products.Any(p => p.Merchant.OwnerId != merchantOwnerId))
        {
            return Result.Fail("Access denied", "FORBIDDEN_PRODUCTS");
        }

        // Update display orders
        foreach (var request in requests)
        {
            var product = products.First(p => p.Id == request.ProductId);
            product.DisplayOrder = request.NewDisplayOrder;
            product.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Repository<Product>().Update(product);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
