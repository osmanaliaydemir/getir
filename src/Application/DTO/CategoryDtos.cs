namespace Getir.Application.DTO;

public record CreateCategoryRequest(
    string Name,
    string? Description,
    string? ImageUrl,
    int DisplayOrder);

public record UpdateCategoryRequest(
    string Name,
    string? Description,
    string? ImageUrl,
    int DisplayOrder,
    bool IsActive);

public record CategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    string? ImageUrl,
    int DisplayOrder,
    bool IsActive,
    DateTime CreatedAt);
