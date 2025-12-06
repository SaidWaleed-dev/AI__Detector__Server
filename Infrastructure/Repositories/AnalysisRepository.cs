using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// تنفيذ Repository للتحليلات
/// يتعامل مع قاعدة البيانات لعمل عمليات CRUD على التحليلات
/// </summary>
public class AnalysisRepository : IAnalysisRepository
{
    private readonly ApplicationDbContext _context;

    public AnalysisRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// جلب تحليل بالـ Id
    /// </summary>
    public async Task<Analysis?> GetByIdAsync(Guid id)
    {
        return await _context.Analyses
            .Include(a => a.User) // جلب بيانات المستخدم
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    /// <summary>
    /// جلب كل تحليلات مستخدم معين
    /// </summary>
    public async Task<IEnumerable<Analysis>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Analyses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.AnalyzedAt) // الأحدث أولاً
            .ToListAsync();
    }

    /// <summary>
    /// إضافة تحليل جديد
    /// </summary>
    public async Task<Analysis> AddAsync(Analysis analysis)
    {
        _context.Analyses.Add(analysis);
        await _context.SaveChangesAsync();
        return analysis;
    }

    /// <summary>
    /// تحديث تحليل
    /// </summary>
    public async Task UpdateAsync(Analysis analysis)
    {
        _context.Analyses.Update(analysis);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// حذف تحليل
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        var analysis = await GetByIdAsync(id);
        if (analysis != null)
        {
            _context.Analyses.Remove(analysis);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// جلب آخر N تحليل لمستخدم معين
    /// </summary>
    public async Task<IEnumerable<Analysis>> GetRecentByUserIdAsync(Guid userId, int count = 10)
    {
        return await _context.Analyses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.AnalyzedAt)
            .Take(count) // أخذ أول N تحليل
            .ToListAsync();
    }
}