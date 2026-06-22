using Application.Models;

namespace Application.Interfaces;


public interface IAiDetectionService
{
    
    Task<AiDetectionResult> DetectTextAsync(string text);
    
    
    Task<AiDetectionResult> DetectImageAsync(string imagePath);
    
    
    Task<AiDetectionResult> DetectVideoAsync(string videoPath);
    
    Task<AiDetectionResult> DetectAudioAsync(string audioPath);
}
