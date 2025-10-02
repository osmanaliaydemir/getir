# 🔄 Code Duplication Fixes

## 📋 Genel Bakış

Bu dokümantasyon, Getir API projesindeki code duplication problemlerini çözmek için detaylı implementation guide'ı sağlar.

## 🎯 Tespit Edilen Duplication'lar

### 1. **DTO Property Tekrarları**

#### **1.1 Base Entity Properties**

**Problem**: Tüm entity response'larında benzer properties tekrarlanıyor.

**Mevcut Durum**:
```csharp
// ProductResponse
public record ProductResponse(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    // ... product-specific properties
);

// MerchantResponse
public record MerchantResponse(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    // ... merchant-specific properties
);
```

**Çözüm**: Base DTO'lar oluşturun.

**Implementation**:

```csharp
// src/Application/DTO/CommonDtos.cs
namespace Getir.Application.DTO;

/// <summary>
/// Base response for all entities with common properties
/// </summary>
public record BaseEntityResponse(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// Base response for entities with status information
/// </summary>
public record BaseStatusEntityResponse : BaseEntityResponse
{
    public bool IsActive { get; init; }
    public bool IsDeleted { get; init; }
}

/// <summary>
/// Base response for entities with rating information
/// </summary>
public record BaseRatedEntityResponse : BaseStatusEntityResponse
{
    public decimal? Rating { get; init; }
    public int TotalReviews { get; init; }
}

/// <summary>
/// Base response for paginated results
/// </summary>
public record BasePagedResponse<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
```

**Güncellenmiş DTO'lar**:

```csharp
// src/Application/DTO/ProductDtos.cs
public record ProductResponse : BaseStatusEntityResponse
{
    public Guid MerchantId { get; init; }
    public string MerchantName { get; init; } = string.Empty;
    public Guid? ProductCategoryId { get; init; }
    public string? ProductCategoryName { get; init; }
    public string? ImageUrl { get; init; }
    public decimal Price { get; init; }
    public decimal? DiscountedPrice { get; init; }
    public int StockQuantity { get; init; }
    public string? Unit { get; init; }
    public bool IsAvailable { get; init; }
}

// src/Application/DTO/MerchantDtos.cs
public record MerchantResponse : BaseRatedEntityResponse
{
    public Guid OwnerId { get; init; }
    public string OwnerName { get; init; } = string.Empty;
    public Guid ServiceCategoryId { get; init; }
    public string ServiceCategoryName { get; init; } = string.Empty;
    public string? LogoUrl { get; init; }
    public string? CoverImageUrl { get; init; }
    public string Address { get; init; } = string.Empty;
    public decimal Latitude { get; init; }
    public decimal Longitude { get; init; }
    public string PhoneNumber { get; init; } = string.Empty;
    public string? Email { get; init; }
    public decimal MinimumOrderAmount { get; init; }
    public decimal DeliveryFee { get; init; }
    public int AverageDeliveryTime { get; init; }
    public bool IsBusy { get; init; }
    public bool IsOpen { get; init; }
}
```

### 2. **Validation Rule Tekrarları**

#### **2.1 Common Validation Rules**

**Problem**: Benzer validation rule'ları tekrarlanıyor.

**Mevcut Durum**:
```csharp
// AuthValidators.cs
RuleFor(x => x.Email)
    .NotEmpty().WithMessage("Email is required")
    .EmailAddress().WithMessage("Invalid email format")
    .MaximumLength(256).WithMessage("Email must not exceed 256 characters");

// MerchantValidators.cs
RuleFor(x => x.Email)
    .NotEmpty().WithMessage("Email is required")
    .EmailAddress().WithMessage("Invalid email format")
    .MaximumLength(256).WithMessage("Email must not exceed 256 characters");
```

**Çözüm**: Validation extension'ları oluşturun.

**Implementation**:

