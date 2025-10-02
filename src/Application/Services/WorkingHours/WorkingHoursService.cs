using Getir.Application.Abstractions;
using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.WorkingHours;

public class WorkingHoursService : BaseService, IWorkingHoursService
{
    private readonly IBackgroundTaskService _backgroundTaskService;

    public WorkingHoursService(
        IUnitOfWork unitOfWork,
        ILogger<WorkingHoursService> logger,
        ILoggingService loggingService,
        ICacheService cacheService,
        IBackgroundTaskService backgroundTaskService) 
        : base(unitOfWork, logger, loggingService, cacheService)
    {
        _backgroundTaskService = backgroundTaskService;
    }

    public async Task<Result<List<WorkingHoursResponse>>> GetWorkingHoursByMerchantAsync(
        Guid merchantId,
        CancellationToken cancellationToken = default)
    {
        var workingHours = await _unitOfWork.ReadRepository<Domain.Entities.WorkingHours>()
            .ListAsync(
                filter: wh => wh.MerchantId == merchantId,
                orderBy: wh => wh.DayOfWeek,
                cancellationToken: cancellationToken);

        var response = workingHours.Select(wh => new WorkingHoursResponse(
            wh.Id,
            wh.MerchantId,
            wh.DayOfWeek,
            wh.OpenTime,
            wh.CloseTime,
            wh.IsClosed,
            wh.CreatedAt
        )).ToList();

        return Result.Ok(response);
    }

    public async Task<Result<WorkingHoursResponse>> GetWorkingHoursByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var workingHours = await _unitOfWork.Repository<Domain.Entities.WorkingHours>()
            .GetAsync(wh => wh.Id == id, include: "Merchant", cancellationToken: cancellationToken);

        if (workingHours == null)
        {
            return Result.Fail<WorkingHoursResponse>("Working hours not found", "NOT_FOUND_WORKING_HOURS");
        }

        var response = new WorkingHoursResponse(
            workingHours.Id,
            workingHours.MerchantId,
            workingHours.DayOfWeek,
            workingHours.OpenTime,
            workingHours.CloseTime,
            workingHours.IsClosed,
            workingHours.CreatedAt
        );

