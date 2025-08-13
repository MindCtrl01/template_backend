using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TemplateBackend.API.Infrastructure.Data;
using TemplateBackend.API.Infrastructure.Identity;
using TemplateBackend.API.Common.Services.Interfaces;

namespace TemplateBackend.API.Common.Services.Implementations;

/// <summary>
/// User service implementation
/// </summary>
public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(UserManager<User> userManager, ApplicationDbContext context, ILogger<UserService> logger)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _userManager.FindByIdAsync(id.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user by ID: {UserId}", id);
            return null;
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        try
        {
            return await _userManager.FindByEmailAsync(email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user by email: {Email}", email);
            return null;
        }
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        try
        {
            return await _userManager.FindByNameAsync(username);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user by username: {Username}", username);
            return null;
        }
    }

    public async Task<User> CreateAsync(User user, string password)
    {
        try
        {
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create user: {errors}");
            }

            _logger.LogInformation("User created successfully: {UserId}", user.Id);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user");
            throw;
        }
    }

    public async Task<User> UpdateAsync(User user)
    {
        try
        {
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to update user: {errors}");
            }

            _logger.LogInformation("User updated successfully: {UserId}", user.Id);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user: {UserId}", user.Id);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                _logger.LogWarning("User not found for deletion: {UserId}", id);
                return false;
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Failed to delete user: {Errors}", errors);
                return false;
            }

            _logger.LogInformation("User deleted successfully: {UserId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete user: {UserId}", id);
            return false;
        }
    }

    public async Task<(List<User> Users, int TotalCount)> GetAllAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var skip = (page - 1) * pageSize;
            var users = await _userManager.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await _userManager.Users.CountAsync();

            return (users, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all users");
            return (new List<User>(), 0);
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if email exists: {Email}", email);
            return false;
        }
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        try
        {
            var user = await _userManager.FindByNameAsync(username);
            return user != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if username exists: {Username}", username);
            return false;
        }
    }
} 