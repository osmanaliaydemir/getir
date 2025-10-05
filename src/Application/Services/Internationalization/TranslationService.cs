using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Getir.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Getir.Application.Services.Internationalization;

public class TranslationService : ITranslationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TranslationService> _logger;

    public TranslationService(AppDbContext context, ILogger<TranslationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<TranslationDto>> GetTranslationAsync(string key, LanguageCode languageCode)
    {
        try
        {
            var translation = await _context.Translations
                .Where(t => t.Key == key && t.LanguageCode == languageCode && t.IsActive)
                .Select(t => new TranslationDto
                {
                    Id = t.Id,
                    Key = t.Key,
                    Value = t.Value,
                    LanguageCode = t.LanguageCode,
                    Category = t.Category,
                    Context = t.Context,
                    Description = t.Description,
                    IsActive = t.IsActive
                })
                .FirstOrDefaultAsync();

            if (translation == null)
            {
                // Fallback to default language (Turkish)
                if (languageCode != LanguageCode.Turkish)
                {
                    return await GetTranslationAsync(key, LanguageCode.Turkish);
                }
                return Result<TranslationDto>.Failure($"Translation not found for key: {key}");
            }

            return Result<TranslationDto>.Success(translation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting translation: {Key}, {LanguageCode}", key, languageCode);
            return Result<TranslationDto>.Failure("Error getting translation");
        }
    }

    public async Task<Result<GetTranslationsByKeysResponse>> GetTranslationsByKeysAsync(GetTranslationsByKeysRequest request)
    {
        try
        {
            var translations = await _context.Translations
                .Where(t => request.Keys.Contains(t.Key) && 
                           t.LanguageCode == request.LanguageCode && 
                           t.IsActive &&
                           (request.Category == null || t.Category == request.Category))
                .ToDictionaryAsync(t => t.Key, t => t.Value);

            var response = new GetTranslationsByKeysResponse
            {
                Translations = translations,
                LanguageCode = request.LanguageCode,
                FoundCount = translations.Count,
                MissingCount = request.Keys.Count - translations.Count,
                MissingKeys = request.Keys.Where(k => !translations.ContainsKey(k)).ToList()
            };

            return Result<GetTranslationsByKeysResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting translations by keys: {Request}", request);
            return Result<GetTranslationsByKeysResponse>.Failure("Error getting translations");
        }
    }

    public async Task<Result<TranslationSearchResponse>> SearchTranslationsAsync(TranslationSearchRequest request)
    {
        try
        {
            var query = _context.Translations.AsQueryable();

            if (!string.IsNullOrEmpty(request.Key))
            {
                query = query.Where(t => t.Key.Contains(request.Key));
            }

            if (request.LanguageCode.HasValue)
            {
                query = query.Where(t => t.LanguageCode == request.LanguageCode.Value);
            }

            if (!string.IsNullOrEmpty(request.Category))
            {
                query = query.Where(t => t.Category == request.Category);
            }

            if (!string.IsNullOrEmpty(request.Context))
            {
                query = query.Where(t => t.Context != null && t.Context.Contains(request.Context));
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(t => t.IsActive == request.IsActive.Value);
            }

            var totalCount = await query.CountAsync();

            var translations = await query
                .OrderBy(t => t.Key)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(t => new TranslationDto
                {
                    Id = t.Id,
                    Key = t.Key,
                    Value = t.Value,
                    LanguageCode = t.LanguageCode,
                    Category = t.Category,
                    Context = t.Context,
                    Description = t.Description,
                    IsActive = t.IsActive
                })
                .ToListAsync();

            var response = new TranslationSearchResponse
            {
                Translations = translations,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
            };

            return Result<TranslationSearchResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching translations: {Request}", request);
            return Result<TranslationSearchResponse>.Failure("Error searching translations");
        }
    }

    public async Task<Result<TranslationDto>> CreateTranslationAsync(CreateTranslationRequest request)
    {
        try
        {
            // Check if translation already exists
            var existingTranslation = await _context.Translations
                .FirstOrDefaultAsync(t => t.Key == request.Key && t.LanguageCode == request.LanguageCode);

            if (existingTranslation != null)
            {
                return Result<TranslationDto>.Failure("Translation already exists for this key and language");
            }

            var translation = new Translation
            {
                Id = Guid.NewGuid(),
                Key = request.Key,
                Value = request.Value,
                LanguageCode = request.LanguageCode,
                Category = request.Category,
                Context = request.Context,
                Description = request.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Translations.Add(translation);
            await _context.SaveChangesAsync();

            var translationDto = new TranslationDto
            {
                Id = translation.Id,
                Key = translation.Key,
                Value = translation.Value,
                LanguageCode = translation.LanguageCode,
                Category = translation.Category,
                Context = translation.Context,
                Description = translation.Description,
                IsActive = translation.IsActive
            };

            return Result<TranslationDto>.Success(translationDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating translation: {Request}", request);
            return Result<TranslationDto>.Failure("Error creating translation");
        }
    }

    public async Task<Result<TranslationDto>> UpdateTranslationAsync(Guid id, UpdateTranslationRequest request)
    {
        try
        {
            var translation = await _context.Translations.FindAsync(id);
            if (translation == null)
            {
                return Result<TranslationDto>.Failure("Translation not found");
            }

            translation.Value = request.Value;
            translation.Category = request.Category;
            translation.Context = request.Context;
            translation.Description = request.Description;
            translation.IsActive = request.IsActive;
            translation.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var translationDto = new TranslationDto
            {
                Id = translation.Id,
                Key = translation.Key,
                Value = translation.Value,
                LanguageCode = translation.LanguageCode,
                Category = translation.Category,
                Context = translation.Context,
                Description = translation.Description,
                IsActive = translation.IsActive
            };

            return Result<TranslationDto>.Success(translationDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating translation: {Id}, {Request}", id, request);
            return Result<TranslationDto>.Failure("Error updating translation");
        }
    }

    public async Task<Result<bool>> DeleteTranslationAsync(Guid id)
    {
        try
        {
            var translation = await _context.Translations.FindAsync(id);
            if (translation == null)
            {
                return Result<bool>.Failure("Translation not found");
            }

            _context.Translations.Remove(translation);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting translation: {Id}", id);
            return Result<bool>.Failure("Error deleting translation");
        }
    }

    public async Task<Result<bool>> BulkCreateTranslationsAsync(BulkTranslationRequest request)
    {
        try
        {
            var translations = new List<Translation>();

            foreach (var item in request.Translations)
            {
                // Check if translation already exists
                var existing = await _context.Translations
                    .FirstOrDefaultAsync(t => t.Key == item.Key && t.LanguageCode == request.LanguageCode);

                if (existing == null)
                {
                    translations.Add(new Translation
                    {
                        Id = Guid.NewGuid(),
                        Key = item.Key,
                        Value = item.Value,
                        LanguageCode = request.LanguageCode,
                        Category = request.Category,
                        Context = item.Context,
                        Description = item.Description,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            if (translations.Any())
            {
                _context.Translations.AddRange(translations);
                await _context.SaveChangesAsync();
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk creating translations: {Request}", request);
            return Result<bool>.Failure("Error bulk creating translations");
        }
    }

    public async Task<Result<bool>> BulkUpdateTranslationsAsync(BulkTranslationRequest request)
    {
        try
        {
            var keys = request.Translations.Select(t => t.Key).ToList();
            var existingTranslations = await _context.Translations
                .Where(t => keys.Contains(t.Key) && t.LanguageCode == request.LanguageCode)
                .ToListAsync();

            foreach (var item in request.Translations)
            {
                var existing = existingTranslations.FirstOrDefault(t => t.Key == item.Key);
                if (existing != null)
                {
                    existing.Value = item.Value;
                    existing.Context = item.Context;
                    existing.Description = item.Description;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating translations: {Request}", request);
            return Result<bool>.Failure("Error bulk updating translations");
        }
    }

    public async Task<Result<List<TranslationDto>>> GetTranslationsByCategoryAsync(string category, LanguageCode languageCode)
    {
        try
        {
            var translations = await _context.Translations
                .Where(t => t.Category == category && t.LanguageCode == languageCode && t.IsActive)
                .OrderBy(t => t.Key)
                .Select(t => new TranslationDto
                {
                    Id = t.Id,
                    Key = t.Key,
                    Value = t.Value,
                    LanguageCode = t.LanguageCode,
                    Category = t.Category,
                    Context = t.Context,
                    Description = t.Description,
                    IsActive = t.IsActive
                })
                .ToListAsync();

            return Result<List<TranslationDto>>.Success(translations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting translations by category: {Category}, {LanguageCode}", category, languageCode);
            return Result<List<TranslationDto>>.Failure("Error getting translations by category");
        }
    }

    public async Task<Result<Dictionary<string, string>>> GetTranslationsDictionaryAsync(LanguageCode languageCode, string? category = null)
    {
        try
        {
            var query = _context.Translations
                .Where(t => t.LanguageCode == languageCode && t.IsActive);

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(t => t.Category == category);
            }

            var translations = await query
                .ToDictionaryAsync(t => t.Key, t => t.Value);

            return Result<Dictionary<string, string>>.Success(translations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting translations dictionary: {LanguageCode}, {Category}", languageCode, category);
            return Result<Dictionary<string, string>>.Failure("Error getting translations dictionary");
        }
    }

    public async Task<Result<List<string>>> GetMissingTranslationsAsync(LanguageCode languageCode, string? category = null)
    {
        try
        {
            // Get all unique keys from all languages
            var allKeys = await _context.Translations
                .Where(t => string.IsNullOrEmpty(category) || t.Category == category)
                .Select(t => t.Key)
                .Distinct()
                .ToListAsync();

            // Get existing keys for the specified language
            var existingKeys = await _context.Translations
                .Where(t => t.LanguageCode == languageCode && 
                           (string.IsNullOrEmpty(category) || t.Category == category))
                .Select(t => t.Key)
                .ToListAsync();

            var missingKeys = allKeys.Except(existingKeys).ToList();

            return Result<List<string>>.Success(missingKeys);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting missing translations: {LanguageCode}, {Category}", languageCode, category);
            return Result<List<string>>.Failure("Error getting missing translations");
        }
    }

    public async Task<Result<bool>> ImportTranslationsFromJsonAsync(string jsonContent, LanguageCode languageCode, string? category = null)
    {
        try
        {
            var translations = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
            if (translations == null)
            {
                return Result<bool>.Failure("Invalid JSON format");
            }

            var translationEntities = new List<Translation>();

            foreach (var kvp in translations)
            {
                // Check if translation already exists
                var existing = await _context.Translations
                    .FirstOrDefaultAsync(t => t.Key == kvp.Key && t.LanguageCode == languageCode);

                if (existing == null)
                {
                    translationEntities.Add(new Translation
                    {
                        Id = Guid.NewGuid(),
                        Key = kvp.Key,
                        Value = kvp.Value,
                        LanguageCode = languageCode,
                        Category = category,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            if (translationEntities.Any())
            {
                _context.Translations.AddRange(translationEntities);
                await _context.SaveChangesAsync();
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing translations from JSON: {LanguageCode}, {Category}", languageCode, category);
            return Result<bool>.Failure("Error importing translations");
        }
    }

    public async Task<Result<string>> ExportTranslationsToJsonAsync(LanguageCode languageCode, string? category = null)
    {
        try
        {
            var query = _context.Translations
                .Where(t => t.LanguageCode == languageCode && t.IsActive);

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(t => t.Category == category);
            }

            var translations = await query
                .ToDictionaryAsync(t => t.Key, t => t.Value);

            var json = JsonSerializer.Serialize(translations, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            return Result<string>.Success(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting translations to JSON: {LanguageCode}, {Category}", languageCode, category);
            return Result<string>.Failure("Error exporting translations");
        }
    }
}
