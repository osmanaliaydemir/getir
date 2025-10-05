namespace Getir.Domain.Enums;

public enum LanguageCode
{
    Turkish = 1,
    English = 2,
    Arabic = 3
}

public static class LanguageCodeExtensions
{
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

    public static bool IsRtl(this LanguageCode languageCode)
    {
        return languageCode == LanguageCode.Arabic;
    }
}