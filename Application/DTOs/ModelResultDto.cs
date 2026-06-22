namespace Application.DTOs;

public class ModelResultDto
{
    public Guid ResultId { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public double AiProbability { get; set; }
    public bool IsAiGenerated { get; set; }
    public string? Details { get; set; }
    public DateTime AnalyzedAt { get; set; }
}
