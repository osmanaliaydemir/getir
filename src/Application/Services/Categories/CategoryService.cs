using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;

namespace Getir.Application.Services.Categories;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<CategoryResponse>>> GetCategoriesAsync(
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.Repository<Category>().GetPagedAsync(
            filter: c => c.IsActive,
            orderBy: c => c.DisplayOrder,
            ascending: true,
            page: query.Page,
            pageSize: query.PageSize,
            cancellationToken: cancellationToken);

        var total = await _unitOfWork.ReadRepository<Category>()
            .CountAsync(c => c.IsActive, cancellationToken);

        var response = categories.Select(c => new CategoryResponse(
            c.Id,
            c.Name,
            c.Description,
            c.ImageUrl,
            c.DisplayOrder,
            c.IsActive,
            c.CreatedAt
        )).ToList();

        var pagedResult = PagedResult<CategoryResponse>.Create(response, total, query.Page, query.PageSize);
        
        return Result.Ok(pagedResult);
    }

    public async Task<Result<CategoryResponse>> GetCategoryByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.ReadRepository<Category>()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken: cancellationToken);

        if (category == null)
        {
            return Result.Fail<CategoryResponse>("Category not found", "NOT_FOUND_CATEGORY");
        }

        var response = new CategoryResponse(
            category.Id,
            category.Name,
            category.Description,
            category.ImageUrl,
            category.DisplayOrder,
            category.IsActive,
            category.CreatedAt
        );

        return Result.Ok(response);
    }

    public async Task<Result<CategoryResponse>> CreateCategoryAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            DisplayOrder = request.DisplayOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Category>().AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CategoryResponse(
            category.Id,
            category.Name,
            category.Description,
            category.ImageUrl,
            category.DisplayOrder,
            category.IsActive,
            category.CreatedAt
        );

        return Result.Ok(response);
    }

    public async Task<Result<CategoryResponse>> UpdateCategoryAsync(
        Guid id,
        UpdateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Repository<Category>()
            .GetByIdAsync(id, cancellationToken);

        if (category == null)
        {
            return Result.Fail<CategoryResponse>("Category not found", "NOT_FOUND_CATEGORY");
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.ImageUrl = request.ImageUrl;
        category.DisplayOrder = request.DisplayOrder;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Category>().Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new CategoryResponse(
            category.Id,
            category.Name,
            category.Description,
            category.ImageUrl,
            category.DisplayOrder,
            category.IsActive,
            category.CreatedAt
        );

        return Result.Ok(response);
    }

    public async Task<Result> DeleteCategoryAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Repository<Category>()
            .GetByIdAsync(id, cancellationToken);

        if (category == null)
        {
            return Result.Fail("Category not found", "NOT_FOUND_CATEGORY");
        }

        // Soft delete
        category.IsActive = false;
        category.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Category>().Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