```csharp
// src/Application/Validators/ValidationExtensions.cs
using FluentValidation;

namespace Getir.Application.Validators;

/// <summary>
/// Common validation rules as extension methods
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Email validation rule
    /// </summary>
    public static IRuleBuilder<T, string> EmailRule<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters");
    }

    /// <summary>
    /// Name validation rule
    /// </summary>
    public static IRuleBuilder<T, string> NameRule<T>(this IRuleBuilder<T, string> rule, string fieldName)
    {
        return rule
            .NotEmpty().WithMessage($"{fieldName} is required")
            .MaximumLength(100).WithMessage($"{fieldName} must not exceed 100 characters");
    }

    /// <summary>
    /// Description validation rule
    /// </summary>
    public static IRuleBuilder<T, string?> DescriptionRule<T>(this IRuleBuilder<T, string?> rule, string fieldName)
    {
        return rule
            .MaximumLength(500).WithMessage($"{fieldName} must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x));
    }

    /// <summary>
    /// Phone number validation rule
    /// </summary>
    public static IRuleBuilder<T, string?> PhoneNumberRule<T>(this IRuleBuilder<T, string?> rule)
    {
        return rule
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters")
            .Matches(@"^[\+]?[1-9][\d]{0,15}$").WithMessage("Invalid phone number format")
            .When(x => !string.IsNullOrEmpty(x));
    }

    /// <summary>
    /// Password validation rule
    /// </summary>
    public static IRuleBuilder<T, string> PasswordRule<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)").WithMessage("Password must contain at least one lowercase letter, one uppercase letter, and one digit");
    }

    /// <summary>
    /// Price validation rule
    /// </summary>
    public static IRuleBuilder<T, decimal> PriceRule<T>(this IRuleBuilder<T, decimal> rule, string fieldName)
    {
        return rule
            .GreaterThan(0).WithMessage($"{fieldName} must be greater than zero")
            .LessThan(999999.99m).WithMessage($"{fieldName} must be less than 999,999.99");
    }

    /// <summary>
    /// Quantity validation rule
    /// </summary>
    public static IRuleBuilder<T, int> QuantityRule<T>(this IRuleBuilder<T, int> rule, string fieldName)
    {
        return rule
            .GreaterThan(0).WithMessage($"{fieldName} must be greater than zero")
            .LessThanOrEqualTo(1000).WithMessage($"{fieldName} must not exceed 1000");
    }

    /// <summary>
    /// Rating validation rule
    /// </summary>
    public static IRuleBuilder<T, int> RatingRule<T>(this IRuleBuilder<T, int> rule)
    {
        return rule
            .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");
    }

    /// <summary>
    /// Address validation rule
    /// </summary>
    public static IRuleBuilder<T, string> AddressRule<T>(this IRuleBuilder<T, string> rule, string fieldName)
    {
        return rule
            .NotEmpty().WithMessage($"{fieldName} is required")
            .MaximumLength(500).WithMessage($"{fieldName} must not exceed 500 characters");
    }

    /// <summary>
    /// Comment validation rule
    /// </summary>
    public static IRuleBuilder<T, string> CommentRule<T>(this IRuleBuilder<T, string> rule, string fieldName)
    {
        return rule
            .NotEmpty().WithMessage($"{fieldName} is required")
            .MaximumLength(1000).WithMessage($"{fieldName} must not exceed 1000 characters");
    }
}
```

**Güncellenmiş Validator'lar**:

```csharp
// src/Application/Validators/AuthValidators.cs
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).EmailRule();
        RuleFor(x => x.Password).PasswordRule();
        RuleFor(x => x.FirstName).NameRule("First name");
        RuleFor(x => x.LastName).NameRule("Last name");
        RuleFor(x => x.PhoneNumber).PhoneNumberRule();
    }
}

// src/Application/Validators/MerchantValidators.cs
public class CreateMerchantRequestValidator : AbstractValidator<CreateMerchantRequest>
{
    public CreateMerchantRequestValidator()
    {
        RuleFor(x => x.Name).NameRule("Merchant name");
        RuleFor(x => x.Description).DescriptionRule("Description");
        RuleFor(x => x.Email).EmailRule();
        RuleFor(x => x.PhoneNumber).PhoneNumberRule();
        RuleFor(x => x.Address).AddressRule("Address");
        RuleFor(x => x.MinimumOrderAmount).PriceRule("Minimum order amount");
        RuleFor(x => x.DeliveryFee).PriceRule("Delivery fee");
    }
}
```

