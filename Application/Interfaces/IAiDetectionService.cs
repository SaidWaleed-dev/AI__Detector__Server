using Application.Models;

namespace Application.Interfaces;

/// <summary>
/// واجهة لخدمة كشف المحتوى المُنشأ بالذكاء الاصطناعي
/// </summary>
public interface IAiDetectionService
{
    /// <summary>
    /// تحليل نص للكشف عن المحتوى المُنشأ بالذكاء الاصطناعي
    /// </summary>
    /// <param name="text">النص المراد تحليله</param>
    /// <returns>نتيجة التحليل</returns>
    Task<AiDetectionResult> DetectTextAsync(string text);
    
    /// <summary>
    /// تحليل صورة للكشف عن المحتوى المُنشأ بالذكاء الاصطناعي
    /// </summary>
    /// <param name="imagePath">مسار الصورة</param>
    /// <returns>نتيجة التحليل</returns>
    Task<AiDetectionResult> DetectImageAsync(string imagePath);
    
    /// <summary>
    /// تحليل فيديو للكشف عن المحتوى المُنشأ بالذكاء الاصطناعي
    /// </summary>
    /// <param name="videoPath">مسار الفيديو</param>
    /// <returns>نتيجة التحليل</returns>
    Task<AiDetectionResult> DetectVideoAsync(string videoPath);
}
