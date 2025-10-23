namespace Getir.Domain.Enums;

/// <summary>
/// Dillerin kodları
/// </summary>
public enum LanguageCode
{
    /// <summary>
    /// Türkçe
    /// </summary>
    Turkish = 1,
    /// <summary>
    /// İngilizce
    /// </summary>
    English = 2,
    /// <summary>
    /// Arapça
    /// </summary>
    Arabic = 3
}

/// <summary>
/// Dillerin kodları uzantıları
/// </summary>
public static class LanguageCodeExtensions
{
    /// <summary>
    /// Dilin kültür kodunu döndürür
    /// </summary>
    /// <param name="languageCode">Dil kodu</param>
    /// <returns>Dilin kültür kodu</returns>
    public static string GetCultureCode(this LanguageCode languageCode)
    {
        return languageCode switch
        {
            LanguageCode.Turkish => "tr-TR",
            LanguageCode.English => "en-US",
            LanguageCode.Arabic => "ar-SA",
            _ => "tr-TR"
        };
    }

    /// <summary>
    /// Dilin görünen adını döndürür
    /// </summary>
    /// <param name="languageCode">Dil kodu</param>
    /// <returns>Dilin görünen adı</returns>
    public static string GetDisplayName(this LanguageCode languageCode)
    {
        return languageCode switch
        {
            LanguageCode.Turkish => "Türkçe",
            LanguageCode.English => "English",
            LanguageCode.Arabic => "العربية",
            _ => "Türkçe"
        };
    }

    /// <summary>
    /// Dilin yerel adını döndürür
    /// </summary>
    /// <param name="languageCode">Dil kodu</param>
    /// <returns>Dilin yerel adı</returns>
    public static string GetNativeName(this LanguageCode languageCode)
    {
        return languageCode switch
        {
            LanguageCode.Turkish => "Türkçe",
            LanguageCode.English => "English",
            LanguageCode.Arabic => "العربية",
            _ => "Türkçe"
        };
    }

    /// <summary>
    /// Dilin RTL olup olmadığını döndürür
    /// </summary>
    /// <param name="languageCode">Dil kodu</param>
    /// <returns>Dilin RTL olup olmadığı</returns>
    public static bool IsRtl(this LanguageCode languageCode)
    {
        return languageCode == LanguageCode.Arabic;
    }
}