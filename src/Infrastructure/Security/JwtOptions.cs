namespace Getir.Infrastructure.Security;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public string Secret { get; set; } = default!;
    public int AccessTokenMinutes { get; set; } = 60;
    public int RefreshTokenMinutes { get; set; } = 10080; // 7 days
}
