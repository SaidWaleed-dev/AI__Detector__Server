namespace Domain.Entities;

public class AIModel
{
    public Guid Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string? Version { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AIDetectionResult> DetectionResults { get; set; } = new List<AIDetectionResult>();
}
