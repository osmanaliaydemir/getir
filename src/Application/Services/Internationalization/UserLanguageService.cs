using Getir.Application.Common;
using Getir.Application.DTO;
using Getir.Domain.Entities;
using Getir.Domain.Enums;
using Getir.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Getir.Application.Services.Internationalization;

public class UserLanguageService : IUserLanguageService
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserLanguageService> _logger;

    public UserLanguageService(AppDbContext context, ILogger<UserLanguageService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<UserLanguagePreferenceDto>> GetUserLanguagePreferenceAsync(Guid userId)
    {
        try
        {
            var preference = await _context.UserLanguagePreferences
                .Include(ulp => ulp.Language)
                .Where(ulp => ulp.UserId == userId && ulp.IsPrimary && ulp.IsActive)
                .Select(ulp => new UserLanguagePreferenceDto
                {
                    Id = ulp.Id,
                    UserId = ulp.UserId,
                    LanguageId = ulp.LanguageId,
                    LanguageCode = ulp.Language.Code,
                    LanguageName = ulp.Language.Name,
                    LanguageNativeName = ulp.Language.NativeName,
                    IsPrimary = ulp.IsPrimary,
                    IsActive = ulp.IsActive
                })
                .FirstOrDefaultAsync();

            if (preference == null)
            {
                // Return default language preference
                var defaultLanguage = await _context.Languages
                    .Where(l => l.IsDefault && l.IsActive)
                    .FirstOrDefaultAsync();

                if (defaultLanguage != null)
                {
                    preference = new UserLanguagePreferenceDto
                    {
                        Id = Guid.Empty,
                        UserId = userId,
                        LanguageId = defaultLanguage.Id,
                        LanguageCode = defaultLanguage.Code,
                        LanguageName = defaultLanguage.Name,
                        LanguageNativeName = defaultLanguage.NativeName,
                        IsPrimary = true,
                        IsActive = true
                    };
                }
            }

            return Result<UserLanguagePreferenceDto>.Success(preference);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user language preference: {UserId}", userId);
            return Result<UserLanguagePreferenceDto>.Failure("Error getting user language preference");
        }
    }

    public async Task<Result<UserLanguagePreferenceDto>> SetUserLanguagePreferenceAsync(Guid userId, SetUserLanguageRequest request)
    {
        try
        {
            // Get language by code
            var language = await _context.Languages
                .FirstOrDefaultAsync(l => l.Code == request.LanguageCode && l.IsActive);

            if (language == null)
            {
                return Result<UserLanguagePreferenceDto>.Failure("Language not found");
            }

            // If setting as primary, unset other primary preferences
            if (request.IsPrimary)
            {
                await _context.UserLanguagePreferences
                    .Where(ulp => ulp.UserId == userId && ulp.IsPrimary)
                    .ExecuteUpdateAsync(ulp => ulp.SetProperty(x => x.IsPrimary, false));
            }

            // Check if preference already exists
            var existingPreference = await _context.UserLanguagePreferences
                .FirstOrDefaultAsync(ulp => ulp.UserId == userId && ulp.LanguageId == language.Id);

            if (existingPreference != null)
            {
                existingPreference.IsPrimary = request.IsPrimary;
                existingPreference.IsActive = true;
                existingPreference.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                var preference = new UserLanguagePreference
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    LanguageId = language.Id,
                    IsPrimary = request.IsPrimary,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.UserLanguagePreferences.Add(preference);
            }

            await _context.SaveChangesAsync();

            var preferenceDto = new UserLanguagePreferenceDto
            {
                Id = existingPreference?.Id ?? Guid.NewGuid(),
                UserId = userId,
                LanguageId = language.Id,
                LanguageCode = language.Code,
                LanguageName = language.Name,
                LanguageNativeName = language.NativeName,
                IsPrimary = request.IsPrimary,
                IsActive = true
            };

            return Result<UserLanguagePreferenceDto>.Success(preferenceDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting user language preference: {UserId}, {Request}", userId, request);
            return Result<UserLanguagePreferenceDto>.Failure("Error setting user language preference");
        }
    }

    public async Task<Result<List<UserLanguagePreferenceDto>>> GetUserLanguagePreferencesAsync(Guid userId)
    {
        try
        {
            var preferences = await _context.UserLanguagePreferences
                .Include(ulp => ulp.Language)
                .Where(ulp => ulp.UserId == userId && ulp.IsActive)
                .OrderByDescending(ulp => ulp.IsPrimary)
                .ThenBy(ulp => ulp.CreatedAt)
                .Select(ulp => new UserLanguagePreferenceDto
                {
                    Id = ulp.Id,
                    UserId = ulp.UserId,
                    LanguageId = ulp.LanguageId,
                    LanguageCode = ulp.Language.Code,
                    LanguageName = ulp.Language.Name,
                    LanguageNativeName = ulp.Language.NativeName,
                    IsPrimary = ulp.IsPrimary,
                    IsActive = ulp.IsActive
                })
                .ToListAsync();

            return Result<List<UserLanguagePreferenceDto>>.Success(preferences);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user language preferences: {UserId}", userId);
            return Result<List<UserLanguagePreferenceDto>>.Failure("Error getting user language preferences");
        }
    }

    public async Task<Result<bool>> RemoveUserLanguagePreferenceAsync(Guid userId, LanguageCode languageCode)
    {
        try
        {
            var language = await _context.Languages
                .FirstOrDefaultAsync(l => l.Code == languageCode);

            if (language == null)
            {
                return Result<bool>.Failure("Language not found");
            }

            var preference = await _context.UserLanguagePreferences
                .FirstOrDefaultAsync(ulp => ulp.UserId == userId && ulp.LanguageId == language.Id);

            if (preference == null)
            {
                return Result<bool>.Failure("Language preference not found");
            }

            // Don't allow removing the last language preference
            var remainingPreferences = await _context.UserLanguagePreferences
                .CountAsync(ulp => ulp.UserId == userId && ulp.IsActive);

            if (remainingPreferences <= 1)
            {
                return Result<bool>.Failure("Cannot remove the last language preference");
            }

            preference.IsActive = false;
            preference.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing user language preference: {UserId}, {LanguageCode}", userId, languageCode);
            return Result<bool>.Failure("Error removing user language preference");
        }
    }

    public async Task<Result<LanguageCode>> GetUserPreferredLanguageAsync(Guid userId)
    {
        try
        {
            var preference = await _context.UserLanguagePreferences
                .Include(ulp => ulp.Language)
                .Where(ulp => ulp.UserId == userId && ulp.IsPrimary && ulp.IsActive)
                .Select(ulp => ulp.Language.Code)
                .FirstOrDefaultAsync();

            if (preference == default(LanguageCode))
            {
                // Return default language
                var defaultLanguage = await _context.Languages
                    .Where(l => l.IsDefault && l.IsActive)
                    .Select(l => l.Code)
                    .FirstOrDefaultAsync();

                return Result<LanguageCode>.Success(defaultLanguage);
            }

            return Result<LanguageCode>.Success(preference);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user preferred language: {UserId}", userId);
            return Result<LanguageCode>.Failure("Error getting user preferred language");
        }
    }

    public async Task<Result<bool>> SetUserPreferredLanguageAsync(Guid userId, LanguageCode languageCode)
    {
        try
        {
            var request = new SetUserLanguageRequest
            {
                LanguageCode = languageCode,
                IsPrimary = true
            };

            var result = await SetUserLanguagePreferenceAsync(userId, request);
            return result.IsSuccess ? Result<bool>.Success(true) : Result<bool>.Failure(result.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting user preferred language: {UserId}, {LanguageCode}", userId, languageCode);
            return Result<bool>.Failure("Error setting user preferred language");
        }
    }
}
