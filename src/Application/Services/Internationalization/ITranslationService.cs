using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Enums;

namespace Getir.Application.Services.Internationalization;

public interface ITranslationService
{
    Task<Result<TranslationDto>> GetTranslationAsync(string key, LanguageCode languageCode);
    Task<Result<GetTranslationsByKeysResponse>> GetTranslationsByKeysAsync(GetTranslationsByKeysRequest request);
    Task<Result<TranslationSearchResponse>> SearchTranslationsAsync(TranslationSearchRequest request);
    Task<Result<TranslationDto>> CreateTranslationAsync(CreateTranslationRequest request);
    Task<Result<TranslationDto>> UpdateTranslationAsync(Guid id, UpdateTranslationRequest request);
    Task<Result<bool>> DeleteTranslationAsync(Guid id);
    Task<Result<bool>> BulkCreateTranslationsAsync(BulkTranslationRequest request);
    Task<Result<bool>> BulkUpdateTranslationsAsync(BulkTranslationRequest request);
    Task<Result<List<TranslationDto>>> GetTranslationsByCategoryAsync(string category, LanguageCode languageCode);
    Task<Result<Dictionary<string, string>>> GetTranslationsDictionaryAsync(LanguageCode languageCode, string? category = null);
    Task<Result<List<string>>> GetMissingTranslationsAsync(LanguageCode languageCode, string? category = null);
    Task<Result<bool>> ImportTranslationsFromJsonAsync(string jsonContent, LanguageCode languageCode, string? category = null);
    Task<Result<string>> ExportTranslationsToJsonAsync(LanguageCode languageCode, string? category = null);
}
