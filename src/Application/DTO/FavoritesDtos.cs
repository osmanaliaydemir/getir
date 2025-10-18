namespace Getir.Application.DTO;

public record AddToFavoritesRequest(Guid ProductId);

public record RemoveFromFavoritesRequest(Guid ProductId);

public record FavoriteProductResponse
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string? ProductDescription { get; init; }
    public decimal Price { get; init; }
    public string? ImageUrl { get; init; }
    public Guid MerchantId { get; init; }
    public string MerchantName { get; init; } = string.Empty;
    public bool IsAvailable { get; init; }
    public DateTime AddedAt { get; init; }
}

