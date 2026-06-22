using Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class MockEmailService : IEmailService
{
    private readonly ILogger<MockEmailService> _logger;

    public MockEmailService(ILogger<MockEmailService> logger)
    {
        _logger = logger;
    }

    public async Task SendResetPasswordEmailAsync(string email, string fullName, string resetLink)
    {
        _logger.LogInformation($"[MOCK EMAIL SERVICE] To: {email} ({fullName})");
        _logger.LogInformation($"[MOCK EMAIL SERVICE] Body: Hello {fullName}. You requested to reset your password. Use this link: {resetLink}");
        await Task.Delay(500);
    }

    public async Task SendPasswordResetCodeAsync(string email, string fullName, string code)
    {
        Console.WriteLine("\n**********************************************************");
        Console.WriteLine($" [NEW PASSWORD RESET EMAIL SENT TO: {email}]");
        Console.WriteLine($" SECURITY CODE: {code}");
        Console.WriteLine("**********************************************************\n");

        _logger.LogInformation($"[MOCK EMAIL SERVICE] Reset Code {code} sent to {email}");
        await Task.Delay(500);
    }

    public async Task SendVerificationCodeAsync(string email, string fullName, string code)
    {
        Console.WriteLine("\n**********************************************************");
        Console.WriteLine($" [NEW VERIFICATION EMAIL SENT TO: {email}]");
        Console.WriteLine($" VERIFICATION CODE: {code}");
        Console.WriteLine("**********************************************************\n");

        _logger.LogInformation($"[MOCK EMAIL SERVICE] Verification Code {code} sent to {email}");
        await Task.Delay(500);
    }
}
