using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.FileUpload;

/// <summary>
/// Service for integrating file uploads with entities
/// </summary>
public interface IFileUploadIntegrationService
{
    /// <summary>
    /// Update merchant logo URL
    /// </summary>
    Task<Result> UpdateMerchantLogoAsync(
        Guid merchantId, 
        string logoUrl, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update merchant cover image URL
    /// </summary>
    Task<Result> UpdateMerchantCoverAsync(
        Guid merchantId, 
        string coverImageUrl, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update product image URL
    /// </summary>
    Task<Result> UpdateProductImageAsync(
        Guid productId, 
        string imageUrl, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete merchant logo and update entity
    /// </summary>
    Task<Result> DeleteMerchantLogoAsync(
        Guid merchantId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete merchant cover image and update entity
    /// </summary>
    Task<Result> DeleteMerchantCoverAsync(
        Guid merchantId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete product image and update entity
    /// </summary>
    Task<Result> DeleteProductImageAsync(
        Guid productId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get merchant logo URL
    /// </summary>
    Task<Result<string?>> GetMerchantLogoAsync(
        Guid merchantId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get merchant cover image URL
    /// </summary>
    Task<Result<string?>> GetMerchantCoverAsync(
        Guid merchantId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get product image URL
    /// </summary>
    Task<Result<string?>> GetProductImageAsync(
        Guid productId, 
        CancellationToken cancellationToken = default);

}
