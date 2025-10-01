using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;

namespace Getir.Application.Services.ServiceCategories;

public class ServiceCategoryService : IServiceCategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public ServiceCategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<ServiceCategoryResponse>>> GetServiceCategoriesAsync(
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.Repository<ServiceCategory>().GetPagedAsync(
            filter: c => c.IsActive,
            orderBy: c => c.DisplayOrder,
            ascending: true,
            page: query.Page,
            pageSize: query.PageSize,
            cancellationToken: cancellationToken);

        var total = await _unitOfWork.ReadRepository<ServiceCategory>()
            .CountAsync(c => c.IsActive, cancellationToken);

        var response = categories.Select(c => new ServiceCategoryResponse(
            c.Id,
            c.Name,
            c.Description,
            c.ImageUrl,
            c.IconUrl,
            c.DisplayOrder,
            c.IsActive,
            c.Merchants?.Count ?? 0
        )).ToList();

        var pagedResult = PagedResult<ServiceCategoryResponse>.Create(response, total, query.Page, query.PageSize);
        
        return Result.Ok(pagedResult);
    }

    public async Task<Result<ServiceCategoryResponse>> GetServiceCategoryByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Repository<ServiceCategory>()
            .GetAsync(c => c.Id == id, include: "Merchants", cancellationToken: cancellationToken);

        if (category == null)
        {
            return Result.Fail<ServiceCategoryResponse>("Service category not found", "NOT_FOUND_SERVICE_CATEGORY");
        }

        var response = new ServiceCategoryResponse(
            category.Id,
            category.Name,
            category.Description,
            category.ImageUrl,
            category.IconUrl,
            category.DisplayOrder,
            category.IsActive,
            category.Merchants?.Count ?? 0
        );

        return Result.Ok(response);
    }

    public async Task<Result<ServiceCategoryResponse>> CreateServiceCategoryAsync(
        CreateServiceCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = new ServiceCategory
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            IconUrl = request.IconUrl,
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<ServiceCategory>().AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new ServiceCategoryResponse(
            category.Id,
            category.Name,
            category.Description,
            category.ImageUrl,
            category.IconUrl,
            category.DisplayOrder,
            category.IsActive,
            0
        );

        return Result.Ok(response);
    }

    public async Task<Result<ServiceCategoryResponse>> UpdateServiceCategoryAsync(
        Guid id,
        UpdateServiceCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Repository<ServiceCategory>()
            .GetByIdAsync(id, cancellationToken);

        if (category == null)
        {
            return Result.Fail<ServiceCategoryResponse>("Service category not found", "NOT_FOUND_SERVICE_CATEGORY");
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.ImageUrl = request.ImageUrl;
        category.IconUrl = request.IconUrl;
        category.DisplayOrder = request.DisplayOrder;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<ServiceCategory>().Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var merchantCount = await _unitOfWork.ReadRepository<Merchant>()
            .CountAsync(m => m.ServiceCategoryId == id, cancellationToken);

        var response = new ServiceCategoryResponse(
            category.Id,
            category.Name,
            category.Description,
            category.ImageUrl,
            category.IconUrl,
            category.DisplayOrder,
            category.IsActive,
            merchantCount
        );

        return Result.Ok(response);
    }

    public async Task<Result> DeleteServiceCategoryAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Repository<ServiceCategory>()
            .GetByIdAsync(id, cancellationToken);

        if (category == null)
        {
            return Result.Fail("Service category not found", "NOT_FOUND_SERVICE_CATEGORY");
        }

        // Soft delete
        category.IsActive = false;
        category.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<ServiceCategory>().Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}

