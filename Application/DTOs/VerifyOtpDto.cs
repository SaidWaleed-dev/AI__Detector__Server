namespace Application.DTOs;

public class VerifyOtpDto
{
    public string Email { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "registration" or "reset-password"
}
