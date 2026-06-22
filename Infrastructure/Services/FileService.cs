using Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;


public class FileService : IFileService
{
    private readonly string _uploadPath;
    private readonly string _baseUrl;

    private readonly string[] _allowedImageTypes = { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
    private readonly string[] _allowedVideoTypes = { "video/mp4", "video/mpeg", "video/quicktime", "video/x-msvideo" };
    private readonly string[] _allowedAudioTypes = { "audio/mpeg", "audio/wav", "audio/ogg", "audio/mp3" };

    public FileService(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _uploadPath = Path.Combine(environment.WebRootPath, "uploads");
        
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }

        _baseUrl = configuration["BaseUrl"] ?? "http://localhost:5050";
    }

    
    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            var fileExtension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(_uploadPath, uniqueFileName);

            using (var fileStreamOutput = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fileStreamOutput);
            }

            return $"{_baseUrl}/uploads/{uniqueFileName}";
        }
        catch (Exception ex)
        {
            throw new Exception($"Error saving file: {ex.Message}");
        }
    }

    
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
    
    public bool IsValidFileType(string contentType)
    {
        return  _allowedImageTypes.Contains(contentType.ToLower()) || 
                _allowedVideoTypes.Contains(contentType.ToLower()) ||
                _allowedAudioTypes.Contains(contentType.ToLower());
    }
}