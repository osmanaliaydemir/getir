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
)
{
    public BaseEntityResponse() : this(Guid.Empty, string.Empty, null, DateTime.MinValue, null) { }
}

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
