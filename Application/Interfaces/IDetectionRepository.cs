using Domain.Entities;

namespace Application.Interfaces;


public interface IDetectionRepository
{
    Task<Content?> GetContentByIdAsync(Guid id);
    Task<IEnumerable<Content>> GetContentsByUserIdAsync(Guid userId);
    Task<Content> AddContentAsync(Content content);
    
    Task<AIDetectionResult?> GetResultByContentIdAsync(Guid contentId);
    Task<AIDetectionResult> AddResultAsync(AIDetectionResult result);
    
    Task<IEnumerable<Content>> GetRecentContentsByUserIdAsync(Guid userId, int count = 10);
    Task<Content?> GetContentByDataAsync(Guid userId, string data, ContentType type);

    Task<AIModel?> GetModelByNameAsync(string name);
    Task<AIModel> AddModelAsync(AIModel model);
    Task<int> GetUsageCountAsync(Guid userId, ContentType type);
    Task<bool> DeleteContentAsync(Guid id);
    Task<bool> DeleteAllUserContentAsync(Guid userId);
}
