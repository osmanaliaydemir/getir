using Getir.Application.Common;
using Getir.Application.DTO;

namespace Getir.Application.Services.SpecialHolidays;

public interface ISpecialHolidayService
{
    /// <summary>
    /// Tüm özel tatilleri listele (aktif olanlar)
    /// </summary>
    Task<Result<List<SpecialHolidayResponse>>> GetAllSpecialHolidaysAsync(CancellationToken cancellationToken = default);
    /// <summary>
    /// Merchant'a ait tüm özel tatilleri getir
    /// </summary>
    Task<Result<List<SpecialHolidayResponse>>> GetSpecialHolidaysByMerchantAsync(Guid merchantId, bool includeInactive = false, CancellationToken cancellationToken = default);
    /// <summary>
    /// Belirli bir tarih aralığındaki özel tatilleri getir
    /// </summary>
    Task<Result<List<SpecialHolidayResponse>>> GetSpecialHolidaysByDateRangeAsync(Guid merchantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    /// <summary>
    /// ID'ye göre özel tatil getir
    /// </summary>
    Task<Result<SpecialHolidayResponse>> GetSpecialHolidayByIdAsync(Guid id, CancellationToken cancellationToken = default);
    /// <summary>
    /// Yeni özel tatil oluştur
    /// </summary>
    Task<Result<SpecialHolidayResponse>> CreateSpecialHolidayAsync(CreateSpecialHolidayRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Özel tatil güncelle
    /// </summary>
    Task<Result<SpecialHolidayResponse>> UpdateSpecialHolidayAsync(Guid id, UpdateSpecialHolidayRequest request, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Özel tatil sil
    /// </summary>
    Task<Result> DeleteSpecialHolidayAsync(Guid id, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Özel tatili aktif/pasif yap
    /// </summary>
    Task<Result> ToggleSpecialHolidayStatusAsync(Guid id, Guid merchantOwnerId, CancellationToken cancellationToken = default);
    /// <summary>
    /// Belirli bir tarihte merchant'ın özel tatilde olup olmadığını kontrol et
    /// </summary>
    Task<Result<MerchantAvailabilityResponse>> CheckMerchantAvailabilityAsync(Guid merchantId, DateTime checkDate, CancellationToken cancellationToken = default);
    /// <summary>
    /// Aktif olan gelecek özel tatilleri getir
    /// </summary>
    Task<Result<List<SpecialHolidayResponse>>> GetUpcomingSpecialHolidaysAsync(Guid merchantId, CancellationToken cancellationToken = default);
}

