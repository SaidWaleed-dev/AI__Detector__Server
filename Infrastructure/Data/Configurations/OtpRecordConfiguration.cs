using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class OtpRecordConfiguration : IEntityTypeConfiguration<OtpRecord>
{
    public void Configure(EntityTypeBuilder<OtpRecord> builder)
    {
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Email)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.OtpCode)
            .IsRequired()
            .HasMaxLength(10); // Typically 6 digits
            
        builder.Property(e => e.Metadata)
            .HasMaxLength(2000);
            
        builder.HasIndex(e => e.Email);
    }
}
