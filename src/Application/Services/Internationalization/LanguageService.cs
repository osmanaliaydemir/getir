using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Getir.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.Internationalization;

public class LanguageService : ILanguageService
{
    private readonly AppDbContext _context;
    private readonly ILogger<LanguageService> _logger;

    public LanguageService(AppDbContext context, ILogger<LanguageService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<List<LanguageDto>>> GetAllLanguagesAsync()
    {
        try
        {
            var languages = await _context.Languages
                .Where(l => l.IsActive)
                .OrderBy(l => l.SortOrder)
                .ThenBy(l => l.Name)
                .Select(l => new LanguageDto
                {
                    Id = l.Id,
                    Code = l.Code,
                    Name = l.Name,
                    NativeName = l.NativeName,
                    CultureCode = l.CultureCode,
                    IsRtl = l.IsRtl,
                    IsActive = l.IsActive,
                    IsDefault = l.IsDefault,
                    SortOrder = l.SortOrder,
                    FlagIcon = l.FlagIcon
                })
                .ToListAsync();

            return Result<List<LanguageDto>>.Success(languages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all languages");
            return Result<List<LanguageDto>>.Failure("Error getting languages");
        }
    }

    public async Task<Result<LanguageDto>> GetLanguageByIdAsync(Guid id)
    {
        try
        {
            var language = await _context.Languages
                .Where(l => l.Id == id)
                .Select(l => new LanguageDto
                {
                    Id = l.Id,
                    Code = l.Code,
                    Name = l.Name,
                    NativeName = l.NativeName,
                    CultureCode = l.CultureCode,
                    IsRtl = l.IsRtl,
                    IsActive = l.IsActive,
                    IsDefault = l.IsDefault,
                    SortOrder = l.SortOrder,
                    FlagIcon = l.FlagIcon
                })
                .FirstOrDefaultAsync();

            if (language == null)
            {
                return Result<LanguageDto>.Failure("Language not found");
            }

            return Result<LanguageDto>.Success(language);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting language by id: {Id}", id);
            return Result<LanguageDto>.Failure("Error getting language");
        }
    }

    public async Task<Result<LanguageDto>> GetLanguageByCodeAsync(LanguageCode code)
    {
        try
        {
            var language = await _context.Languages
                .Where(l => l.Code == code && l.IsActive)
                .Select(l => new LanguageDto
                {
                    Id = l.Id,
                    Code = l.Code,
                    Name = l.Name,
                    NativeName = l.NativeName,
                    CultureCode = l.CultureCode,
                    IsRtl = l.IsRtl,
                    IsActive = l.IsActive,
                    IsDefault = l.IsDefault,
                    SortOrder = l.SortOrder,
                    FlagIcon = l.FlagIcon
                })
                .FirstOrDefaultAsync();

            if (language == null)
            {
                return Result<LanguageDto>.Failure("Language not found");
            }

            return Result<LanguageDto>.Success(language);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting language by code: {Code}", code);
            return Result<LanguageDto>.Failure("Error getting language");
        }
    }

    public async Task<Result<LanguageDto>> GetDefaultLanguageAsync()
    {
        try
        {
            var language = await _context.Languages
                .Where(l => l.IsDefault && l.IsActive)
                .Select(l => new LanguageDto
                {
                    Id = l.Id,
                    Code = l.Code,
                    Name = l.Name,
                    NativeName = l.NativeName,
                    CultureCode = l.CultureCode,
                    IsRtl = l.IsRtl,
                    IsActive = l.IsActive,
                    IsDefault = l.IsDefault,
                    SortOrder = l.SortOrder,
                    FlagIcon = l.FlagIcon
                })
                .FirstOrDefaultAsync();

            if (language == null)
            {
                // Fallback to Turkish if no default is set
                return await GetLanguageByCodeAsync(LanguageCode.Turkish);
            }

            return Result<LanguageDto>.Success(language);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting default language");
            return Result<LanguageDto>.Failure("Error getting default language");
        }
    }

    public async Task<Result<LanguageDto>> CreateLanguageAsync(CreateLanguageRequest request)
    {
        try
        {
            // Check if language code already exists
            var existingLanguage = await _context.Languages
                .FirstOrDefaultAsync(l => l.Code == request.Code);

            if (existingLanguage != null)
            {
                return Result<LanguageDto>.Failure("Language code already exists");
            }

            // If this is set as default, unset other defaults
            if (request.IsDefault)
            {
                await _context.Languages
                    .Where(l => l.IsDefault)
                    .ExecuteUpdateAsync(l => l.SetProperty(x => x.IsDefault, false));
            }

            var language = new Language
            {
                Id = Guid.NewGuid(),
                Code = request.Code,
                Name = request.Name,
                NativeName = request.NativeName,
                CultureCode = request.CultureCode,
                IsRtl = request.IsRtl,
                IsActive = true,
                IsDefault = request.IsDefault,
                SortOrder = request.SortOrder,
                FlagIcon = request.FlagIcon,
                CreatedAt = DateTime.UtcNow
            };

            _context.Languages.Add(language);
            await _context.SaveChangesAsync();

            var languageDto = new LanguageDto
            {
                Id = language.Id,
                Code = language.Code,
                Name = language.Name,
                NativeName = language.NativeName,
                CultureCode = language.CultureCode,
                IsRtl = language.IsRtl,
                IsActive = language.IsActive,
                IsDefault = language.IsDefault,
                SortOrder = language.SortOrder,
                FlagIcon = language.FlagIcon
            };

            return Result<LanguageDto>.Success(languageDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating language: {Request}", request);
            return Result<LanguageDto>.Failure("Error creating language");
        }
    }

    public async Task<Result<LanguageDto>> UpdateLanguageAsync(Guid id, UpdateLanguageRequest request)
    {
        try
        {
            var language = await _context.Languages.FindAsync(id);
            if (language == null)
            {
                return Result<LanguageDto>.Failure("Language not found");
            }

            // If this is set as default, unset other defaults
            if (request.IsDefault && !language.IsDefault)
            {
                await _context.Languages
                    .Where(l => l.IsDefault)
                    .ExecuteUpdateAsync(l => l.SetProperty(x => x.IsDefault, false));
            }

            language.Name = request.Name;
            language.NativeName = request.NativeName;
            language.CultureCode = request.CultureCode;
            language.IsRtl = request.IsRtl;
            language.IsActive = request.IsActive;
            language.IsDefault = request.IsDefault;
            language.SortOrder = request.SortOrder;
            language.FlagIcon = request.FlagIcon;
            language.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var languageDto = new LanguageDto
            {
                Id = language.Id,
                Code = language.Code,
                Name = language.Name,
                NativeName = language.NativeName,
                CultureCode = language.CultureCode,
                IsRtl = language.IsRtl,
                IsActive = language.IsActive,
                IsDefault = language.IsDefault,
                SortOrder = language.SortOrder,
                FlagIcon = language.FlagIcon
            };

            return Result<LanguageDto>.Success(languageDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating language: {Id}, {Request}", id, request);
            return Result<LanguageDto>.Failure("Error updating language");
        }
    }

    public async Task<Result<bool>> DeleteLanguageAsync(Guid id)
    {
        try
        {
            var language = await _context.Languages.FindAsync(id);
            if (language == null)
            {
                return Result<bool>.Failure("Language not found");
            }

            if (language.IsDefault)
            {
                return Result<bool>.Failure("Cannot delete default language");
            }

            // Check if language is used by users
            var userCount = await _context.UserLanguagePreferences
                .CountAsync(ulp => ulp.LanguageId == id && ulp.IsActive);

            if (userCount > 0)
            {
                return Result<bool>.Failure("Cannot delete language that is used by users");
            }

            _context.Languages.Remove(language);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting language: {Id}", id);
            return Result<bool>.Failure("Error deleting language");
        }
    }

    public async Task<Result<bool>> SetDefaultLanguageAsync(Guid id)
    {
        try
        {
            var language = await _context.Languages.FindAsync(id);
            if (language == null)
            {
                return Result<bool>.Failure("Language not found");
            }

            if (!language.IsActive)
            {
                return Result<bool>.Failure("Cannot set inactive language as default");
            }

            // Unset current default
            await _context.Languages
                .Where(l => l.IsDefault)
                .ExecuteUpdateAsync(l => l.SetProperty(x => x.IsDefault, false));

            // Set new default
            language.IsDefault = true;
            language.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default language: {Id}", id);
            return Result<bool>.Failure("Error setting default language");
        }
    }

    public async Task<Result<List<LanguageStatisticsDto>>> GetLanguageStatisticsAsync()
    {
        try
        {
            var statistics = await _context.Languages
                .Where(l => l.IsActive)
                .Select(l => new LanguageStatisticsDto
                {
                    LanguageCode = l.Code,
                    LanguageName = l.Name,
                    TotalTranslations = l.Translations.Count,
                    ActiveTranslations = l.Translations.Count(t => t.IsActive),
                    InactiveTranslations = l.Translations.Count(t => !t.IsActive),
                    UserCount = l.UserLanguagePreferences.Count(ulp => ulp.IsActive),
                    CompletionPercentage = 0 // Will be calculated based on required translations
                })
                .ToListAsync();

            // Calculate completion percentage (assuming we have a list of required translation keys)
            var requiredKeys = new[] { "welcome", "login", "register", "home", "profile", "settings", "logout" };
            
            foreach (var stat in statistics)
            {
                var languageTranslations = await _context.Translations
                    .Where(t => t.LanguageCode == stat.LanguageCode && t.IsActive)
                    .Select(t => t.Key)
                    .ToListAsync();

                var foundKeys = requiredKeys.Count(key => languageTranslations.Contains(key));
                stat.CompletionPercentage = requiredKeys.Length > 0 ? (double)foundKeys / requiredKeys.Length * 100 : 0;
            }

            return Result<List<LanguageStatisticsDto>>.Success(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting language statistics");
            return Result<List<LanguageStatisticsDto>>.Failure("Error getting language statistics");
        }
    }
}
