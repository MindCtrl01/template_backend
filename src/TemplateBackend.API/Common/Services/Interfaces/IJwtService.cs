using TemplateBackend.API.Infrastructure.Identity;

namespace TemplateBackend.API.Common.Services.Interfaces;

/// <summary>
/// JWT service interface
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Generates JWT token for user
    /// </summary>
    /// <param name="user">User entity</param>
    /// <returns>JWT token string</returns>
    string GenerateJwtToken(User user);

    /// <summary>
    /// Generates refresh token for user
    /// </summary>
    /// <param name="user">User entity</param>
    /// <returns>Refresh token string</returns>
    string GenerateRefreshToken(User user);

    /// <summary>
    /// Validates JWT token
    /// </summary>
    /// <param name="token">JWT token to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    bool ValidateJwtToken(string token);

    /// <summary>
    /// Gets user ID from JWT token
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <returns>User ID or null if invalid</returns>
    string? GetUserIdFromToken(string token);

    /// <summary>
    /// Gets user roles from JWT token
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <returns>List of user roles</returns>
    List<string> GetUserRolesFromToken(string token);
} 