using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;
using TemplateBackend.API.Common.Models;
using TemplateBackend.API.Infrastructure.MongoDB;
using TemplateBackend.API.Common.Services.Interfaces;

namespace TemplateBackend.API.Common.Services.Implementations;

/// <summary>
/// Email service implementation using MailKit
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly IEmailMongoService _emailMongoService;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> emailSettings, 
        IEmailMongoService emailMongoService,
        ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _emailMongoService = emailMongoService;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        var emailDocument = new EmailDocument
        {
            To = to,
            Subject = subject,
            Body = body,
            EmailType = "General",
            Status = "Pending"
        };

        try
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_emailSettings.FromEmail));
            email.To.Add(MailboxAddress.Parse(to));
            email.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = body
            };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            // Update email document with success status and save to MongoDB
            emailDocument.Status = "Sent";
            await _emailMongoService.SaveEmailAsync(emailDocument);

            _logger.LogInformation("Email sent successfully to {Email}", to);
            return true;
        }
        catch (Exception ex)
        {
            // Update email document with failure status and save to MongoDB
            emailDocument.Status = "Failed";
            emailDocument.ErrorMessage = ex.Message;
            await _emailMongoService.SaveEmailAsync(emailDocument);

            _logger.LogError(ex, "Failed to send email to {Email}", to);
            return false;
        }
    }

    public async Task<bool> SendLoginNotificationAsync(string userEmail, string userName, DateTime loginTime, string ipAddress)
    {
        var subject = "New Login Detected";
        var body = $@"
            <html>
                <body>
                    <h2>New Login Notification</h2>
                    <p>Hello {userName},</p>
                    <p>A new login was detected for your account.</p>
                    <ul>
                        <li><strong>Time:</strong> {loginTime:yyyy-MM-dd HH:mm:ss UTC}</li>
                        <li><strong>IP Address:</strong> {ipAddress}</li>
                        <li><strong>Location:</strong> {await GetLocationFromIpAsync(ipAddress)}</li>
                    </ul>
                    <p>If this was not you, please contact support immediately.</p>
                    <p>Best regards,<br>Template Backend Team</p>
                </body>
            </html>";

        var emailDocument = new EmailDocument
        {
            To = userEmail,
            Subject = subject,
            Body = body,
            EmailType = "LoginNotification",
            UserEmail = userEmail,
            UserName = userName,
            Metadata = new Dictionary<string, object>
            {
                { "LoginTime", loginTime },
                { "IpAddress", ipAddress },
                { "Location", await GetLocationFromIpAsync(ipAddress) }
            },
            Status = "Pending"
        };

        try
        {
            var result = await SendEmailAsync(userEmail, subject, body);
            if (result)
            {
                emailDocument.Status = "Sent";
                await _emailMongoService.SaveEmailAsync(emailDocument);
            }
            else
            {
                emailDocument.Status = "Failed";
                await _emailMongoService.SaveEmailAsync(emailDocument);
            }
            return result;
        }
        catch (Exception ex)
        {
            emailDocument.Status = "Failed";
            emailDocument.ErrorMessage = ex.Message;
            await _emailMongoService.SaveEmailAsync(emailDocument);
            return false;
        }
    }

    public async Task<bool> SendRegistrationConfirmationAsync(string userEmail, string userName, string confirmationToken)
    {
        var subject = "Welcome to Template Backend - Confirm Your Email";
        var body = $@"
            <html>
                <body>
                    <h2>Welcome to Template Backend!</h2>
                    <p>Hello {userName},</p>
                    <p>Thank you for registering with us. Please confirm your email address by clicking the link below:</p>
                    <p><a href=""{_emailSettings.BaseUrl}/confirm-email?token={confirmationToken}"">Confirm Email Address</a></p>
                    <p>If the link doesn't work, copy and paste this URL into your browser:</p>
                    <p>{_emailSettings.BaseUrl}/confirm-email?token={confirmationToken}</p>
                    <p>This link will expire in 24 hours.</p>
                    <p>Best regards,<br>Template Backend Team</p>
                </body>
            </html>";

        var emailDocument = new EmailDocument
        {
            To = userEmail,
            Subject = subject,
            Body = body,
            EmailType = "RegistrationConfirmation",
            UserEmail = userEmail,
            UserName = userName,
            Metadata = new Dictionary<string, object>
            {
                { "ConfirmationToken", confirmationToken },
                { "ExpiresAt", DateTime.UtcNow.AddHours(24) }
            },
            Status = "Pending"
        };

        try
        {
            var result = await SendEmailAsync(userEmail, subject, body);
            if (result)
            {
                emailDocument.Status = "Sent";
                await _emailMongoService.SaveEmailAsync(emailDocument);
            }
            else
            {
                emailDocument.Status = "Failed";
                await _emailMongoService.SaveEmailAsync(emailDocument);
            }
            return result;
        }
        catch (Exception ex)
        {
            emailDocument.Status = "Failed";
            emailDocument.ErrorMessage = ex.Message;
            await _emailMongoService.SaveEmailAsync(emailDocument);
            return false;
        }
    }

    public async Task<bool> SendPasswordResetAsync(string userEmail, string userName, string resetToken)
    {
        var subject = "Password Reset Request";
        var body = $@"
            <html>
                <body>
                    <h2>Password Reset Request</h2>
                    <p>Hello {userName},</p>
                    <p>We received a request to reset your password. Click the link below to create a new password:</p>
                    <p><a href=""{_emailSettings.BaseUrl}/reset-password?token={resetToken}"">Reset Password</a></p>
                    <p>If the link doesn't work, copy and paste this URL into your browser:</p>
                    <p>{_emailSettings.BaseUrl}/reset-password?token={resetToken}</p>
                    <p>This link will expire in 1 hour.</p>
                    <p>If you didn't request this password reset, please ignore this email.</p>
                    <p>Best regards,<br>Template Backend Team</p>
                </body>
            </html>";

        var emailDocument = new EmailDocument
        {
            To = userEmail,
            Subject = subject,
            Body = body,
            EmailType = "PasswordReset",
            UserEmail = userEmail,
            UserName = userName,
            Metadata = new Dictionary<string, object>
            {
                { "ResetToken", resetToken },
                { "ExpiresAt", DateTime.UtcNow.AddHours(1) }
            },
            Status = "Pending"
        };

        try
        {
            var result = await SendEmailAsync(userEmail, subject, body);
            if (result)
            {
                emailDocument.Status = "Sent";
                await _emailMongoService.SaveEmailAsync(emailDocument);
            }
            else
            {
                emailDocument.Status = "Failed";
                await _emailMongoService.SaveEmailAsync(emailDocument);
            }
            return result;
        }
        catch (Exception ex)
        {
            emailDocument.Status = "Failed";
            emailDocument.ErrorMessage = ex.Message;
            await _emailMongoService.SaveEmailAsync(emailDocument);
            return false;
        }
    }

    private async Task<string> GetLocationFromIpAsync(string ipAddress)
    {
        try
        {
            // TODO: Implement IP geolocation service
            // For now, return a placeholder
            return "Unknown Location";
        }
        catch
        {
            return "Unknown Location";
        }
    }
}

/// <summary>
/// Email settings configuration
/// </summary>
public class EmailSettings
{
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
} 