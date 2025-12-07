using Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

/// <summary>
/// تنفيذ خدمة رفع وحفظ الملفات
/// </summary>
public class FileService : IFileService
{
    private readonly string _uploadPath;
    private readonly string _baseUrl;

    // الأنواع المسموح بها
    private readonly string[] _allowedImageTypes = { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
    private readonly string[] _allowedVideoTypes = { "video/mp4", "video/mpeg", "video/quicktime", "video/x-msvideo" };

    public FileService(IWebHostEnvironment environment, IConfiguration configuration)
    {
        // مسار حفظ الملفات (wwwroot/uploads)
        _uploadPath = Path.Combine(environment.WebRootPath, "uploads");
        
        // إنشاء المجلد إذا لم يكن موجوداً
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }

        // Base URL للملفات
        _baseUrl = configuration["BaseUrl"] ?? "http://localhost:5050";
    }

    /// <summary>
    /// حفظ ملف على السيرفر
    /// </summary>
    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            // إنشاء اسم فريد للملف
            var fileExtension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(_uploadPath, uniqueFileName);

            // حفظ الملف
            using (var fileStreamOutput = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamOutput);
            }

            // إرجاع URL الملف
            return $"{_baseUrl}/uploads/{uniqueFileName}";
        }
        catch (Exception ex)
        {
            throw new Exception($"Error saving file: {ex.Message}");
        }
    }

    /// <summary>
    /// حذف ملف من السيرفر
    /// </summary>
    public async Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// التحقق من نوع الملف
    /// </summary>
    public bool IsValidFileType(string contentType)
    {
        return _allowedImageTypes.Contains(contentType.ToLower()) || 
               _allowedVideoTypes.Contains(contentType.ToLower());
    }
}