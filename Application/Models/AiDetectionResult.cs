namespace Application.Models;


public class AiDetectionResult
{
    
    public double AiProbability { get; set; }
    
    
    public bool IsAiGenerated { get; set; }
    
    
    public double Confidence { get; set; }
    
    
    public string Details { get; set; } = string.Empty;
    
        
    public List<string> Indicators { get; set; } = new();
    
    
    public long ProcessingTimeMs { get; set; }
}
