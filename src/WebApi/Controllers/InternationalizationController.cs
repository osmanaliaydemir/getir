using Getir.Application.DTO;
using Getir.Application.Services.Internationalization;
using Getir.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Getir.WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class InternationalizationController : BaseController
{
    private readonly ILanguageService _languageService;
    private readonly ITranslationService _translationService;
    private readonly IUserLanguageService _userLanguageService;

    public InternationalizationController(
        ILanguageService languageService,
        ITranslationService translationService,
        IUserLanguageService userLanguageService)
    {
        _languageService = languageService;
        _translationService = translationService;
        _userLanguageService = userLanguageService;
    }

    #region Language Management

    /// <summary>
    /// Get all available languages
    /// </summary>
    [HttpGet("languages")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetAllLanguages()
    {
        var result = await _languageService.GetAllLanguagesAsync();
        return HandleResult(result);
    }

    /// <summary>
    /// Get language by ID
    /// </summary>
    [HttpGet("languages/{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetLanguageById(Guid id)
    {
        var result = await _languageService.GetLanguageByIdAsync(id);
        return HandleResult(result);
    }

    /// <summary>
    /// Get language by code
    /// </summary>
    [HttpGet("languages/code/{code}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetLanguageByCode(LanguageCode code)
    {
        var result = await _languageService.GetLanguageByCodeAsync(code);
        return HandleResult(result);
    }

    /// <summary>
    /// Get default language
    /// </summary>
    [HttpGet("languages/default")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetDefaultLanguage()
    {
        var result = await _languageService.GetDefaultLanguageAsync();
        return HandleResult(result);
    }

    /// <summary>
    /// Create new language
    /// </summary>
    [HttpPost("languages")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateLanguage([FromBody] CreateLanguageRequest request)
    {
        var result = await _languageService.CreateLanguageAsync(request);
        return HandleResult(result);
    }

    /// <summary>
    /// Update language
    /// </summary>
    [HttpPut("languages/{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateLanguage(Guid id, [FromBody] UpdateLanguageRequest request)
    {
        var result = await _languageService.UpdateLanguageAsync(id, request);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete language
    /// </summary>
    [HttpDelete("languages/{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteLanguage(Guid id)
    {
        var result = await _languageService.DeleteLanguageAsync(id);
        return HandleResult(result);
    }

    /// <summary>
    /// Set default language
    /// </summary>
    [HttpPost("languages/{id:guid}/set-default")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SetDefaultLanguage(Guid id)
    {
        var result = await _languageService.SetDefaultLanguageAsync(id);
        return HandleResult(result);
    }

    /// <summary>
    /// Get language statistics
    /// </summary>
    [HttpGet("languages/statistics")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetLanguageStatistics()
    {
        var result = await _languageService.GetLanguageStatisticsAsync();
        return HandleResult(result);
    }

    #endregion

    #region Translation Management

    /// <summary>
    /// Get translation by key and language
    /// </summary>
    [HttpGet("translations/{key}/{languageCode}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetTranslation(string key, LanguageCode languageCode)
    {
        var result = await _translationService.GetTranslationAsync(key, languageCode);
        return HandleResult(result);
    }

    /// <summary>
    /// Get multiple translations by keys
    /// </summary>
    [HttpPost("translations/bulk")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetTranslationsByKeys([FromBody] GetTranslationsByKeysRequest request)
    {
        var result = await _translationService.GetTranslationsByKeysAsync(request);
        return HandleResult(result);
    }

    /// <summary>
    /// Search translations
    /// </summary>
    [HttpPost("translations/search")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SearchTranslations([FromBody] TranslationSearchRequest request)
    {
        var result = await _translationService.SearchTranslationsAsync(request);
        return HandleResult(result);
    }

    /// <summary>
    /// Create new translation
    /// </summary>
    [HttpPost("translations")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateTranslation([FromBody] CreateTranslationRequest request)
    {
        var result = await _translationService.CreateTranslationAsync(request);
        return HandleResult(result);
    }

    /// <summary>
    /// Update translation
    /// </summary>
    [HttpPut("translations/{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateTranslation(Guid id, [FromBody] UpdateTranslationRequest request)
    {
        var result = await _translationService.UpdateTranslationAsync(id, request);
        return HandleResult(result);
    }

    /// <summary>
    /// Delete translation
    /// </summary>
    [HttpDelete("translations/{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteTranslation(Guid id)
    {
        var result = await _translationService.DeleteTranslationAsync(id);
        return HandleResult(result);
    }

    /// <summary>
    /// Bulk create translations
    /// </summary>
    [HttpPost("translations/bulk-create")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> BulkCreateTranslations([FromBody] BulkTranslationRequest request)
    {
        var result = await _translationService.BulkCreateTranslationsAsync(request);
        return HandleResult(result);
    }

    /// <summary>
    /// Bulk update translations
    /// </summary>
    [HttpPost("translations/bulk-update")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> BulkUpdateTranslations([FromBody] BulkTranslationRequest request)
    {
        var result = await _translationService.BulkUpdateTranslationsAsync(request);
        return HandleResult(result);
    }

    /// <summary>
    /// Get translations by category
    /// </summary>
    [HttpGet("translations/category/{category}/{languageCode}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetTranslationsByCategory(string category, LanguageCode languageCode)
    {
        var result = await _translationService.GetTranslationsByCategoryAsync(category, languageCode);
        return HandleResult(result);
    }

    /// <summary>
    /// Get translations dictionary
    /// </summary>
    [HttpGet("translations/dictionary/{languageCode}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetTranslationsDictionary(LanguageCode languageCode, [FromQuery] string? category = null)
    {
        var result = await _translationService.GetTranslationsDictionaryAsync(languageCode, category);
        return HandleResult(result);
    }

    /// <summary>
    /// Get missing translations
    /// </summary>
    [HttpGet("translations/missing/{languageCode}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetMissingTranslations(LanguageCode languageCode, [FromQuery] string? category = null)
    {
        var result = await _translationService.GetMissingTranslationsAsync(languageCode, category);
        return HandleResult(result);
    }

    /// <summary>
    /// Import translations from JSON
    /// </summary>
    [HttpPost("translations/import/{languageCode}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ImportTranslations(LanguageCode languageCode, [FromBody] ImportTranslationsRequest request)
    {
        var result = await _translationService.ImportTranslationsFromJsonAsync(request.JsonContent, languageCode, request.Category);
        return HandleResult(result);
    }

    /// <summary>
    /// Export translations to JSON
    /// </summary>
    [HttpGet("translations/export/{languageCode}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> ExportTranslations(LanguageCode languageCode, [FromQuery] string? category = null)
    {
        var result = await _translationService.ExportTranslationsToJsonAsync(languageCode, category);
        return HandleResult(result);
    }

    #endregion

    #region User Language Preferences

    /// <summary>
    /// Get user language preference
    /// </summary>
    [HttpGet("users/{userId:guid}/language")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUserLanguagePreference(Guid userId)
    {
        var result = await _userLanguageService.GetUserLanguagePreferenceAsync(userId);
        return HandleResult(result);
    }

    /// <summary>
    /// Set user language preference
    /// </summary>
    [HttpPost("users/{userId:guid}/language")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SetUserLanguagePreference(Guid userId, [FromBody] SetUserLanguageRequest request)
    {
        var result = await _userLanguageService.SetUserLanguagePreferenceAsync(userId, request);
        return HandleResult(result);
    }

    /// <summary>
    /// Get user language preferences
    /// </summary>
    [HttpGet("users/{userId:guid}/languages")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetUserLanguagePreferences(Guid userId)
    {
        var result = await _userLanguageService.GetUserLanguagePreferencesAsync(userId);
        return HandleResult(result);
    }

    /// <summary>
    /// Remove user language preference
    /// </summary>
    [HttpDelete("users/{userId:guid}/languages/{languageCode}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> RemoveUserLanguagePreference(Guid userId, LanguageCode languageCode)
    {
        var result = await _userLanguageService.RemoveUserLanguagePreferenceAsync(userId, languageCode);
        return HandleResult(result);
    }

    /// <summary>
    /// Get user preferred language
    /// </summary>
    [HttpGet("users/{userId:guid}/preferred-language")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetUserPreferredLanguage(Guid userId)
    {
        var result = await _userLanguageService.GetUserPreferredLanguageAsync(userId);
        return HandleResult(result);
    }

    /// <summary>
    /// Set user preferred language
    /// </summary>
    [HttpPost("users/{userId:guid}/preferred-language/{languageCode}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> SetUserPreferredLanguage(Guid userId, LanguageCode languageCode)
    {
        var result = await _userLanguageService.SetUserPreferredLanguageAsync(userId, languageCode);
        return HandleResult(result);
    }

    #endregion
}

public class ImportTranslationsRequest
{
    public string JsonContent { get; set; } = default!;
    public string? Category { get; set; }
}
