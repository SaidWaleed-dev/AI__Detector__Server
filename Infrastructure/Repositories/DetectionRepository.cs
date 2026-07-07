using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Specifications;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;


public class DetectionRepository : GenericRepository<Content>, IDetectionRepository
{
    public DetectionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Content?> GetContentByIdAsync(Guid id)
    {
        // ================= without spec =================
        // return await _context.Contents
        //     .Include(c => c.User)
        //     .Include(c => c.DetectionResults)
        //         .ThenInclude(r => r.AIModel)
        //     .FirstOrDefaultAsync(c => c.Id == id);
        // ================================================

        // ================= with spec ====================
        var spec = new ContentWithDetailsSpecification(id);
        return await GetEntityWithSpecAsync(spec);
        // ================================================
    }

    public async Task<IEnumerable<Content>> GetContentsByUserIdAsync(Guid userId)
    {
        // ================= without spec =================
        // return await _context.Contents
        //     .Where(c => c.UserId == userId)
        //     .OrderByDescending(c => c.UploadedAt)
        //     .ToListAsync();
        // ================================================

        // ================= with spec ====================
        var spec = new ContentWithDetailsSpecification(userId, includeDetails: false);
        return await ListAsync(spec);
        // ================================================
    }

    public async Task<Content> AddContentAsync(Content content)
    {
        return await AddAsync(content);
    }

    public async Task<AIDetectionResult?> GetResultByContentIdAsync(Guid contentId)
    {
        return await _context.DetectionResults
            .Include(r => r.AIModel)
            .OrderByDescending(r => r.AnalyzedAt)
            .FirstOrDefaultAsync(r => r.ContentId == contentId);
    }

    public async Task<AIDetectionResult> AddResultAsync(AIDetectionResult result)
    {
        _context.DetectionResults.Add(result);
        await _context.SaveChangesAsync();
        return result;
    }

    public async Task<IEnumerable<Content>> GetRecentContentsByUserIdAsync(Guid userId, int count = 10)
    {
        // ================= without spec =================
        // return await _context.Contents
        //     .Include(c => c.DetectionResults)
        //         .ThenInclude(r => r.AIModel)
        //     .Where(c => c.UserId == userId)
        //     .OrderByDescending(c => c.UploadedAt)
        //     .Take(count)
        //     .ToListAsync();
        // ================================================

        // ================= with spec ====================
        var spec = new ContentWithDetailsSpecification(userId, count);
        return await ListAsync(spec);
        // ================================================
    }

    public async Task<AIModel?> GetModelByNameAsync(string name)
    {
        return await _context.AIModels.FirstOrDefaultAsync(m => m.Name == name);
    }

    public async Task<AIModel> AddModelAsync(AIModel model)
    {
        _context.AIModels.Add(model);
        await _context.SaveChangesAsync();
        return model;
    }

    public async Task<Content?> GetContentByDataAsync(Guid userId, string data, ContentType type)
    {
        // ================= without spec =================
        // return await _context.Contents
        //     .Include(c => c.DetectionResults)
        //         .ThenInclude(r => r.AIModel)
        //     .FirstOrDefaultAsync(c => c.UserId == userId && 
        //                              c.Type == type && 
        //                              (c.Data.Trim() == data || c.Data.EndsWith("|" + data)));
        // ================================================
        
        // ================= with spec ====================
        var spec = new ContentWithDetailsSpecification(userId, data, type);
        return await GetEntityWithSpecAsync(spec);
        // ================================================
    }

    public async Task<int> GetUsageCountAsync(Guid userId, ContentType type)
    {
        return await _context.Contents.CountAsync(c => c.UserId == userId && c.Type == type);
    }

    public async Task<bool> DeleteContentAsync(Guid id)
    {
        var content = await _context.Contents.FindAsync(id);
        if (content == null) return false;

        _context.Contents.Remove(content);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAllUserContentAsync(Guid userId)
    {
        var userContents = await _context.Contents
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (!userContents.Any()) return false;

        _context.Contents.RemoveRange(userContents);
        await _context.SaveChangesAsync();
        return true;
    }
}