        return Result.Ok(response);
    }

    public async Task<Result<WorkingHoursResponse>> CreateWorkingHoursAsync(
        CreateWorkingHoursRequest request,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default)
    {
        // Merchant ownership kontrolü
        var merchant = await _unitOfWork.ReadRepository<Merchant>()
            .FirstOrDefaultAsync(m => m.Id == request.MerchantId && m.OwnerId == merchantOwnerId, cancellationToken: cancellationToken);

        if (merchant == null)
        {
            return Result.Fail<WorkingHoursResponse>("Merchant not found or access denied", "NOT_FOUND_MERCHANT");
        }

        // Aynı gün için zaten kayıt var mı kontrol et
        var existingWorkingHours = await _unitOfWork.ReadRepository<Domain.Entities.WorkingHours>()
            .FirstOrDefaultAsync(
                wh => wh.MerchantId == request.MerchantId && wh.DayOfWeek == request.DayOfWeek,
                cancellationToken: cancellationToken);

        if (existingWorkingHours != null)
        {
            return Result.Fail<WorkingHoursResponse>("Working hours for this day already exists", "CONFLICT_WORKING_HOURS");
        }

        var workingHours = new Domain.Entities.WorkingHours
        {
            Id = Guid.NewGuid(),
            MerchantId = request.MerchantId,
            DayOfWeek = request.DayOfWeek,
            OpenTime = request.OpenTime,
            CloseTime = request.CloseTime,
            IsClosed = request.IsClosed,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Domain.Entities.WorkingHours>().AddAsync(workingHours, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new WorkingHoursResponse(
            workingHours.Id,
            workingHours.MerchantId,
            workingHours.DayOfWeek,
            workingHours.OpenTime,
            workingHours.CloseTime,
            workingHours.IsClosed,
            workingHours.CreatedAt
        );

        return Result.Ok(response);
    }

    public async Task<Result<WorkingHoursResponse>> UpdateWorkingHoursAsync(
        Guid id,
        UpdateWorkingHoursRequest request,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default)
    {
        var workingHours = await _unitOfWork.Repository<Domain.Entities.WorkingHours>()
            .GetAsync(
                wh => wh.Id == id,
                include: "Merchant",
                cancellationToken: cancellationToken);

        if (workingHours == null)
        {
            return Result.Fail<WorkingHoursResponse>("Working hours not found", "NOT_FOUND_WORKING_HOURS");
        }

        // Merchant ownership kontrolü
        if (workingHours.Merchant.OwnerId != merchantOwnerId)
        {
            return Result.Fail<WorkingHoursResponse>("Access denied", "FORBIDDEN_WORKING_HOURS");
        }

        workingHours.OpenTime = request.OpenTime;
        workingHours.CloseTime = request.CloseTime;
        workingHours.IsClosed = request.IsClosed;
        workingHours.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Repository<Domain.Entities.WorkingHours>().Update(workingHours);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new WorkingHoursResponse(
            workingHours.Id,
            workingHours.MerchantId,
            workingHours.DayOfWeek,
            workingHours.OpenTime,
            workingHours.CloseTime,
            workingHours.IsClosed,
            workingHours.CreatedAt
        );

        return Result.Ok(response);
    }

    public async Task<Result> DeleteWorkingHoursAsync(
        Guid id,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default)
    {
        var workingHours = await _unitOfWork.Repository<Domain.Entities.WorkingHours>()
            .GetAsync(
                wh => wh.Id == id,
                include: "Merchant",
                cancellationToken: cancellationToken);

        if (workingHours == null)
        {
            return Result.Fail("Working hours not found", "NOT_FOUND_WORKING_HOURS");
        }

        // Merchant ownership kontrolü
        if (workingHours.Merchant.OwnerId != merchantOwnerId)
        {
            return Result.Fail("Access denied", "FORBIDDEN_WORKING_HOURS");
        }

        _unitOfWork.Repository<Domain.Entities.WorkingHours>().Delete(workingHours);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<Result> BulkUpdateWorkingHoursAsync(
        Guid merchantId,
        BulkUpdateWorkingHoursRequest request,
        Guid merchantOwnerId,
        CancellationToken cancellationToken = default)
    {
        // Merchant ownership kontrolü
        var merchant = await _unitOfWork.ReadRepository<Merchant>()
            .FirstOrDefaultAsync(m => m.Id == merchantId && m.OwnerId == merchantOwnerId, cancellationToken: cancellationToken);

        if (merchant == null)
        {
            return Result.Fail("Merchant not found or access denied", "NOT_FOUND_MERCHANT");
        }

        // Mevcut working hours'ları al
        var existingWorkingHours = await _unitOfWork.Repository<Domain.Entities.WorkingHours>()
            .ListAsync(filter: wh => wh.MerchantId == merchantId, cancellationToken: cancellationToken);

        foreach (var dayRequest in request.WorkingHours)
        {
            var existing = existingWorkingHours.FirstOrDefault(wh => wh.DayOfWeek == dayRequest.DayOfWeek);
            
            if (existing != null)
            {
                // Güncelle
                existing.OpenTime = dayRequest.OpenTime;
                existing.CloseTime = dayRequest.CloseTime;
                existing.IsClosed = dayRequest.IsClosed;
                existing.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<Domain.Entities.WorkingHours>().Update(existing);
            }
            else
            {
                // Yeni oluştur
                var newWorkingHours = new Domain.Entities.WorkingHours
                {
                    Id = Guid.NewGuid(),
                    MerchantId = merchantId,
                    DayOfWeek = dayRequest.DayOfWeek,
                    OpenTime = dayRequest.OpenTime,
                    CloseTime = dayRequest.CloseTime,
                    IsClosed = dayRequest.IsClosed,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Repository<Domain.Entities.WorkingHours>().AddAsync(newWorkingHours, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result<bool>> IsMerchantOpenAsync(
        Guid merchantId,
        DateTime? checkTime = null,
        CancellationToken cancellationToken = default)
    {
        var timeToCheck = checkTime ?? DateTime.UtcNow;
        var dayOfWeek = timeToCheck.DayOfWeek;

        var workingHours = await _unitOfWork.ReadRepository<Domain.Entities.WorkingHours>()
            .FirstOrDefaultAsync(
                wh => wh.MerchantId == merchantId && wh.DayOfWeek == dayOfWeek,
                cancellationToken: cancellationToken);

        if (workingHours == null || workingHours.IsClosed)
        {
            return Result.Ok(false);
        }

        if (workingHours.OpenTime == null || workingHours.CloseTime == null)
        {
            return Result.Ok(false);
        }

        var currentTime = timeToCheck.TimeOfDay;
        var isOpen = currentTime >= workingHours.OpenTime.Value && currentTime <= workingHours.CloseTime.Value;

        return Result.Ok(isOpen);
    }
}
