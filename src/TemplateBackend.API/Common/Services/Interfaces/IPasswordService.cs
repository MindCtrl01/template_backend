namespace TemplateBackend.API.Common.Services.Interfaces;

/// <summary>
/// Password service interface
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Hashes a password
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <returns>Hashed password</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies a password against a hash
    /// </summary>
    /// <param name="password">Plain text password</param>
    /// <param name="hash">Hashed password</param>
    /// <returns>True if password matches hash</returns>
    bool VerifyPassword(string password, string hash);
} 