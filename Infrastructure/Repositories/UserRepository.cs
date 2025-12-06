using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

/// <summary>
/// تنفيذ Repository للمستخدمين
/// يتعامل مع قاعدة البيانات لعمل عمليات CRUD على المستخدمين
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    // حقن DbContext عن طريق Dependency Injection
    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// جلب مستخدم بالـ Id
    /// </summary>
    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .Include(u => u.Analyses) // جلب التحليلات الخاصة بالمستخدم
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    /// <summary>
    /// جلب مستخدم بالـ Email
    /// </summary>
    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    /// <summary>
    /// إضافة مستخدم جديد
    /// </summary>
    public async Task<User> AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// تحديث بيانات مستخدم
    /// </summary>
    public async Task UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// حذف مستخدم
    /// </summary>
    public async Task DeleteAsync(Guid id)
    {
        var user = await GetByIdAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// التحقق من وجود Email في قاعدة البيانات
    /// </summary>
    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }
}