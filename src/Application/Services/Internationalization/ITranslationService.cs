using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Internationalization;

/// <summary>
/// Çeviri yönetimi servisi: key-value bazlı çeviri CRUD işlemleri, arama, toplu işlemler, import/export.
/// </summary>
public interface ITranslationService
{
    /// <summary>Belirtilen anahtar ve dil için çeviri getirir (cache).</summary>
    Task<TranslationDto?> GetTranslationAsync(string key, LanguageCode languageCode);
    /// <summary>Birden fazla anahtar için çevirileri getirir.</summary>
    Task<GetTranslationsByKeysResponse> GetTranslationsByKeysAsync(GetTranslationsByKeysRequest request);
    /// <summary>Çevirilerde arama yapar (filtreleme ve sayfalama).</summary>
    Task<TranslationSearchResponse> SearchTranslationsAsync(TranslationSearchRequest request);
    /// <summary>Yeni çeviri oluşturur (cache invalidation).</summary>
    Task<TranslationDto> CreateTranslationAsync(CreateTranslationRequest request);
    /// <summary>Çeviri günceller (cache invalidation).</summary>
    Task<TranslationDto> UpdateTranslationAsync(Guid id, UpdateTranslationRequest request);
    /// <summary>Çeviri siler (cache invalidation).</summary>
    Task<bool> DeleteTranslationAsync(Guid id);
    /// <summary>Toplu çeviri oluşturur.</summary>
    Task<bool> BulkCreateTranslationsAsync(BulkTranslationRequest request);
    /// <summary>Toplu çeviri günceller.</summary>
    Task<bool> BulkUpdateTranslationsAsync(BulkTranslationRequest request);
    /// <summary>Kategoriye göre çevirileri getirir.</summary>
    Task<List<TranslationDto>> GetTranslationsByCategoryAsync(string category, LanguageCode languageCode);
    /// <summary>Çevirileri sözlük formatında getirir (cache).</summary>
    Task<Dictionary<string, string>> GetTranslationsDictionaryAsync(LanguageCode languageCode, string? category = null);
    /// <summary>Eksik çevirileri getirir.</summary>
    Task<List<string>> GetMissingTranslationsAsync(LanguageCode languageCode, string? category = null);
    /// <summary>JSON'dan çevirileri içe aktarır.</summary>
    Task<bool> ImportTranslationsFromJsonAsync(string jsonContent, LanguageCode languageCode, string? category = null);
    /// <summary>Çevirileri JSON formatında dışa aktarır.</summary>
    Task<string> ExportTranslationsToJsonAsync(LanguageCode languageCode, string? category = null);
}
