namespace Application.Interfaces;

/// <summary>
/// واجهة للتعامل مع رفع وحفظ الملفات
/// </summary>
public interface IFileService
{
    /// <summary>
    /// حفظ ملف على السيرفر وإرجاع المسار
    /// </summary>
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType);
    
    /// <summary>
    /// حذف ملف من السيرفر
    /// </summary>
    Task<bool> DeleteFileAsync(string filePath);
    
    /// <summary>
    /// التحقق من نوع الملف (صورة أو فيديو)
    /// </summary>
    bool IsValidFileType(string contentType);
}