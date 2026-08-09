using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCare.Domain.ClinicalDirectory;

namespace SmartCare.Infrastructure.Presistence.Configurations;

public class DoctorProfileConfiguration : IEntityTypeConfiguration<DoctorProfile>
{
    public void Configure(EntityTypeBuilder<DoctorProfile> b)
    {
        b.ToTable("DoctorProfile");
        b.HasKey(d => d.Id);
        b.Property(d => d.FullName).IsRequired().HasMaxLength(200);
        b.Property(d => d.LicenseNumber).IsRequired().HasMaxLength(100);
        b.Property(d => d.Specialization).IsRequired().HasMaxLength(150);
        b.Property(d => d.Gender).HasMaxLength(20);

        // The actual fix: License + Specialization together must be unique, not License alone.
        b.HasIndex(d => new { d.LicenseNumber, d.Specialization }).IsUnique();
    }
}
