using Domain.Entities;

namespace Application.Interfaces;

/// <summary>
/// واجهة Repository للتعامل مع بيانات التحليلات
/// </summary>
public interface IAnalysisRepository
{
    // جلب تحليل بالـ Id
    Task<Analysis?> GetByIdAsync(Guid id);
    
    // جلب كل تحليلات مستخدم معين
    Task<IEnumerable<Analysis>> GetByUserIdAsync(Guid userId);
    
    // إضافة تحليل جديد
    Task<Analysis> AddAsync(Analysis analysis);
    
    // تحديث تحليل
    Task UpdateAsync(Analysis analysis);
    
    // حذف تحليل
    Task DeleteAsync(Guid id);
    
    // جلب آخر N تحليل لمستخدم
    Task<IEnumerable<Analysis>> GetRecentByUserIdAsync(Guid userId, int count = 10);
}