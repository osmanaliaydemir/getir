using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Application.Services.FileUpload;
using Getir.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Getir.WebApi.Endpoints;

public static class FileUploadEndpoints
{
    public static void MapFileUploadEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/files")
            .WithTags("File Upload");

        // Customer endpoints
        group.MapPost("/upload", UploadFile)
            .WithName("UploadFile")
            .WithSummary("Upload a file (Customer)")
            .RequireAuthorization()
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<FileUploadResponse>(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(401)
            .Produces<ProblemDetails>(413);

        group.MapPost("/upload-multiple", UploadMultipleFiles)
            .WithName("UploadMultipleFiles")
            .WithSummary("Upload multiple files (Customer)")
            .RequireAuthorization()
            .Accepts<IFormFile[]>("multipart/form-data")
            .Produces<IEnumerable<FileUploadResponse>>(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(401)
            .Produces<ProblemDetails>(413);

        group.MapGet("/{containerName}/{fileName}", GetFileUrl)
            .WithName("GetFileUrl")
            .WithSummary("Get file download URL")
            .RequireAuthorization()
            .Produces<string>(200)
            .Produces<ProblemDetails>(404);

        group.MapDelete("/{containerName}/{fileName}", DeleteFile)
            .WithName("DeleteFile")
            .WithSummary("Delete a file (Customer)")
            .RequireAuthorization()
            .Produces(204)
            .Produces<ProblemDetails>(404)
            .Produces<ProblemDetails>(401);

        // Merchant endpoints
        var merchantGroup = group.MapGroup("/merchant")
            .RequireAuthorization();

        merchantGroup.MapPost("/logo", UploadMerchantLogo)
            .WithName("UploadMerchantLogo")
            .WithSummary("Upload merchant logo")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<FileUploadResponse>(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(401);

        merchantGroup.MapPost("/cover", UploadMerchantCover)
            .WithName("UploadMerchantCover")
            .WithSummary("Upload merchant cover image")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<FileUploadResponse>(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(401);

        merchantGroup.MapPost("/product-image", UploadProductImage)
            .WithName("UploadProductImage")
            .WithSummary("Upload product image")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<FileUploadResponse>(200)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(401);

        // Admin endpoints
        var adminGroup = group.MapGroup("/admin")
            .RequireAuthorization();

        adminGroup.MapGet("/stats", GetUploadStats)
            .WithName("GetUploadStats")
            .WithSummary("Get file upload statistics (Admin)")
            .Produces<FileUploadStats>(200)
            .Produces<ProblemDetails>(401);

        adminGroup.MapDelete("/bulk/{containerName}", BulkDeleteFiles)
            .WithName("BulkDeleteFiles")
            .WithSummary("Bulk delete files (Admin)")
            .Produces(204)
            .Produces<ProblemDetails>(400)
            .Produces<ProblemDetails>(401);

        // CDN endpoints
        var cdnGroup = group.MapGroup("/cdn")
            .RequireAuthorization();

        cdnGroup.MapGet("/url/{containerName}/{fileName}", GetCdnUrl)
            .WithName("GetCdnUrl")
            .WithSummary("Get CDN URL for a file")
            .Produces<string>(200)
            .Produces<ProblemDetails>(404);

        cdnGroup.MapGet("/optimized/{containerName}/{fileName}", GetOptimizedImageUrl)
            .WithName("GetOptimizedImageUrl")
            .WithSummary("Get optimized image URL with transformations")
            .Produces<string>(200)
            .Produces<ProblemDetails>(404);

        cdnGroup.MapDelete("/cache/{containerName}/{fileName}", InvalidateCache)
            .WithName("InvalidateCache")
            .WithSummary("Invalidate CDN cache for a file")
            .Produces(204)
            .Produces<ProblemDetails>(404);

        cdnGroup.MapGet("/stats", GetCdnStats)
            .WithName("GetCdnStats")
            .WithSummary("Get CDN statistics")
            .Produces<CdnStats>(200)
            .Produces<ProblemDetails>(401);
    }

    #region Customer Endpoints

    private static async Task<IResult> UploadFile(
        IFormFile file,
        [FromForm] FileCategory category,
        [FromForm] string? description,
        [FromForm] Guid? relatedEntityId,
        [FromForm] string? relatedEntityType,
        ClaimsPrincipal user,
        IFileStorageService fileStorageService,
        CancellationToken cancellationToken)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return Results.BadRequest(new ProblemDetails
                {
                    Title = "No file provided",
                    Detail = "Please provide a valid file to upload",
                    Status = 400
                });
            }

            var fileContent = new byte[file.Length];
            using var stream = file.OpenReadStream();
            await stream.ReadAsync(fileContent, 0, (int)file.Length, cancellationToken);

            var containerName = GetContainerNameForCategory(category);
            var request = new FileUploadRequest(
                file.FileName,
                fileContent,
                file.ContentType,
                containerName,
                category,
                description,
                relatedEntityId,
                relatedEntityType);

            var result = await fileStorageService.UploadFileAsync(request, cancellationToken);
            return result.ToApiResult();
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "File upload failed",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> UploadMultipleFiles(
        IFormFileCollection files,
        [FromForm] FileCategory category,
        [FromForm] string? description,
        [FromForm] Guid? relatedEntityId,
        [FromForm] string? relatedEntityType,
        ClaimsPrincipal user,
        IFileStorageService fileStorageService,
        CancellationToken cancellationToken)
    {
        try
        {
            if (files == null || !files.Any())
            {
                return Results.BadRequest(new ProblemDetails
                {
                    Title = "No files provided",
                    Detail = "Please provide at least one file to upload",
                    Status = 400
                });
            }

            var requests = new List<FileUploadRequest>();
            var containerName = GetContainerNameForCategory(category);

            foreach (var file in files)
            {
                if (file.Length == 0) continue;

                var fileContent = new byte[file.Length];
                using var stream = file.OpenReadStream();
                await stream.ReadAsync(fileContent, 0, (int)file.Length, cancellationToken);

                var request = new FileUploadRequest(
                    file.FileName,
                    fileContent,
                    file.ContentType,
                    containerName,
                    category,
                    description,
                    relatedEntityId,
                    relatedEntityType);

                requests.Add(request);
            }

            var result = await fileStorageService.UploadMultipleFilesAsync(requests, cancellationToken);
            return result.ToApiResult();
        }
        catch (Exception ex)
        {
            return Results.Problem(
                title: "Multiple file upload failed",
                detail: ex.Message,
                statusCode: 500);
        }
    }

    private static async Task<IResult> GetFileUrl(
        string containerName,
        string fileName,
        ClaimsPrincipal user,
        IFileStorageService fileStorageService,
        CancellationToken cancellationToken)
    {
        var result = await fileStorageService.GetFileUrlAsync(fileName, containerName, cancellationToken);
        return result.ToApiResult();
    }

    private static async Task<IResult> DeleteFile(
        string containerName,
        string fileName,
        ClaimsPrincipal user,
        IFileStorageService fileStorageService,
        CancellationToken cancellationToken)
    {
        var result = await fileStorageService.DeleteFileAsync(fileName, containerName, cancellationToken);
        return result.ToApiResult();
    }

    #endregion

    #region Merchant Endpoints

    private static async Task<IResult> UploadMerchantLogo(
        IFormFile file,
        [FromForm] Guid merchantId,
        ClaimsPrincipal user,
        IFileStorageService fileStorageService,
        IFileUploadIntegrationService integrationService,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "No logo file provided",
                Detail = "Please provide a valid logo file to upload",
                Status = 400
            });
        }

