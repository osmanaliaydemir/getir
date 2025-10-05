using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Internationalization;

public interface ITranslationService
{
    Task<TranslationDto?> GetTranslationAsync(string key, LanguageCode languageCode);
    Task<GetTranslationsByKeysResponse> GetTranslationsByKeysAsync(GetTranslationsByKeysRequest request);
    Task<TranslationSearchResponse> SearchTranslationsAsync(TranslationSearchRequest request);
    Task<TranslationDto> CreateTranslationAsync(CreateTranslationRequest request);
    Task<TranslationDto> UpdateTranslationAsync(Guid id, UpdateTranslationRequest request);
    Task<bool> DeleteTranslationAsync(Guid id);
    Task<bool> BulkCreateTranslationsAsync(BulkTranslationRequest request);
    Task<bool> BulkUpdateTranslationsAsync(BulkTranslationRequest request);
    Task<List<TranslationDto>> GetTranslationsByCategoryAsync(string category, LanguageCode languageCode);
    Task<Dictionary<string, string>> GetTranslationsDictionaryAsync(LanguageCode languageCode, string? category = null);
    Task<List<string>> GetMissingTranslationsAsync(LanguageCode languageCode, string? category = null);
    Task<bool> ImportTranslationsFromJsonAsync(string jsonContent, LanguageCode languageCode, string? category = null);
    Task<string> ExportTranslationsToJsonAsync(LanguageCode languageCode, string? category = null);
}
