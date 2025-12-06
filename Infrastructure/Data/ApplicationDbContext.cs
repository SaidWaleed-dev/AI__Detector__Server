using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

/// <summary>
/// DbContext الرئيسي للتطبيق
/// يمثل الاتصال بقاعدة البيانات ويحتوي على الجداول (DbSets)
/// </summary>
public class ApplicationDbContext : DbContext
{
    // Constructor يستقبل الإعدادات (Connection String)
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    // جدول المستخدمين
    public DbSet<User> Users { get; set; }
    
    // جدول التحليلات
    public DbSet<Analysis> Analyses { get; set; }

    /// <summary>
    /// إعداد العلاقات والقواعد في قاعدة البيانات
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // إعدادات جدول User
        modelBuilder.Entity<User>(entity =>
        {
            // Email يجب أن يكون فريد
            entity.HasIndex(e => e.Email).IsUnique();
            
            // Email مطلوب وطوله الأقصى 100 حرف
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(100);
            
            // FullName مطلوب وطوله الأقصى 150 حرف
            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(150);
        });

        // إعدادات جدول Analysis
        modelBuilder.Entity<Analysis>(entity =>
        {
            // العلاقة: Analysis تنتمي لـ User واحد
            entity.HasOne(a => a.User)
                .WithMany(u => u.Analyses)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade); // لو حذفنا User، تحذف كل تحليلاته
            
            // Content مطلوب
            entity.Property(e => e.Content)
                .IsRequired();
        });
    }
}