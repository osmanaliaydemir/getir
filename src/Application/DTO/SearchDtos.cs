namespace Getir.Application.DTO;

public record SearchProductsQuery(
    string Query,
    Guid? MerchantId,
    Guid? CategoryId,
    decimal? MinPrice,
    decimal? MaxPrice,
    int Page = 1,
    int PageSize = 20);

public record SearchMerchantsQuery(
    string Query,
    Guid? CategoryId,
    decimal? Latitude,
    decimal? Longitude,
    int? MaxDistance, // km
    int Page = 1,
    int PageSize = 20);
