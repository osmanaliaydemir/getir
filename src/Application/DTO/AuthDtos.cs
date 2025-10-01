using Getir.Domain.Enums;

namespace Getir.Application.DTO;

public record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    UserRole? Role = null); // Optional, default Customer

public record LoginRequest(
    string Email,
    string Password);

public record RefreshTokenRequest(
    string RefreshToken);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserRole Role,
    Guid UserId,
    string Email,
    string FullName);

public record UserProfileResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    UserRole Role,
    bool IsEmailVerified,
    DateTime CreatedAt);
