using TemplateBackend.API.Infrastructure.Identity;

namespace TemplateBackend.API.Common.Services.Interfaces;

/// <summary>
/// User service interface
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets a user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User or null if not found</returns>
    Task<User?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets a user by email
    /// </summary>
    /// <param name="email">Email address</param>
    /// <returns>User or null if not found</returns>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Gets a user by username
    /// </summary>
    /// <param name="username">Username</param>
    /// <returns>User or null if not found</returns>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="user">User to create</param>
    /// <param name="password">User password</param>
    /// <returns>Created user</returns>
    Task<User> CreateAsync(User user, string password);

    /// <summary>
    /// Updates a user
    /// </summary>
    /// <param name="user">User to update</param>
    /// <returns>Updated user</returns>
    Task<User> UpdateAsync(User user);

    /// <summary>
    /// Deletes a user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Gets all users with pagination
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Tuple of users and total count</returns>
    Task<(List<User> Users, int TotalCount)> GetAllAsync(int page = 1, int pageSize = 10);

    /// <summary>
    /// Checks if email exists
    /// </summary>
    /// <param name="email">Email address</param>
    /// <returns>True if email exists</returns>
    Task<bool> EmailExistsAsync(string email);

    /// <summary>
    /// Checks if username exists
    /// </summary>
    /// <param name="username">Username</param>
    /// <returns>True if username exists</returns>
    Task<bool> UsernameExistsAsync(string username);
} 