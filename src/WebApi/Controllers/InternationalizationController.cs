using Microsoft.AspNetCore.Mvc;
using Getir.Application.DTO;
using Getir.Application.Services.Internationalization;
using Getir.Domain.Enums;

namespace Getir.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
/// <summary>
/// Uluslararasılaştırma (diller, çeviriler ve kullanıcı dil tercihleri) ile ilgili işlemler
/// </summary>
public class InternationalizationController : ControllerBase
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

    #region Languages

    /// <summary>
    /// Tüm dilleri getirir
    /// </summary>
    /// <returns>Diller listesi</returns>
    [HttpGet("languages")]
    public async Task<ActionResult<List<LanguageDto>>> GetAllLanguages()
    {
        var languages = await _languageService.GetAllLanguagesAsync();
        return Ok(languages);
    }

    /// <summary>
    /// ID'ye göre dili getirir
    /// </summary>
    /// <param name="id">Dil ID</param>
    /// <returns>Dil</returns>
    [HttpGet("languages/{id:guid}")]
    public async Task<ActionResult<LanguageDto>> GetLanguageById(Guid id)
    {
        var language = await _languageService.GetLanguageByIdAsync(id);
        if (language == null)
            return NotFound(new { Error = "Language not found", ErrorCode = "LANGUAGE_NOT_FOUND" });

        return Ok(language);
    }

    /// <summary>
    /// Koda göre dili getirir
    /// </summary>
    /// <param name="code">Dil kodu</param>
    /// <returns>Dil</returns>
    [HttpGet("languages/code/{code}")]
    public async Task<ActionResult<LanguageDto>> GetLanguageByCode(LanguageCode code)
    {
        var language = await _languageService.GetLanguageByCodeAsync(code);
        if (language == null)
            return NotFound(new { Error = "Language not found", ErrorCode = "LANGUAGE_NOT_FOUND" });

        return Ok(language);
    }

    /// <summary>
    /// Varsayılan dili getirir
    /// </summary>
    /// <returns>Varsayılan dil</returns>
    [HttpGet("languages/default")]
    public async Task<ActionResult<LanguageDto>> GetDefaultLanguage()
    {
        var language = await _languageService.GetDefaultLanguageAsync();
        if (language == null)
            return NotFound(new { Error = "Default language not found", ErrorCode = "DEFAULT_LANGUAGE_NOT_FOUND" });

        return Ok(language);
    }

    /// <summary>
    /// Yeni dil oluşturur
    /// </summary>
    /// <param name="request">Dil oluşturma isteği</param>
    /// <returns>Oluşturulan dil</returns>
    [HttpPost("languages")]
    public async Task<ActionResult<LanguageDto>> CreateLanguage([FromBody] CreateLanguageRequest request)
    {
        var language = await _languageService.CreateLanguageAsync(request);
        return CreatedAtAction(nameof(GetLanguageById), new { id = language.Id }, language);
    }

    /// <summary>
    /// Dili günceller
    /// </summary>
    /// <param name="id">Dil ID</param>
    /// <param name="request">Dil güncelleme isteği</param>
    /// <returns>Güncellenen dil</returns>
    [HttpPut("languages/{id:guid}")]
    public async Task<ActionResult<LanguageDto>> UpdateLanguage(Guid id, [FromBody] UpdateLanguageRequest request)
    {
        var language = await _languageService.UpdateLanguageAsync(id, request);
        return Ok(language);
    }

    /// <summary>
    /// Dili siler
    /// </summary>
    /// <param name="id">Dil ID</param>
    /// <returns>İçerik yok</returns>
    [HttpDelete("languages/{id:guid}")]
    public async Task<ActionResult> DeleteLanguage(Guid id)
    {
        var result = await _languageService.DeleteLanguageAsync(id);
        if (!result)
            return NotFound(new { Error = "Language not found", ErrorCode = "LANGUAGE_NOT_FOUND" });

        return NoContent();
    }

    /// <summary>
    /// Varsayılan dili değiştirir
    /// </summary>
    /// <param name="id">Dil ID</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("languages/{id:guid}/set-default")]
    public async Task<ActionResult> SetDefaultLanguage(Guid id)
    {
        var result = await _languageService.SetDefaultLanguageAsync(id);
        if (!result)
            return NotFound(new { Error = "Language not found", ErrorCode = "LANGUAGE_NOT_FOUND" });

        return Ok(new { Message = "Default language updated successfully" });
    }

    /// <summary>
    /// Dil istatistiklerini getirir
    /// </summary>
    /// <returns>Dil istatistikleri</returns>
    [HttpGet("languages/statistics")]
    public async Task<ActionResult<List<LanguageStatisticsDto>>> GetLanguageStatistics()
    {
        var statistics = await _languageService.GetLanguageStatisticsAsync();
        return Ok(statistics);
    }

    #endregion

    #region Translations

    /// <summary>
    /// Anahtar ve dil koduna göre çeviri getirir
    /// </summary>
    /// <param name="key">Çeviri anahtarı</param>
    /// <param name="languageCode">Dil kodu</param>
    /// <returns>Çeviri</returns>
    [HttpGet("translations/{key}/{languageCode}")]
    public async Task<ActionResult<TranslationDto>> GetTranslation(string key, LanguageCode languageCode)
    {
        var translation = await _translationService.GetTranslationAsync(key, languageCode);
        if (translation == null)
            return NotFound(new { Error = "Translation not found", ErrorCode = "TRANSLATION_NOT_FOUND" });

        return Ok(translation);
    }

    /// <summary>
    /// Anahtarlara göre çoklu çevirileri getirir
    /// </summary>
    /// <param name="request">Çoklu çeviri istek modeli</param>
    /// <returns>Çoklu çeviri sonucu</returns>
    [HttpPost("translations/bulk")]
    public async Task<ActionResult<GetTranslationsByKeysResponse>> GetTranslationsByKeys([FromBody] GetTranslationsByKeysRequest request)
    {
        var response = await _translationService.GetTranslationsByKeysAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// Çeviri araması yapar
    /// </summary>
    /// <param name="request">Arama isteği</param>
    /// <returns>Arama sonucu</returns>
    [HttpPost("translations/search")]
    public async Task<ActionResult<TranslationSearchResponse>> SearchTranslations([FromBody] TranslationSearchRequest request)
    {
        var response = await _translationService.SearchTranslationsAsync(request);
        return Ok(response);
    }

    /// <summary>
    /// Yeni çeviri oluşturur
    /// </summary>
    /// <param name="request">Çeviri oluşturma isteği</param>
    /// <returns>Oluşturulan çeviri</returns>
    [HttpPost("translations")]
    public async Task<ActionResult<TranslationDto>> CreateTranslation([FromBody] CreateTranslationRequest request)
    {
        var translation = await _translationService.CreateTranslationAsync(request);
        return CreatedAtAction(nameof(GetTranslation), new { key = translation.Key, languageCode = translation.LanguageCode }, translation);
    }

    /// <summary>
    /// Çeviriyi günceller
    /// </summary>
    /// <param name="id">Çeviri ID</param>
    /// <param name="request">Çeviri güncelleme isteği</param>
    /// <returns>Güncellenen çeviri</returns>
    [HttpPut("translations/{id:guid}")]
    public async Task<ActionResult<TranslationDto>> UpdateTranslation(Guid id, [FromBody] UpdateTranslationRequest request)
    {
        var translation = await _translationService.UpdateTranslationAsync(id, request);
        return Ok(translation);
    }

    /// <summary>
    /// Çeviriyi siler
    /// </summary>
    /// <param name="id">Çeviri ID</param>
    /// <returns>İçerik yok</returns>
    [HttpDelete("translations/{id:guid}")]
    public async Task<ActionResult> DeleteTranslation(Guid id)
    {
        var result = await _translationService.DeleteTranslationAsync(id);
        if (!result)
            return NotFound(new { Error = "Translation not found", ErrorCode = "TRANSLATION_NOT_FOUND" });

        return NoContent();
    }

    /// <summary>
    /// Toplu çeviri oluşturur
    /// </summary>
    /// <param name="request">Toplu çeviri isteği</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("translations/bulk-create")]
    public async Task<ActionResult> BulkCreateTranslations([FromBody] BulkTranslationRequest request)
    {
        var result = await _translationService.BulkCreateTranslationsAsync(request);
        if (!result)
            return BadRequest(new { Error = "Failed to create translations", ErrorCode = "BULK_CREATE_FAILED" });

        return Ok(new { Message = "Translations created successfully" });
    }

    /// <summary>
    /// Toplu çeviri günceller
    /// </summary>
    /// <param name="request">Toplu çeviri isteği</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("translations/bulk-update")]
    public async Task<ActionResult> BulkUpdateTranslations([FromBody] BulkTranslationRequest request)
    {
        var result = await _translationService.BulkUpdateTranslationsAsync(request);
        if (!result)
            return BadRequest(new { Error = "Failed to update translations", ErrorCode = "BULK_UPDATE_FAILED" });

        return Ok(new { Message = "Translations updated successfully" });
    }

    /// <summary>
    /// Kategori ve dil koduna göre çevirileri getirir
    /// </summary>
    /// <param name="category">Kategori</param>
    /// <param name="languageCode">Dil kodu</param>
    /// <returns>Çeviri listesi</returns>
    [HttpGet("translations/category/{category}/{languageCode}")]
    public async Task<ActionResult<List<TranslationDto>>> GetTranslationsByCategory(string category, LanguageCode languageCode)
    {
        var translations = await _translationService.GetTranslationsByCategoryAsync(category, languageCode);
        return Ok(translations);
    }

    /// <summary>
    /// Verilen dil için sözlük formatında çevirileri getirir
    /// </summary>
    /// <param name="languageCode">Dil kodu</param>
    /// <param name="category">İsteğe bağlı kategori</param>
    /// <returns>Anahtar-değer sözlüğü</returns>
    [HttpGet("translations/dictionary/{languageCode}")]
    public async Task<ActionResult<Dictionary<string, string>>> GetTranslationsDictionary(LanguageCode languageCode, [FromQuery] string? category = null)
    {
        var dictionary = await _translationService.GetTranslationsDictionaryAsync(languageCode, category);
        return Ok(dictionary);
    }

    /// <summary>
    /// Belirtilen dil için eksik çeviri anahtarlarını getirir
    /// </summary>
    /// <param name="languageCode">Dil kodu</param>
    /// <param name="category">İsteğe bağlı kategori</param>
    /// <returns>Eksik anahtar listesi</returns>
    [HttpGet("translations/missing/{languageCode}")]
    public async Task<ActionResult<List<string>>> GetMissingTranslations(LanguageCode languageCode, [FromQuery] string? category = null)
    {
        var missingKeys = await _translationService.GetMissingTranslationsAsync(languageCode, category);
        return Ok(missingKeys);
    }

    /// <summary>
    /// JSON içerikten çevirileri içe aktarır
    /// </summary>
    /// <param name="languageCode">Dil kodu</param>
    /// <param name="request">İçe aktarma isteği</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("translations/import/{languageCode}")]
    public async Task<ActionResult> ImportTranslations(LanguageCode languageCode, [FromBody] ImportTranslationsRequest request)
    {
        var result = await _translationService.ImportTranslationsFromJsonAsync(request.JsonContent, languageCode, request.Category);
        if (!result)
            return BadRequest(new { Error = "Failed to import translations", ErrorCode = "IMPORT_FAILED" });

        return Ok(new { Message = "Translations imported successfully" });
    }

    /// <summary>
    /// Çevirileri JSON olarak dışa aktarır
    /// </summary>
    /// <param name="languageCode">Dil kodu</param>
    /// <param name="category">İsteğe bağlı kategori</param>
    /// <returns>JSON içerik</returns>
    [HttpGet("translations/export/{languageCode}")]
    public async Task<ActionResult> ExportTranslations(LanguageCode languageCode, [FromQuery] string? category = null)
    {
        var jsonContent = await _translationService.ExportTranslationsToJsonAsync(languageCode, category);
        return Ok(new { JsonContent = jsonContent });
    }

    #endregion

    #region User Language Preferences

    /// <summary>
    /// Kullanıcının dil tercihlerini getirir (tek tercih)
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <returns>Kullanıcı dil tercihi</returns>
    [HttpGet("users/{userId:guid}/language-preference")]
    public async Task<ActionResult<UserLanguagePreferenceDto>> GetUserLanguagePreference(Guid userId)
    {
        var preference = await _userLanguageService.GetUserLanguagePreferenceAsync(userId);
        if (preference == null)
            return NotFound(new { Error = "User language preference not found", ErrorCode = "USER_LANGUAGE_PREFERENCE_NOT_FOUND" });

        return Ok(preference);
    }

    /// <summary>
    /// Kullanıcının dil tercihlerini ayarlar (tek tercih)
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="request">Dil tercih isteği</param>
    /// <returns>Güncellenen tercih</returns>
    [HttpPost("users/{userId:guid}/language-preference")]
    public async Task<ActionResult<UserLanguagePreferenceDto>> SetUserLanguagePreference(Guid userId, [FromBody] SetUserLanguageRequest request)
    {
        var preference = await _userLanguageService.SetUserLanguagePreferenceAsync(userId, request);
        return Ok(preference);
    }

    /// <summary>
    /// Kullanıcının tüm dil tercihlerini getirir
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <returns>Dil tercihleri listesi</returns>
    [HttpGet("users/{userId:guid}/language-preferences")]
    public async Task<ActionResult<List<UserLanguagePreferenceDto>>> GetUserLanguagePreferences(Guid userId)
    {
        var preferences = await _userLanguageService.GetUserLanguagePreferencesAsync(userId);
        return Ok(preferences);
    }

    /// <summary>
    /// Kullanıcının belirli bir dil tercihlerini kaldırır
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="languageCode">Dil kodu</param>
    /// <returns>İçerik yok</returns>
    [HttpDelete("users/{userId:guid}/language-preferences/{languageCode}")]
    public async Task<ActionResult> RemoveUserLanguagePreference(Guid userId, LanguageCode languageCode)
    {
        var result = await _userLanguageService.RemoveUserLanguagePreferenceAsync(userId, languageCode);
        if (!result)
            return NotFound(new { Error = "User language preference not found", ErrorCode = "USER_LANGUAGE_PREFERENCE_NOT_FOUND" });

        return NoContent();
    }

    /// <summary>
    /// Kullanıcının tercih ettiği dili getirir
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <returns>Dil kodu</returns>
    [HttpGet("users/{userId:guid}/preferred-language")]
    public async Task<ActionResult<LanguageCode>> GetUserPreferredLanguage(Guid userId)
    {
        var languageCode = await _userLanguageService.GetUserPreferredLanguageAsync(userId);
        return Ok(languageCode);
    }

    /// <summary>
    /// Kullanıcının tercih ettiği dili ayarlar
    /// </summary>
    /// <param name="userId">Kullanıcı ID</param>
    /// <param name="request">Tercih edilen dil isteği</param>
    /// <returns>Başarı yanıtı</returns>
    [HttpPost("users/{userId:guid}/preferred-language")]
    public async Task<ActionResult> SetUserPreferredLanguage(Guid userId, [FromBody] SetPreferredLanguageRequest request)
    {
        var result = await _userLanguageService.SetUserPreferredLanguageAsync(userId, request.LanguageCode);
        if (!result)
            return BadRequest(new { Error = "Failed to set preferred language", ErrorCode = "SET_PREFERRED_LANGUAGE_FAILED" });

        return Ok(new { Message = "Preferred language updated successfully" });
    }

    #endregion
}

public class ImportTranslationsRequest
{
    public string JsonContent { get; set; } = default!;
    public string? Category { get; set; }
}

public class SetPreferredLanguageRequest
{
    public LanguageCode LanguageCode { get; set; }
}
