namespace Application.Interfaces;

public interface IEmailService
{
    Task SendResetPasswordEmailAsync(string email, string fullName, string resetLink);
    Task SendPasswordResetCodeAsync(string email, string fullName, string code);
    Task SendVerificationCodeAsync(string email, string fullName, string code);
}