### 3. **Service Method Tekrarları**

#### **3.1 Common Service Operations**

**Problem**: Benzer service operation'ları tekrarlanıyor.

**Mevcut Durum**:
```csharp
// OrderService.cs
private async Task<Result<Order>> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken)
{
    var order = await _unitOfWork.ReadRepository<Order>()
        .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken: cancellationToken);
    
    if (order == null)
        return Result.Fail<Order>("Order not found", "ORDER_NOT_FOUND");
    
    return Result.Ok(order);
}

// ProductService.cs
private async Task<Result<Product>> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken)
{
    var product = await _unitOfWork.ReadRepository<Product>()
        .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken: cancellationToken);
    
    if (product == null)
        return Result.Fail<Product>("Product not found", "PRODUCT_NOT_FOUND");
    
    return Result.Ok(product);
}
```

**Çözüm**: Generic helper method'ları oluşturun.

**Implementation**:

```csharp
// src/Application/Common/ServiceHelpers.cs
using Getir.Application.Abstractions;
using Getir.Application.Common;
using System.Linq.Expressions;

namespace Getir.Application.Common;

/// <summary>
/// Common service helper methods
/// </summary>
public static class ServiceHelpers
{
    /// <summary>
    /// Generic method to get entity by ID with error handling
    /// </summary>
    public static async Task<Result<T>> GetEntityByIdAsync<T>(
        IReadOnlyRepository<T> repository,
        Guid id,
        string entityName,
        string errorCode,
        CancellationToken cancellationToken = default) where T : class
    {
        var entity = await repository.FirstOrDefaultAsync(
            e => EF.Property<Guid>(e, "Id") == id,
            cancellationToken: cancellationToken);

        if (entity == null)
            return Result.Fail<T>($"{entityName} not found", errorCode);

        return Result.Ok(entity);
    }

    /// <summary>
    /// Generic method to get entity by filter with error handling
    /// </summary>
    public static async Task<Result<T>> GetEntityByFilterAsync<T>(
        IReadOnlyRepository<T> repository,
        Expression<Func<T, bool>> filter,
        string entityName,
        string errorCode,
        CancellationToken cancellationToken = default) where T : class
    {
        var entity = await repository.FirstOrDefaultAsync(filter, cancellationToken: cancellationToken);

        if (entity == null)
            return Result.Fail<T>($"{entityName} not found", errorCode);

        return Result.Ok(entity);
    }

    /// <summary>
    /// Generic method to check if entity exists
    /// </summary>
    public static async Task<bool> EntityExistsAsync<T>(
        IReadOnlyRepository<T> repository,
        Expression<Func<T, bool>> filter,
        CancellationToken cancellationToken = default) where T : class
    {
        return await repository.AnyAsync(filter, cancellationToken);
    }

    /// <summary>
    /// Generic method to get paged entities
    /// </summary>
    public static async Task<Result<PagedResult<T>>> GetPagedEntitiesAsync<T>(
        IReadOnlyRepository<T> repository,
        Expression<Func<T, bool>>? filter,
        Expression<Func<T, object>>? orderBy,
        bool ascending,
        int page,
        int pageSize,
        string? include,
        CancellationToken cancellationToken = default) where T : class
    {
        var entities = await repository.GetPagedAsync(
            filter, orderBy, ascending, page, pageSize, include, cancellationToken);

        var totalCount = await repository.CountAsync(filter ?? (x => true), cancellationToken);

        var pagedResult = new PagedResult<T>(
            entities,
            totalCount,
            page,
            pageSize,
            (int)Math.Ceiling((double)totalCount / pageSize));

        return Result.Ok(pagedResult);
    }
}
```

**Güncellenmiş Service'ler**:

