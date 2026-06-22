namespace Domain.Entities;


public class Content
{
    public Guid Id { get; set; }
    
    public Guid UserId { get; set; }
    
    public ContentType Type { get; set; }
    
    public string Data { get; set; } = string.Empty;
    
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<AIDetectionResult> DetectionResults { get; set; } = new List<AIDetectionResult>();
}
