using Domain.Entities;

namespace Application.DTOs;

/// <summary>
/// DTO لإرجاع نتيجة التحليل
/// </summary>
public class AnalysisResponseDto
{
    public Guid Id { get; set; }
    public ContentType ContentType { get; set; }
    public double AiProbability { get; set; }
    public bool IsAiGenerated { get; set; }
    public string? Details { get; set; }
    public DateTime AnalyzedAt { get; set; }
    public AnalysisStatus Status { get; set; }
}