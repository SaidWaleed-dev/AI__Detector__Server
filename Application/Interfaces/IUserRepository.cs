using Domain.Entities;

namespace Application.Interfaces;

/// <summary>
/// واجهة Repository للتعامل مع بيانات المستخدمين
/// </summary>
public interface IUserRepository
{
    // جلب مستخدم بالـ Id
    Task<User?> GetByIdAsync(Guid id);
    
    // جلب مستخدم بالـ Email
    Task<User?> GetByEmailAsync(string email);
    
    // إضافة مستخدم جديد
    Task<User> AddAsync(User user);
    
    // تحديث بيانات مستخدم
    Task UpdateAsync(User user);
    
    // حذف مستخدم
    Task DeleteAsync(Guid id);
    
    // التحقق من وجود Email
    Task<bool> EmailExistsAsync(string email);
}