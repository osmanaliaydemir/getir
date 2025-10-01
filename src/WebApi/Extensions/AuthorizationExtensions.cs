using Getir.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Getir.WebApi.Extensions;

/// <summary>
/// Authorization ve Claims extension metodları
/// </summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// Endpoint'e role bazlı yetkilendirme ekler
    /// </summary>
    public static TBuilder RequireRole<TBuilder>(this TBuilder builder, params UserRole[] roles)
        where TBuilder : IEndpointConventionBuilder
    {
        var roleNames = roles.Select(r => r.ToString()).ToArray();
        return builder.RequireAuthorization(new AuthorizeAttribute { Roles = string.Join(",", roleNames) });
    }

    /// <summary>
    /// Endpoint'e role bazlı yetkilendirme ekler (string array ile)
    /// </summary>
    public static TBuilder RequireRole<TBuilder>(this TBuilder builder, params string[] roles)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireAuthorization(new AuthorizeAttribute { Roles = string.Join(",", roles) });
    }

    /// <summary>
    /// Sadece Admin rolüne izin verir
    /// </summary>
    public static TBuilder RequireAdmin<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireRole(UserRole.Admin);
    }

    /// <summary>
    /// Sadece Merchant Owner rolüne izin verir
    /// </summary>
    public static TBuilder RequireMerchantOwner<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireRole(UserRole.MerchantOwner);
    }

    /// <summary>
    /// Sadece Courier rolüne izin verir
    /// </summary>
    public static TBuilder RequireCourier<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireRole(UserRole.Courier);
    }

    /// <summary>
    /// ClaimsPrincipal'dan UserId'yi alır
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    /// <summary>
    /// ClaimsPrincipal'dan Email'i alır
    /// </summary>
    public static string? GetEmail(this ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return user.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// ClaimsPrincipal'dan Role'ü alır
    /// </summary>
    public static UserRole? GetRole(this ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);
        var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;
        return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : null;
    }

    /// <summary>
    /// Kullanıcının belirli bir role sahip olup olmadığını kontrol eder
    /// </summary>
    public static bool HasRole(this ClaimsPrincipal user, UserRole role)
    {
        return user.GetRole() == role;
    }

    /// <summary>
    /// Kullanıcının Admin olup olmadığını kontrol eder
    /// </summary>
    public static bool IsAdmin(this ClaimsPrincipal user)
    {
        return user.HasRole(UserRole.Admin);
    }

    /// <summary>
    /// Kullanıcının MerchantOwner olup olmadığını kontrol eder
    /// </summary>
    public static bool IsMerchantOwner(this ClaimsPrincipal user)
    {
        return user.HasRole(UserRole.MerchantOwner);
    }

    /// <summary>
    /// Kullanıcının Courier olup olmadığını kontrol eder
    /// </summary>
    public static bool IsCourier(this ClaimsPrincipal user)
    {
        return user.HasRole(UserRole.Courier);
    }

    /// <summary>
    /// Kullanıcının Customer olup olmadığını kontrol eder
    /// </summary>
    public static bool IsCustomer(this ClaimsPrincipal user)
    {
        return user.HasRole(UserRole.Customer);
    }
}

