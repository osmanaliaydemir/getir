using Getir.Domain.Enums;

namespace Getir.Application.DTO;

public class LanguageDto
{
    public Guid Id { get; set; }
    public LanguageCode Code { get; set; }
    public string Name { get; set; } = default!;
    public string NativeName { get; set; } = default!;
    public string CultureCode { get; set; } = default!;
    public bool IsRtl { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
    public string? FlagIcon { get; set; }
}

public class CreateLanguageRequest
{
    public LanguageCode Code { get; set; }
    public string Name { get; set; } = default!;
    public string NativeName { get; set; } = default!;
    public string CultureCode { get; set; } = default!;
    public bool IsRtl { get; set; } = false;
    public bool IsDefault { get; set; } = false;
    public int SortOrder { get; set; } = 0;
    public string? FlagIcon { get; set; }
}

public class UpdateLanguageRequest
{
    public string Name { get; set; } = default!;
    public string NativeName { get; set; } = default!;
    public string CultureCode { get; set; } = default!;
    public bool IsRtl { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
    public string? FlagIcon { get; set; }
}

public class TranslationDto
{
    public Guid Id { get; set; }
    public string Key { get; set; } = default!;
    public string Value { get; set; } = default!;
    public LanguageCode LanguageCode { get; set; }
    public string? Category { get; set; }
    public string? Context { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class CreateTranslationRequest
{
    public string Key { get; set; } = default!;
    public string Value { get; set; } = default!;
    public LanguageCode LanguageCode { get; set; }
    public string? Category { get; set; }
    public string? Context { get; set; }
    public string? Description { get; set; }
}

public class UpdateTranslationRequest
{
    public string Value { get; set; } = default!;
    public string? Category { get; set; }
    public string? Context { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class BulkTranslationRequest
{
    public LanguageCode LanguageCode { get; set; }
    public string? Category { get; set; }
    public List<TranslationItem> Translations { get; set; } = new();
}

public class TranslationItem
{
    public string Key { get; set; } = default!;
    public string Value { get; set; } = default!;
    public string? Context { get; set; }
    public string? Description { get; set; }
}

public class UserLanguagePreferenceDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid LanguageId { get; set; }
    public LanguageCode LanguageCode { get; set; }
    public string LanguageName { get; set; } = default!;
    public string LanguageNativeName { get; set; } = default!;
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; }
}

public class SetUserLanguageRequest
{
    public LanguageCode LanguageCode { get; set; }
    public bool IsPrimary { get; set; } = true;
}

public class TranslationSearchRequest
{
    public string? Key { get; set; }
    public LanguageCode? LanguageCode { get; set; }
    public string? Category { get; set; }
    public string? Context { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class TranslationSearchResponse
{
    public List<TranslationDto> Translations { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class LanguageStatisticsDto
{
    public LanguageCode LanguageCode { get; set; }
    public string LanguageName { get; set; } = default!;
    public int TotalTranslations { get; set; }
    public int ActiveTranslations { get; set; }
    public int InactiveTranslations { get; set; }
    public int UserCount { get; set; }
    public double CompletionPercentage { get; set; }
}

public class GetTranslationsByKeysRequest
{
    public List<string> Keys { get; set; } = new();
    public LanguageCode LanguageCode { get; set; }
    public string? Category { get; set; }
}

public class GetTranslationsByKeysResponse
{
    public Dictionary<string, string> Translations { get; set; } = new();
    public LanguageCode LanguageCode { get; set; }
    public int FoundCount { get; set; }
    public int MissingCount { get; set; }
    public List<string> MissingKeys { get; set; } = new();
}