```csharp
// src/Application/Services/Orders/OrderService.cs
private async Task<Result<Order>> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken)
{
    return await ServiceHelpers.GetEntityByIdAsync(
        _unitOfWork.ReadRepository<Order>(),
        orderId,
        "Order",
        "ORDER_NOT_FOUND",
        cancellationToken);
}

// src/Application/Services/Products/ProductService.cs
private async Task<Result<Product>> GetProductByIdAsync(Guid productId, CancellationToken cancellationToken)
{
    return await ServiceHelpers.GetEntityByIdAsync(
        _unitOfWork.ReadRepository<Product>(),
        productId,
        "Product",
        "PRODUCT_NOT_FOUND",
        cancellationToken);
}
```

### 4. **Endpoint Pattern Tekrarları**

#### **4.1 Common Endpoint Patterns**

**Problem**: Benzer endpoint pattern'ları tekrarlanıyor.

**Mevcut Durum**:
```csharp
// ProductEndpoints.cs
group.MapGet("/{id:guid}", async (
    [FromRoute] Guid id,
    [FromServices] IProductService service,
    CancellationToken ct) =>
{
    var result = await service.GetProductByIdAsync(id, ct);
    return result.ToIResult();
})
.WithName("GetProduct")
.Produces<ProductResponse>(200)
.Produces(404);

// MerchantEndpoints.cs
group.MapGet("/{id:guid}", async (
    [FromRoute] Guid id,
    [FromServices] IMerchantService service,
    CancellationToken ct) =>
{
    var result = await service.GetMerchantByIdAsync(id, ct);
    return result.ToIResult();
})
.WithName("GetMerchant")
.Produces<MerchantResponse>(200)
.Produces(404);
```

**Çözüm**: Generic endpoint helper'ları oluşturun.

**Implementation**:

```csharp
// src/WebApi/Extensions/EndpointExtensions.cs
using Getir.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Extensions;

/// <summary>
/// Common endpoint patterns as extension methods
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Generic GET by ID endpoint
    /// </summary>
    public static RouteHandlerBuilder MapGetById<TService, TResponse>(
        this RouteGroupBuilder group,
        string pattern,
        Func<TService, Guid, CancellationToken, Task<Result<TResponse>>> serviceMethod,
        string endpointName,
        string tag)
    {
        return group.MapGet(pattern, async (
            [FromRoute] Guid id,
            [FromServices] TService service,
            CancellationToken ct) =>
        {
            var result = await serviceMethod(service, id, ct);
            return result.ToIResult();
        })
        .WithName(endpointName)
        .WithTags(tag)
        .Produces<TResponse>(200)
        .Produces(404);
    }

    /// <summary>
    /// Generic GET paged endpoint
    /// </summary>
    public static RouteHandlerBuilder MapGetPaged<TService, TResponse>(
        this RouteGroupBuilder group,
        string pattern,
        Func<TService, PaginationQuery, CancellationToken, Task<Result<PagedResult<TResponse>>>> serviceMethod,
        string endpointName,
        string tag)
    {
        return group.MapGet(pattern, async (
            [AsParameters] PaginationQuery query,
            [FromServices] TService service,
            CancellationToken ct) =>
        {
            var result = await serviceMethod(service, query, ct);
            return result.ToIResult();
        })
        .WithName(endpointName)
        .WithTags(tag)
        .Produces<PagedResult<TResponse>>(200);
    }

    /// <summary>
    /// Generic POST endpoint
    /// </summary>
    public static RouteHandlerBuilder MapPost<TService, TRequest, TResponse>(
        this RouteGroupBuilder group,
        string pattern,
        Func<TService, TRequest, CancellationToken, Task<Result<TResponse>>> serviceMethod,
        string endpointName,
        string tag)
    {
        return group.MapPost(pattern, async (
            [FromBody] TRequest request,
            [FromServices] TService service,
            CancellationToken ct) =>
        {
            var result = await serviceMethod(service, request, ct);
            return result.ToIResult();
        })
        .WithName(endpointName)
        .WithTags(tag)
        .Produces<TResponse>(201)
        .Produces(400);
    }

    /// <summary>
    /// Generic PUT endpoint
    /// </summary>
    public static RouteHandlerBuilder MapPut<TService, TRequest, TResponse>(
        this RouteGroupBuilder group,
        string pattern,
        Func<TService, Guid, TRequest, CancellationToken, Task<Result<TResponse>>> serviceMethod,
        string endpointName,
        string tag)
    {
        return group.MapPut(pattern, async (
            [FromRoute] Guid id,
            [FromBody] TRequest request,
            [FromServices] TService service,
            CancellationToken ct) =>
        {
            var result = await serviceMethod(service, id, request, ct);
            return result.ToIResult();
        })
        .WithName(endpointName)
        .WithTags(tag)
        .Produces<TResponse>(200)
        .Produces(400)
        .Produces(404);
    }

    /// <summary>
    /// Generic DELETE endpoint
    /// </summary>
    public static RouteHandlerBuilder MapDelete<TService>(
        this RouteGroupBuilder group,
        string pattern,
        Func<TService, Guid, CancellationToken, Task<Result>> serviceMethod,
        string endpointName,
        string tag)
    {
        return group.MapDelete(pattern, async (
            [FromRoute] Guid id,
            [FromServices] TService service,
            CancellationToken ct) =>
        {
            var result = await serviceMethod(service, id, ct);
            return result.ToIResult();
        })
        .WithName(endpointName)
        .WithTags(tag)
        .Produces(204)
        .Produces(404);
    }
}
```

