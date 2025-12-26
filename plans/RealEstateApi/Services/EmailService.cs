using System.Net;
using System.Net.Mail;

namespace RealEstateApi.Services;

public interface IEmailService
{
    Task SendWelcomeEmailAsync(string email, string firstName);
    Task SendPasswordResetEmailAsync(string email, string resetToken);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendWelcomeEmailAsync(string email, string firstName)
    {
        var subject = "Welcome to Real Estate Marketplace!";
        var body = $@"
        <h1>Welcome to Real Estate Marketplace, {firstName}!</h1>
        <p>Thank you for registering with us. Your account has been successfully created.</p>
        <p>You can now:</p>
        <ul>
            <li>Browse properties for sale and rent</li>
            <li>Save your favorite listings</li>
            <li>Contact real estate agents</li>
            <li>List your own properties (if you're a seller)</li>
        </ul>
        <p>Start exploring: <a href=""{_configuration["App:FrontendUrl"]}"">Visit our website</a></p>
        <br>
        <p>Best regards,<br>The Real Estate Marketplace Team</p>
        ";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendPasswordResetEmailAsync(string email, string resetToken)
    {
        var resetUrl = $"{_configuration["App:FrontendUrl"]}/reset-password?token={resetToken}";
        var subject = "Password Reset Request";
        var body = $@"
        <h1>Password Reset Request</h1>
        <p>You have requested to reset your password for your Real Estate Marketplace account.</p>
        <p>Please click the link below to reset your password:</p>
        <p><a href=""{resetUrl}"">Reset Password</a></p>
        <p>This link will expire in 24 hours.</p>
        <p>If you did not request this password reset, please ignore this email.</p>
        <br>
        <p>Best regards,<br>The Real Estate Marketplace Team</p>
        ";

        await SendEmailAsync(email, subject, body);
    }

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        try
        {
            // For development/demo purposes, we'll log the email instead of sending
            // In production, replace this with actual email sending logic

            var emailMode = _configuration["Email:Mode"] ?? "Development";

            if (emailMode == "Development")
            {
                _logger.LogInformation("EMAIL WOULD BE SENT (Development Mode)");
                _logger.LogInformation("To: {Email}", toEmail);
                _logger.LogInformation("Subject: {Subject}", subject);
                _logger.LogInformation("Body: {Body}", htmlBody.Replace("<", "<").Replace(">", ">"));
                return;
            }

            // Production email sending logic would go here
            // Example using SMTP:
            /*
            using var smtpClient = new SmtpClient(_configuration["Email:SmtpHost"])
            {
                Port = int.Parse(_configuration["Email:SmtpPort"] ?? "587"),
                Credentials = new NetworkCredential(
                    _configuration["Email:Username"],
                    _configuration["Email:Password"]
                ),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["Email:FromAddress"]!),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
            */
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            throw;
        }
    }
}