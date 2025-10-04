using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.Merchants;

public class MerchantOnboardingService : BaseService, IMerchantOnboardingService
{
    private readonly IBackgroundTaskService _backgroundTaskService;

    public MerchantOnboardingService(
        IUnitOfWork unitOfWork,
        ILogger<MerchantOnboardingService> logger,
        ILoggingService loggingService,
        ICacheService cacheService,
        IBackgroundTaskService backgroundTaskService) 
        : base(unitOfWork, logger, loggingService, cacheService)
    {
        _backgroundTaskService = backgroundTaskService;
    }

    public async Task<Result<MerchantOnboardingResponse>> CreateOnboardingAsync(
        CreateMerchantOnboardingRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Merchant var mı kontrol et
        var merchant = await _unitOfWork.ReadRepository<Merchant>()
            .FirstOrDefaultAsync(m => m.Id == request.MerchantId && m.OwnerId == request.OwnerId, 
                cancellationToken: cancellationToken);

        if (merchant == null)
        {
            return Result.Fail<MerchantOnboardingResponse>("Merchant not found or access denied", "NOT_FOUND_MERCHANT");
        }

        // Zaten onboarding var mı kontrol et
        var existingOnboarding = await _unitOfWork.ReadRepository<MerchantOnboarding>()
            .FirstOrDefaultAsync(o => o.MerchantId == request.MerchantId, 
                cancellationToken: cancellationToken);

        if (existingOnboarding != null)
        {
            return Result.Fail<MerchantOnboardingResponse>("Onboarding already exists for this merchant", "ONBOARDING_EXISTS");
        }

        var onboarding = new MerchantOnboarding
        {
            Id = Guid.NewGuid(),
            MerchantId = request.MerchantId,
            OwnerId = request.OwnerId,
            BasicInfoCompleted = false,
            BusinessInfoCompleted = false,
            WorkingHoursCompleted = false,
            DeliveryZonesCompleted = false,
            ProductsAdded = false,
            DocumentsUploaded = false,
            IsVerified = false,
            IsApproved = false,
            CompletedSteps = 0,
            TotalSteps = 6,
            ProgressPercentage = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<MerchantOnboarding>().AddAsync(onboarding, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new MerchantOnboardingResponse(
            onboarding.Id,
            onboarding.MerchantId,
            onboarding.OwnerId,
            onboarding.BasicInfoCompleted,
            onboarding.BusinessInfoCompleted,
            onboarding.WorkingHoursCompleted,
            onboarding.DeliveryZonesCompleted,
            onboarding.ProductsAdded,
            onboarding.DocumentsUploaded,
            onboarding.IsVerified,
            onboarding.IsApproved,
            onboarding.RejectionReason,
            onboarding.VerifiedAt,
            onboarding.ApprovedAt,
            onboarding.CompletedSteps,
            onboarding.TotalSteps,
            onboarding.ProgressPercentage,
            onboarding.CreatedAt,
            onboarding.UpdatedAt);

        return Result.Ok(response);
    }

    public async Task<Result<MerchantOnboardingResponse>> GetOnboardingStatusAsync(
        Guid merchantId,
        CancellationToken cancellationToken = default)
    {
        var onboarding = await _unitOfWork.ReadRepository<MerchantOnboarding>()
            .FirstOrDefaultAsync(o => o.MerchantId == merchantId, 
                include: "Merchant,Owner", 
                cancellationToken: cancellationToken);

        if (onboarding == null)
        {
            return Result.Fail<MerchantOnboardingResponse>("Onboarding not found", "NOT_FOUND_ONBOARDING");
        }

        var response = new MerchantOnboardingResponse(
            onboarding.Id,
            onboarding.MerchantId,
            onboarding.OwnerId,
            onboarding.BasicInfoCompleted,
            onboarding.BusinessInfoCompleted,
            onboarding.WorkingHoursCompleted,
            onboarding.DeliveryZonesCompleted,
            onboarding.ProductsAdded,
            onboarding.DocumentsUploaded,
            onboarding.IsVerified,
            onboarding.IsApproved,
            onboarding.RejectionReason,
            onboarding.VerifiedAt,
            onboarding.ApprovedAt,
            onboarding.CompletedSteps,
            onboarding.TotalSteps,
            onboarding.ProgressPercentage,
            onboarding.CreatedAt,
            onboarding.UpdatedAt);

        return Result.Ok(response);
    }

    public async Task<Result<OnboardingProgressResponse>> GetOnboardingProgressAsync(
        Guid merchantId,
        CancellationToken cancellationToken = default)
    {
        var onboarding = await _unitOfWork.ReadRepository<MerchantOnboarding>()
            .FirstOrDefaultAsync(o => o.MerchantId == merchantId, 
                cancellationToken: cancellationToken);

        if (onboarding == null)
        {
            return Result.Fail<OnboardingProgressResponse>("Onboarding not found", "NOT_FOUND_ONBOARDING");
        }

        var steps = new List<OnboardingStepResponse>
        {
            new("BasicInfo", "Temel Bilgiler", "İşletme adı, adres ve iletişim bilgileri", 
                onboarding.BasicInfoCompleted, true, "BusinessInfo"),
            new("BusinessInfo", "İş Bilgileri", "Açıklama, kategori ve teslimat bilgileri", 
                onboarding.BusinessInfoCompleted, true, "WorkingHours"),
            new("WorkingHours", "Çalışma Saatleri", "Haftalık çalışma saatlerini belirle", 
                onboarding.WorkingHoursCompleted, true, "DeliveryZones"),
            new("DeliveryZones", "Teslimat Bölgeleri", "Hizmet verilecek bölgeleri tanımla", 
                onboarding.DeliveryZonesCompleted, true, "Products"),
            new("Products", "Ürün Ekleme", "En az 5 ürün ekle", 
                onboarding.ProductsAdded, true, "Documents"),
            new("Documents", "Belgeler", "Gerekli belgeleri yükle", 
                onboarding.DocumentsUploaded, true)
        };

        var currentStep = steps.FirstOrDefault(s => !s.IsCompleted)?.StepName ?? "Completed";
        var canProceed = onboarding.CompletedSteps >= onboarding.TotalSteps;

        var response = new OnboardingProgressResponse(
            onboarding.Id,
            onboarding.MerchantId,
            steps,
            onboarding.CompletedSteps,
            onboarding.TotalSteps,
            onboarding.ProgressPercentage,
            currentStep,
            canProceed,
            onboarding.IsVerified,
            onboarding.IsApproved);

        return Result.Ok(response);
    }

    public async Task<Result<MerchantOnboardingResponse>> UpdateOnboardingStepAsync(
        UpdateOnboardingStepRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var onboarding = await _unitOfWork.ReadRepository<MerchantOnboarding>()
            .FirstOrDefaultAsync(o => o.Id == request.OnboardingId, cancellationToken: cancellationToken);

        if (onboarding == null)
        {
            return Result.Fail<MerchantOnboardingResponse>("Onboarding not found", "NOT_FOUND_ONBOARDING");
        }

        // Step'i güncelle
        switch (request.StepName.ToLowerInvariant())
        {
            case "basicinfo":
                onboarding.BasicInfoCompleted = request.IsCompleted;
                break;
            case "businessinfo":
                onboarding.BusinessInfoCompleted = request.IsCompleted;
                break;
            case "workinghours":
                onboarding.WorkingHoursCompleted = request.IsCompleted;
                break;
            case "deliveryzones":
                onboarding.DeliveryZonesCompleted = request.IsCompleted;
                break;
            case "products":
                onboarding.ProductsAdded = request.IsCompleted;
                break;
            case "documents":
                onboarding.DocumentsUploaded = request.IsCompleted;
                break;
            default:
                return Result.Fail<MerchantOnboardingResponse>("Invalid step name", "INVALID_STEP");
        }

        // Progress'i güncelle
        onboarding.CompletedSteps = GetCompletedStepsCount(onboarding);
        onboarding.ProgressPercentage = (decimal)onboarding.CompletedSteps / onboarding.TotalSteps * 100;
        onboarding.UpdatedAt = DateTime.UtcNow;

        // Tüm adımlar tamamlandıysa verify et
        if (onboarding.CompletedSteps >= onboarding.TotalSteps && !onboarding.IsVerified)
        {
            onboarding.IsVerified = true;
            onboarding.VerifiedAt = DateTime.UtcNow;
        }

        _unitOfWork.Repository<MerchantOnboarding>().Update(onboarding);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new MerchantOnboardingResponse(
            onboarding.Id,
            onboarding.MerchantId,
            onboarding.OwnerId,
            onboarding.BasicInfoCompleted,
            onboarding.BusinessInfoCompleted,
            onboarding.WorkingHoursCompleted,
            onboarding.DeliveryZonesCompleted,
            onboarding.ProductsAdded,
            onboarding.DocumentsUploaded,
            onboarding.IsVerified,
            onboarding.IsApproved,
            onboarding.RejectionReason,
            onboarding.VerifiedAt,
            onboarding.ApprovedAt,
            onboarding.CompletedSteps,
            onboarding.TotalSteps,
            onboarding.ProgressPercentage,
            onboarding.CreatedAt,
            onboarding.UpdatedAt);

        return Result.Ok(response);
    }

    public async Task<Result> CompleteOnboardingAsync(
        CompleteOnboardingRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var onboarding = await _unitOfWork.ReadRepository<MerchantOnboarding>()
            .FirstOrDefaultAsync(o => o.Id == request.OnboardingId, cancellationToken: cancellationToken);

        if (onboarding == null)
        {
            return Result.Fail("Onboarding not found", "NOT_FOUND_ONBOARDING");
        }

        if (onboarding.CompletedSteps < onboarding.TotalSteps)
        {
            return Result.Fail("All steps must be completed before finishing onboarding", "INCOMPLETE_ONBOARDING");
        }

        onboarding.IsVerified = true;
        onboarding.VerifiedAt = DateTime.UtcNow;
        onboarding.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<MerchantOnboarding>().Update(onboarding);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result<PagedResult<MerchantOnboardingResponse>>> GetPendingApprovalsAsync(
        GetPendingApprovalsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var onboardings = await _unitOfWork.Repository<MerchantOnboarding>().GetPagedAsync(
            filter: o => o.IsVerified && !o.IsApproved,
            orderBy: o => o.VerifiedAt,
            ascending: false,
            page: query.Pagination.Page,
            pageSize: query.Pagination.PageSize,
            include: "Merchant,Owner",
            cancellationToken: cancellationToken);

        var total = await _unitOfWork.ReadRepository<MerchantOnboarding>()
            .CountAsync(o => o.IsVerified && !o.IsApproved, cancellationToken);

        var responses = onboardings.Select(o => new MerchantOnboardingResponse(
            o.Id,
            o.MerchantId,
            o.OwnerId,
            o.BasicInfoCompleted,
            o.BusinessInfoCompleted,
            o.WorkingHoursCompleted,
            o.DeliveryZonesCompleted,
            o.ProductsAdded,
            o.DocumentsUploaded,
            o.IsVerified,
            o.IsApproved,
            o.RejectionReason,
            o.VerifiedAt,
            o.ApprovedAt,
            o.CompletedSteps,
            o.TotalSteps,
            o.ProgressPercentage,
            o.CreatedAt,
            o.UpdatedAt)).ToList();

        var pagedResult = PagedResult<MerchantOnboardingResponse>.Create(
            responses,
            total,
            query.Pagination.Page,
            query.Pagination.PageSize);

        return Result.Ok(pagedResult);
    }

    public async Task<Result> ApproveMerchantAsync(
        ApproveMerchantRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var onboarding = await _unitOfWork.ReadRepository<MerchantOnboarding>()
            .FirstOrDefaultAsync(o => o.Id == request.OnboardingId, 
                include: "Merchant", 
                cancellationToken: cancellationToken);

        if (onboarding == null)
        {
            return Result.Fail("Onboarding not found", "NOT_FOUND_ONBOARDING");
        }

        if (!onboarding.IsVerified)
        {
            return Result.Fail("Merchant must be verified before approval", "NOT_VERIFIED");
        }

        onboarding.IsApproved = request.IsApproved;
        onboarding.RejectionReason = request.IsApproved ? null : request.RejectionReason;
        onboarding.ApprovedAt = DateTime.UtcNow;
        onboarding.UpdatedAt = DateTime.UtcNow;

        // Merchant'ı aktif et
        if (request.IsApproved)
        {
            onboarding.Merchant.IsActive = true;
            _unitOfWork.Repository<Merchant>().Update(onboarding.Merchant);
        }

        _unitOfWork.Repository<MerchantOnboarding>().Update(onboarding);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result<List<OnboardingStepResponse>>> GetOnboardingStepsAsync(
        CancellationToken cancellationToken = default)
    {
        var steps = new List<OnboardingStepResponse>
        {
            new("BasicInfo", "Temel Bilgiler", "İşletme adı, adres ve iletişim bilgileri", false, true, "BusinessInfo"),
            new("BusinessInfo", "İş Bilgileri", "Açıklama, kategori ve teslimat bilgileri", false, true, "WorkingHours"),
            new("WorkingHours", "Çalışma Saatleri", "Haftalık çalışma saatlerini belirle", false, true, "DeliveryZones"),
            new("DeliveryZones", "Teslimat Bölgeleri", "Hizmet verilecek bölgeleri tanımla", false, true, "Products"),
            new("Products", "Ürün Ekleme", "En az 5 ürün ekle", false, true, "Documents"),
            new("Documents", "Belgeler", "Gerekli belgeleri yükle", false, true)
        };

        return Result.Ok(steps);
    }

    public async Task<Result<bool>> CanMerchantStartTradingAsync(
        Guid merchantId,
        CancellationToken cancellationToken = default)
    {
        var onboarding = await _unitOfWork.ReadRepository<MerchantOnboarding>()
            .FirstOrDefaultAsync(o => o.MerchantId == merchantId, cancellationToken: cancellationToken);

        if (onboarding == null)
        {
            return Result.Ok(false);
        }

        var canStart = onboarding.IsVerified && onboarding.IsApproved;
        return Result.Ok(canStart);
    }

    private static int GetCompletedStepsCount(MerchantOnboarding onboarding)
    {
        var count = 0;
        if (onboarding.BasicInfoCompleted) count++;
        if (onboarding.BusinessInfoCompleted) count++;
        if (onboarding.WorkingHoursCompleted) count++;
        if (onboarding.DeliveryZonesCompleted) count++;
        if (onboarding.ProductsAdded) count++;
        if (onboarding.DocumentsUploaded) count++;
        return count;
    }

    #region Additional Controller Methods

    public async Task<Result<OnboardingStepResponse>> CompleteOnboardingStepAsync(
        Guid merchantId,
        Guid stepId,
        CompleteOnboardingStepRequest request,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await CompleteOnboardingStepInternalAsync(merchantId, stepId, request, cancellationToken),
            "CompleteOnboardingStep",
            new { merchantId, stepId, request.StepName },
            cancellationToken);
    }

    private async Task<Result<OnboardingStepResponse>> CompleteOnboardingStepInternalAsync(
        Guid merchantId,
        Guid stepId,
        CompleteOnboardingStepRequest request,
        CancellationToken cancellationToken)
    {
        var onboarding = await _unitOfWork.ReadRepository<MerchantOnboarding>()
            .FirstOrDefaultAsync(o => o.MerchantId == merchantId, cancellationToken: cancellationToken);

        if (onboarding == null)
        {
            return Result.Fail<OnboardingStepResponse>("Onboarding not found", "ONBOARDING_NOT_FOUND");
        }

        // Simplified step completion logic
        var response = new OnboardingStepResponse(
            StepName: "StepName",
            DisplayName: "Step Title",
            Description: "Step Description",
            IsCompleted: true,
            IsRequired: true
        );

        return Result.Ok(response);
    }

    public async Task<Result> SubmitOnboardingAsync(
        Guid merchantId,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await SubmitOnboardingInternalAsync(merchantId, cancellationToken),
            "SubmitOnboarding",
            new { merchantId },
            cancellationToken);
    }

    private async Task<Result> SubmitOnboardingInternalAsync(
        Guid merchantId,
        CancellationToken cancellationToken)
    {
        var onboarding = await _unitOfWork.ReadRepository<MerchantOnboarding>()
            .FirstOrDefaultAsync(o => o.MerchantId == merchantId, cancellationToken: cancellationToken);

        if (onboarding == null)
        {
            return Result.Fail("Onboarding not found", "ONBOARDING_NOT_FOUND");
        }

        // Simplified submission logic
        onboarding.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Repository<MerchantOnboarding>().Update(onboarding);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result<OnboardingChecklistResponse>> GetOnboardingChecklistAsync(
        Guid merchantId,
        CancellationToken cancellationToken = default)
    {
        return await ExecuteWithPerformanceTracking(
            async () => await GetOnboardingChecklistInternalAsync(merchantId, cancellationToken),
            "GetOnboardingChecklist",
            new { merchantId },
            cancellationToken);
    }

    private async Task<Result<OnboardingChecklistResponse>> GetOnboardingChecklistInternalAsync(
        Guid merchantId,
        CancellationToken cancellationToken)
    {
        var onboarding = await _unitOfWork.ReadRepository<MerchantOnboarding>()
            .FirstOrDefaultAsync(o => o.MerchantId == merchantId, cancellationToken: cancellationToken);

        if (onboarding == null)
        {
            return Result.Fail<OnboardingChecklistResponse>("Onboarding not found", "ONBOARDING_NOT_FOUND");
        }

        // Simplified checklist implementation
        var checklist = new List<OnboardingChecklistItem>
        {
            new("Basic Information", "Complete your basic business information", onboarding.BasicInfoCompleted, true),
            new("Business Details", "Add your business details and description", onboarding.BusinessInfoCompleted, true),
            new("Working Hours", "Set your working hours", onboarding.WorkingHoursCompleted, true),
            new("Delivery Zones", "Configure your delivery zones", onboarding.DeliveryZonesCompleted, true),
            new("Products", "Add at least one product", onboarding.ProductsAdded, true),
            new("Documents", "Upload required documents", onboarding.DocumentsUploaded, true)
        };

        var response = new OnboardingChecklistResponse(
            MerchantId: merchantId,
            Items: checklist,
            CompletedCount: checklist.Count(item => item.IsCompleted),
            TotalCount: checklist.Count,
            CompletionPercentage: checklist.Count > 0 ? (decimal)checklist.Count(item => item.IsCompleted) / checklist.Count * 100 : 0
        );

        return Result.Ok(response);
    }

    #endregion
}
