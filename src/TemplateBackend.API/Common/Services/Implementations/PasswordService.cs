using BCrypt.Net;
using TemplateBackend.API.Common.Services.Interfaces;

namespace TemplateBackend.API.Common.Services.Implementations;

/// <summary>
/// Password service implementation using BCrypt
/// </summary>
public class PasswordService : IPasswordService
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
} 