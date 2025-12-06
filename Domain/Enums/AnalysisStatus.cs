namespace Domain.Entities;

/// <summary>
/// حالات التحليل
/// </summary>
public enum AnalysisStatus
{
    Pending = 1,    // في الانتظار
    Processing = 2, // جاري المعالجة
    Completed = 3,  // مكتمل
    Failed = 4      // فشل
}