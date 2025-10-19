using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.UserPreferences;

/// <summary>
/// User preferences service interface
/// </summary>
public interface IUserPreferencesService
{
    /// <summary>
    /// Get user notification preferences
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User notification preferences or null if not found</returns>
    Task<Result<UserNotificationPreferencesResponse>> GetUserPreferencesAsync(Guid userId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Get or create user notification preferences (ensures always exists)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User notification preferences</returns>
    Task<Result<UserNotificationPreferencesResponse>> GetOrCreateUserPreferencesAsync(Guid userId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Update user notification preferences
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated preferences</returns>
    Task<Result<UserNotificationPreferencesResponse>> UpdateUserPreferencesAsync(Guid userId, UpdateUserNotificationPreferencesRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Get simplified merchant portal preferences
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Merchant notification preferences</returns>
    Task<Result<MerchantNotificationPreferencesResponse>> GetMerchantPreferencesAsync(Guid userId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Update simplified merchant portal preferences
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated merchant preferences</returns>
    Task<Result<MerchantNotificationPreferencesResponse>> UpdateMerchantPreferencesAsync(Guid userId, UpdateMerchantNotificationPreferencesRequest request, CancellationToken cancellationToken = default);
    /// <summary>
    /// Delete user preferences
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<Result> DeleteUserPreferencesAsync(Guid userId, CancellationToken cancellationToken = default);
}