        var fileContent = new byte[file.Length];
        using var stream = file.OpenReadStream();
        await stream.ReadAsync(fileContent, 0, (int)file.Length, cancellationToken);

        var request = new FileUploadRequest(
            file.FileName,
            fileContent,
            file.ContentType,
            FileContainers.MerchantLogos,
            FileCategory.MerchantLogo,
            "Merchant logo",
            merchantId,
            "Merchant");

        var uploadResult = await fileStorageService.UploadFileAsync(request, cancellationToken);
        if (!uploadResult.Success)
        {
            return uploadResult.ToApiResult();
        }

        // Update merchant entity with new logo URL
        var updateResult = await integrationService.UpdateMerchantLogoAsync(
            merchantId, 
            uploadResult.Value!.BlobUrl, 
            cancellationToken);

        if (!updateResult.Success)
        {
            // If entity update fails, delete the uploaded file
            await fileStorageService.DeleteFileAsync(
                uploadResult.Value!.FileName, 
                FileContainers.MerchantLogos, 
                cancellationToken);
            
            return updateResult.ToApiResult();
        }

        return uploadResult.ToApiResult();
    }

    private static async Task<IResult> UploadMerchantCover(
        IFormFile file,
        [FromForm] Guid merchantId,
        ClaimsPrincipal user,
        IFileStorageService fileStorageService,
        IFileUploadIntegrationService integrationService,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "No cover file provided",
                Detail = "Please provide a valid cover image file to upload",
                Status = 400
            });
        }

        var fileContent = new byte[file.Length];
        using var stream = file.OpenReadStream();
        await stream.ReadAsync(fileContent, 0, (int)file.Length, cancellationToken);

        var request = new FileUploadRequest(
            file.FileName,
            fileContent,
            file.ContentType,
            FileContainers.MerchantCovers,
            FileCategory.MerchantCover,
            "Merchant cover image",
            merchantId,
            "Merchant");

        var uploadResult = await fileStorageService.UploadFileAsync(request, cancellationToken);
        if (!uploadResult.Success)
        {
            return uploadResult.ToApiResult();
        }

        // Update merchant entity with new cover URL
        var updateResult = await integrationService.UpdateMerchantCoverAsync(
            merchantId, 
            uploadResult.Value!.BlobUrl, 
            cancellationToken);

        if (!updateResult.Success)
        {
            // If entity update fails, delete the uploaded file
            await fileStorageService.DeleteFileAsync(
                uploadResult.Value!.FileName, 
                FileContainers.MerchantCovers, 
                cancellationToken);
            
            return updateResult.ToApiResult();
        }

        return uploadResult.ToApiResult();
    }

    private static async Task<IResult> UploadProductImage(
        IFormFile file,
        [FromForm] Guid productId,
        ClaimsPrincipal user,
        IFileStorageService fileStorageService,
        IFileUploadIntegrationService integrationService,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "No product image file provided",
                Detail = "Please provide a valid product image file to upload",
                Status = 400
            });
        }

        var fileContent = new byte[file.Length];
        using var stream = file.OpenReadStream();
        await stream.ReadAsync(fileContent, 0, (int)file.Length, cancellationToken);

        var request = new FileUploadRequest(
            file.FileName,
            fileContent,
            file.ContentType,
            FileContainers.ProductImages,
            FileCategory.ProductImage,
            "Product image",
            productId,
            "Product");

        var uploadResult = await fileStorageService.UploadFileAsync(request, cancellationToken);
        if (!uploadResult.Success)
        {
            return uploadResult.ToApiResult();
        }

        // Update product entity with new image URL
        var updateResult = await integrationService.UpdateProductImageAsync(
            productId, 
            uploadResult.Value!.BlobUrl, 
            cancellationToken);

        if (!updateResult.Success)
        {
            // If entity update fails, delete the uploaded file
            await fileStorageService.DeleteFileAsync(
                uploadResult.Value!.FileName, 
                FileContainers.ProductImages, 
                cancellationToken);
            
            return updateResult.ToApiResult();
        }

        return uploadResult.ToApiResult();
    }

    #endregion

    #region Admin Endpoints

    private static async Task<IResult> GetUploadStats(
        ClaimsPrincipal user,
        IFileStorageService fileStorageService,
        CancellationToken cancellationToken)
    {
        // TODO: Implement upload statistics
        var stats = new FileUploadStats(
            TotalFiles: 0,
            TotalSizeBytes: 0,
            FilesByCategory: 0,
            LastUploadDate: DateTime.UtcNow);

        return Results.Ok(stats);
    }

    private static async Task<IResult> BulkDeleteFiles(
        string containerName,
        [FromBody] IEnumerable<string> fileNames,
        ClaimsPrincipal user,
        IFileStorageService fileStorageService,
        CancellationToken cancellationToken)
    {
        var result = await fileStorageService.DeleteMultipleFilesAsync(fileNames, containerName, cancellationToken);
        return result.ToApiResult();
    }

    #endregion

    #region CDN Endpoints

    private static async Task<IResult> GetCdnUrl(
        string containerName,
        string fileName,
        ClaimsPrincipal user,
        ICdnService cdnService,
        CancellationToken cancellationToken)
    {
        var originalUrl = $"/uploads/{containerName}/{fileName}";
        var result = await cdnService.GetCdnUrlAsync(originalUrl, containerName, cancellationToken);
        return result.ToApiResult();
    }

    private static async Task<IResult> GetOptimizedImageUrl(
        string containerName,
        string fileName,
        [FromQuery] int? width,
        [FromQuery] int? height,
        [FromQuery] int? quality,
        [FromQuery] string? format,
        ClaimsPrincipal user,
        ICdnService cdnService,
        CancellationToken cancellationToken)
    {
        var originalUrl = $"/uploads/{containerName}/{fileName}";
        var result = await cdnService.GetOptimizedImageUrlAsync(
            originalUrl, width, height, quality, format, cancellationToken);
        return result.ToApiResult();
    }

    private static async Task<IResult> InvalidateCache(
        string containerName,
        string fileName,
        ClaimsPrincipal user,
        ICdnService cdnService,
        CancellationToken cancellationToken)
    {
        var fileUrl = $"/uploads/{containerName}/{fileName}";
        var result = await cdnService.InvalidateCacheAsync(fileUrl, cancellationToken);
        return result.ToApiResult();
    }

    private static async Task<IResult> GetCdnStats(
        ClaimsPrincipal user,
        ICdnService cdnService,
        CancellationToken cancellationToken)
    {
        var result = await cdnService.GetCdnStatsAsync(cancellationToken);
        return result.ToApiResult();
    }

    #endregion

    #region Helper Methods

    private static string GetContainerNameForCategory(FileCategory category)
    {
        return category switch
        {
            FileCategory.MerchantLogo => FileContainers.MerchantLogos,
            FileCategory.MerchantCover => FileContainers.MerchantCovers,
            FileCategory.ProductImage => FileContainers.ProductImages,
            FileCategory.UserAvatar => FileContainers.UserAvatars,
            FileCategory.Document => FileContainers.Documents,
            FileCategory.CategoryIcon => FileContainers.CategoryIcons,
            _ => FileContainers.Documents
        };
    }

    #endregion
}
