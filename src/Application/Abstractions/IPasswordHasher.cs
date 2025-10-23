namespace Getir.Application.Abstractions;

/// <summary>
/// Şifre hashleme servisi interface
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Şifreyi hashle
    /// </summary>
    /// <param name="password">Şifre</param>
    /// <returns>Hashlenmiş şifre</returns>
    string HashPassword(string password);
    /// <summary>
    /// Şifreyi doğrula
    /// </summary>
    /// <param name="password">Şifre</param>
    /// <param name="hashedPassword">Hashlenmiş şifre</param>
    /// <returns>Doğrulama sonucu</returns>
    bool VerifyPassword(string password, string hashedPassword);
}
