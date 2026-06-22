using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class AIDetectionResultConfiguration : IEntityTypeConfiguration<AIDetectionResult>
{
    public void Configure(EntityTypeBuilder<AIDetectionResult> builder)
    {
        builder.HasOne(r => r.Content)
            .WithMany(c => c.DetectionResults)
            .HasForeignKey(r => r.ContentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.AIModel)
            .WithMany(m => m.DetectionResults)
            .HasForeignKey(r => r.AIModelId)
            .OnDelete(DeleteBehavior.Restrict); // لا نحذف النتيجة لو حذفنا الموديل (أو حسب الرغبة)
    }
}
