namespace Domain.Entities;

/// <summary>
/// كيان المستخدم - يمثل المستخدم في النظام
/// </summary>
public class User
{
    // المعرف الفريد للمستخدم
    public Guid Id { get; set; }
    
    // الاسم الكامل
    public string FullName { get; set; } = string.Empty;
    
    // البريد الإلكتروني (فريد)
    public string Email { get; set; } = string.Empty;
    
    // كلمة المرور المشفرة
    public string PasswordHash { get; set; } = string.Empty;
    
    // تاريخ إنشاء الحساب
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // تاريخ آخر تحديث
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // هل الحساب نشط؟
    public bool IsActive { get; set; } = true;
    
    // علاقة: قائمة التحليلات اللي عملها المستخدم
    public ICollection<Analysis> Analyses { get; set; } = new List<Analysis>();
}