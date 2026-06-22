namespace Application.DTOs;

public class ChangePasswordDto
{
    public Guid UserId { get; set; }
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
