namespace Domain.Entities;


public class AIDetectionResult
{
    public Guid Id { get; set; }
    
    public Guid ContentId { get; set; }
    
    public Guid AIModelId { get; set; }
    
    public double AiProbability { get; set; }
    
    public bool IsAiGenerated { get; set; }
    
    public string? Details { get; set; }
    
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

    public Content Content { get; set; } = null!;
    public AIModel AIModel { get; set; } = null!;
}
