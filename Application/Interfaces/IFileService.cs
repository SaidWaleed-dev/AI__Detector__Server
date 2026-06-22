namespace Application.Interfaces;


public interface IFileService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType);
    
    
    Task<bool> DeleteFileAsync(string filePath);
    
    
    bool IsValidFileType(string contentType);
}