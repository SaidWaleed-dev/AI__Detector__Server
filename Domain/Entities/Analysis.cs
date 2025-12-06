namespace Domain.Entities;

/// <summary>
/// كيان التحليل - يمثل عملية فحص محتوى معين
/// </summary>
public class Analysis
{
    // المعرف الفريد للتحليل
    public Guid Id { get; set; }
    
    // معرف المستخدم صاحب التحليل
    public Guid UserId { get; set; }
    
    // نوع المحتوى (نص، صورة، فيديو)
    public ContentType ContentType { get; set; }
    
    // المحتوى الأصلي أو رابط الملف
    public string Content { get; set; } = string.Empty;
    
    // النتيجة: نسبة احتمالية أن المحتوى AI (من 0 إلى 1)
    public double AiProbability { get; set; }
    
    // هل المحتوى AI؟ (true إذا النسبة أكبر من 0.5)
    public bool IsAiGenerated { get; set; }
    
    // تفاصيل إضافية عن التحليل (JSON)
    public string? Details { get; set; }
    
    // تاريخ إجراء التحليل
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    
    // حالة التحليل (جاري، مكتمل، فشل)
    public AnalysisStatus Status { get; set; } = AnalysisStatus.Completed;
    
    // علاقة: المستخدم صاحب التحليل
    public User User { get; set; } = null!;
}