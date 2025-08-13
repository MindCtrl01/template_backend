namespace TemplateBackend.API.Common.Services.Interfaces;

/// <summary>
/// Email service interface
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends a general email
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body</param>
    /// <returns>True if email sent successfully</returns>
    Task<bool> SendEmailAsync(string to, string subject, string body);

    /// <summary>
    /// Sends a login notification email
    /// </summary>
    /// <param name="userEmail">User email address</param>
    /// <param name="userName">User name</param>
    /// <param name="loginTime">Login timestamp</param>
    /// <param name="ipAddress">IP address</param>
    /// <returns>True if email sent successfully</returns>
    Task<bool> SendLoginNotificationAsync(string userEmail, string userName, DateTime loginTime, string ipAddress);

    /// <summary>
    /// Sends a registration confirmation email
    /// </summary>
    /// <param name="userEmail">User email address</param>
    /// <param name="userName">User name</param>
    /// <param name="confirmationToken">Confirmation token</param>
    /// <returns>True if email sent successfully</returns>
    Task<bool> SendRegistrationConfirmationAsync(string userEmail, string userName, string confirmationToken);

    /// <summary>
    /// Sends a password reset email
    /// </summary>
    /// <param name="userEmail">User email address</param>
    /// <param name="userName">User name</param>
    /// <param name="resetToken">Reset token</param>
    /// <returns>True if email sent successfully</returns>
    Task<bool> SendPasswordResetAsync(string userEmail, string userName, string resetToken);
} 