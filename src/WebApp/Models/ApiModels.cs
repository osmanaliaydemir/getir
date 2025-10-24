using System.Text.Json.Serialization;

namespace WebApp.Models;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
}

public class PaginationQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new List<T>();
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(Total / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

public class MerchantApiResponse
{
    public bool IsSuccess { get; set; }
    public PagedResult<MerchantResponse> Data { get; set; } = new();
    public string? Error { get; set; }
}

public class ProductApiResponse
{
    public bool IsSuccess { get; set; }
    public PagedResult<ProductResponse> Data { get; set; } = new();
    public string? Error { get; set; }
}

public class BaseEntityResponse
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class BaseStatusEntityResponse : BaseEntityResponse
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class ProductResponse : BaseStatusEntityResponse
{
    public Guid MerchantId { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public Guid? ProductCategoryId { get; set; }
    public string? ProductCategoryName { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public decimal DiscountPercentage { get; set; }
    public int StockQuantity { get; set; }
    public string? Unit { get; set; }
    public bool IsAvailable { get; set; }
    public decimal? Rating { get; set; }
    public int? ReviewCount { get; set; }
    public string Category { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

public class AuthResponse
{
    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; set; } = true;
    
    [JsonPropertyName("error")]
    public string? Error { get; set; }
    
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; } = string.Empty;
    
    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; } = string.Empty;
    
    [JsonPropertyName("expiresAt")]
    public DateTime ExpiresAt { get; set; }
    
    [JsonPropertyName("role")]
    public int Role { get; set; }
    
    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = string.Empty;
    
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;
    
    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;
    
    [JsonPropertyName("merchantId")]
    public Guid? MerchantId { get; set; }
}

public class AddToCartRequest
{
    public Guid MerchantId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string? Notes { get; set; }
}

public class UpdateCartItemRequest
{
    public int Quantity { get; set; }
    public string? Notes { get; set; }
}

public class CartItemResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid MerchantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductImageUrl { get; set; } = string.Empty;
    public string MerchantName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Notes { get; set; }
}

public class CartResponse
{
    public Guid MerchantId { get; set; }
    public string MerchantName { get; set; } = string.Empty;
    public List<CartItemResponse> Items { get; set; } = new();
    public decimal SubTotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Total { get; set; }
    public decimal TotalAmount => Total; // Alias for Total
    public int ItemCount { get; set; }
}

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Merchant response model
/// </summary>
public class MerchantResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public TimeSpan DeliveryTime { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal MinOrderAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// User profile response model
/// </summary>
public class UserProfileResponse
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string Gender { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<AddressResponse> Addresses { get; set; } = new();
}

/// <summary>
/// Update user profile request
/// </summary>
public class UpdateUserProfileRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string Gender { get; set; } = string.Empty;
}

/// <summary>
/// Change password request
/// </summary>
public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Address response model
/// </summary>
public class AddressResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Instructions { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Update address request
/// </summary>
public class UpdateAddressRequest
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Instructions { get; set; }
    public bool IsDefault { get; set; }
}

/// <summary>
/// Add address request
/// </summary>
public class AddAddressRequest
{
    public string Title { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    public string AddressLine2 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Instructions { get; set; }
    public bool IsDefault { get; set; }
}

/// <summary>
/// Order response model
/// </summary>
public class OrderResponse
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal SubTotal { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public AddressResponse DeliveryAddress { get; set; } = new();
    public List<OrderItemResponse> Items { get; set; } = new();
    public MerchantResponse Merchant { get; set; } = new();
}

/// <summary>
/// Order item response model
/// </summary>
public class OrderItemResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductImageUrl { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

/// <summary>
/// Create order request
/// </summary>
public class CreateOrderRequest
{
    public Guid MerchantId { get; set; }
    public Guid DeliveryAddressId { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public string? Notes { get; set; }
    public List<OrderItemRequest> Items { get; set; } = new();
}

/// <summary>
/// Order item request
/// </summary>
public class OrderItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