**Güncellenmiş Endpoint'ler**:

```csharp
// src/WebApi/Endpoints/ProductEndpoints.cs
public static void MapProductEndpoints(this WebApplication app)
{
    var group = app.MapGroup("/api/v1/products")
        .WithTags("Products");

    // Generic endpoints
    group.MapGetById<IProductService, ProductResponse>(
        "/{id:guid}",
        (service, id, ct) => service.GetProductByIdAsync(id, ct),
        "GetProduct",
        "Products");

    group.MapGetPaged<IProductService, ProductResponse>(
        "/",
        (service, query, ct) => service.GetProductsAsync(query, ct),
        "GetProducts",
        "Products");

    group.MapPost<IProductService, CreateProductRequest, ProductResponse>(
        "/",
        (service, request, ct) => service.CreateProductAsync(request, ct),
        "CreateProduct",
        "Products");

    group.MapPut<IProductService, UpdateProductRequest, ProductResponse>(
        "/{id:guid}",
        (service, id, request, ct) => service.UpdateProductAsync(id, request, ct),
        "UpdateProduct",
        "Products");

    group.MapDelete<IProductService>(
        "/{id:guid}",
        (service, id, ct) => service.DeleteProductAsync(id, ct),
        "DeleteProduct",
        "Products");
}
```

## 📊 Beklenen Faydalar

### **Code Reduction**
- **DTO Code**: %40 azalış
- **Validator Code**: %50 azalış
- **Service Code**: %30 azalış
- **Endpoint Code**: %60 azalış

### **Maintainability**
- **Bug Fix Time**: %50 azalış
- **Feature Development**: %40 hızlanma
- **Code Review Time**: %30 azalış

### **Consistency**
- **Validation Rules**: %100 tutarlılık
- **Error Messages**: %100 tutarlılık
- **Response Format**: %100 tutarlılık

## 🚀 Implementation Plan

### **Hafta 1**
1. Base DTO'ları oluştur
2. Validation extension'ları ekle
3. Service helper'ları oluştur

### **Hafta 2**
1. Mevcut DTO'ları güncelle
2. Validator'ları refactor et
3. Service'leri güncelle

### **Hafta 3**
1. Endpoint helper'ları oluştur
2. Endpoint'leri refactor et
3. Test'leri güncelle

### **Hafta 4**
1. Code review
2. Performance test
3. Documentation update

## ✅ Success Criteria

- [ ] Tüm DTO'lar base class'lardan inherit ediyor
- [ ] Tüm validator'lar extension method'ları kullanıyor
- [ ] Tüm service'ler helper method'ları kullanıyor
- [ ] Tüm endpoint'ler generic pattern'ları kullanıyor
- [ ] Code duplication %40 azaldı
- [ ] Test coverage korundu
- [ ] Performance iyileşti
