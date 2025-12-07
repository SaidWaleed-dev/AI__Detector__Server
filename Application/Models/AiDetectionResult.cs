namespace Application.Models;

/// <summary>
/// نتيجة تحليل الذكاء الاصطناعي
/// </summary>
public class AiDetectionResult
{
    /// <summary>
    /// نسبة احتمالية أن المحتوى مُنشأ بالذكاء الاصطناعي (0 إلى 1)
    /// </summary>
    public double AiProbability { get; set; }
    
    /// <summary>
    /// هل المحتوى مُنشأ بالذكاء الاصطناعي؟
    /// </summary>
    public bool IsAiGenerated { get; set; }
    
    /// <summary>
    /// مستوى الثقة في النتيجة (0 إلى 1)
    /// </summary>
    public double Confidence { get; set; }
    
    /// <summary>
    /// تفاصيل إضافية عن التحليل
    /// </summary>
    public string Details { get; set; } = string.Empty;
    
    /// <summary>
    /// مؤشرات كُشفت في المحتوى
    /// </summary>
    public List<string> Indicators { get; set; } = new();
    
    /// <summary>
    /// وقت التحليل بالميلي ثانية
    /// </summary>
    public long ProcessingTimeMs { get; set; }
}
