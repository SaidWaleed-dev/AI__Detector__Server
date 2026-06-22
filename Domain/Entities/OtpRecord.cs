namespace Domain.Entities;

public enum OtpType
{
    Registration,
    PasswordReset
}


public class OtpRecord
{
    public Guid Id { get; set; }
    
    public string Email { get; set; } = string.Empty;
    
    public string OtpCode { get; set; } = string.Empty;
    
    public OtpType Type { get; set; }
    
    public string? Metadata { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime ExpiresAt { get; set; }
    
    public bool IsVerified { get; set; } = false;
    
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
}
