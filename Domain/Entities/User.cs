namespace Domain.Entities;


public class User 
{
    public Guid Id { get; set; }
    
    public string FullName { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;
    
    public string PasswordHash { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
    
    public string? Provider { get; set; }
    public string? ProviderId { get; set; }

    public string? ResetPasswordToken { get; set; }
    public DateTime? ResetPasswordExpiry { get; set; }

    public string? VerificationToken { get; set; }
    public DateTime? VerificationTokenExpiry { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    
    public ICollection<Content> Contents { get; set; } = new List<Content>();
}