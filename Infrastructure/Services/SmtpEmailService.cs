using Application.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendResetPasswordEmailAsync(string email, string fullName, string resetLink)
    {
        // For older link flow
        await Task.CompletedTask; 
    }

    public async Task SendPasswordResetCodeAsync(string email, string fullName, string code)
    {
        var senderEmail = _configuration["EmailSettings:SenderEmail"];
        var appPassword = _configuration["EmailSettings:AppPassword"]?.Replace(" ", "");
        var senderName = _configuration["EmailSettings:SenderName"] ?? "TrueDetect AI";
        var smtpServer = _configuration["EmailSettings:SmtpServer"];
        var port = int.Parse(_configuration["EmailSettings:Port"] ?? "587");

        var username = _configuration["EmailSettings:Username"] ?? senderEmail;

        if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(appPassword)) 
        {
            throw new Exception("Email configuration is missing. Please set SenderEmail and AppPassword in appsettings.json.");
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(senderName, senderEmail));
        message.To.Add(new MailboxAddress(fullName, email));
        message.Subject = "Security Code - TrueDetect AI";

        var bodyBuilder = new BodyBuilder();
        bodyBuilder.HtmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;'>
                <h2 style='color: #2c3e50; text-align: center;'>TrueDetect AI</h2>
                <hr style='border: 0; border-top: 1px solid #eee;' />
                <p>Hi <strong>{fullName}</strong>,</p>
                <p>We received a request to reset your password. Please use the following security code to proceed:</p>
                <div style='background-color: #f8f9fa; padding: 15px; text-align: center; font-size: 24px; font-weight: bold; letter-spacing: 5px; color: #3498db; border-radius: 5px; margin: 20px 0;'>
                    {code}
                </div>
                <p style='color: #7f8c8d; font-size: 14px;'>This code is valid for <strong>15 minutes</strong>. If you didn't request this, please ignore this email.</p>
                <hr style='border: 0; border-top: 1px solid #eee;' />
                <p style='font-size: 12px; color: #bdc3c7; text-align: center;'>&copy; {DateTime.Now.Year} TrueDetect AI Team. All rights reserved.</p>
            </div>";
        
        bodyBuilder.TextBody = $"Hi {fullName},\n\nYour security code is: {code}\n\nPlease enter this code to reset your password. It expires in 15 minutes.\n\nThanks,\nTrueDetect AI Team";

        message.Body = bodyBuilder.ToMessageBody();

        using (var client = new SmtpClient())
        {
            try {
                _logger.LogInformation("Attempting to send password reset email to {Email}.", email);
                await client.ConnectAsync(smtpServer, port, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(username, appPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                _logger.LogInformation(" Password reset email successfully sent to {Email}", email);
            } catch (Exception ex) {
                _logger.LogError(ex, "Failed to send password reset email to {Email}. Verify SMTP settings and credentials.", email);
                throw;
            }
        }
    }
    public async Task SendVerificationCodeAsync(string email, string fullName, string code)
    {
        var senderEmail = _configuration["EmailSettings:SenderEmail"];
        var appPassword = _configuration["EmailSettings:AppPassword"]?.Replace(" ", "");
        var senderName = _configuration["EmailSettings:SenderName"] ?? "TrueDetect AI";
        var smtpServer = _configuration["EmailSettings:SmtpServer"];
        var port = int.Parse(_configuration["EmailSettings:Port"] ?? "587");

        var username = _configuration["EmailSettings:Username"] ?? senderEmail;

        // Ensure SMTP settings are configured in appsettings.json
        if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(appPassword)) 
        {
            throw new Exception("Email configuration is missing.");
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(senderName, senderEmail));
        message.To.Add(new MailboxAddress(fullName, email));
        message.Subject = "Verify Your Account - TrueDetect AI";

        var bodyBuilder = new BodyBuilder();
        bodyBuilder.HtmlBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 10px;'>
                <h2 style='color: #2c3e50; text-align: center;'>TrueDetect AI</h2>
                <hr style='border: 0; border-top: 1px solid #eee;' />
                <p>Hi <strong>{fullName}</strong>,</p>
                <p>Welcome to TrueDetect AI! Please use the following code to verify your email address and activate your account:</p>
                <div style='background-color: #f8f9fa; padding: 15px; text-align: center; font-size: 24px; font-weight: bold; letter-spacing: 5px; color: #27ae60; border-radius: 5px; margin: 20px 0;'>
                    {code}
                </div>
                <p style='color: #7f8c8d; font-size: 14px;'>If you didn't create an account, please ignore this email.</p>
                <hr style='border: 0; border-top: 1px solid #eee;' />
                <p style='font-size: 12px; color: #bdc3c7; text-align: center;'>&copy; {DateTime.Now.Year} TrueDetect AI Team. All rights reserved.</p>
            </div>";
        
        bodyBuilder.TextBody = $"Hi {fullName},\n\nYour verification code is: {code}\n\nPlease enter this code to activate your account.\n\nThanks,\nTrueDetect AI Team";

        message.Body = bodyBuilder.ToMessageBody();

        using (var client = new SmtpClient())
        {
            try {
                _logger.LogInformation("Attempting to send verification email to {Email}.", email);
                await client.ConnectAsync(smtpServer, port, MailKit.Security.SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(username, appPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                _logger.LogInformation(" Verification email successfully sent to {Email}", email);
            } catch (Exception ex) {
                _logger.LogError(ex, "Failed to send verification email to {Email}. Check SMTP credentials and configuration.", email);
                throw;
            }
        }
    }
}
