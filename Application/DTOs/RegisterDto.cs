namespace Application.DTOs;

/// <summary>
/// DTO لبيانات التسجيل (Register)
/// يستخدم لاستقبال بيانات المستخدم الجديد
/// </summary>
public class RegisterDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}