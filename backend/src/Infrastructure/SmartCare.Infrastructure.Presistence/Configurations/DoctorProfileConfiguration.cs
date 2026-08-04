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
        b.HasIndex(d => d.LicenseNumber).IsUnique();
        b.Property(d => d.Qualification).HasMaxLength(255);
        b.Property(d => d.Specialization).HasMaxLength(150);
        b.Property(d => d.Gender).HasMaxLength(20);
        b.Property(d => d.Phone).HasMaxLength(20);
        b.Property(d => d.Email).HasMaxLength(255);
        b.Property(d => d.PhotoUrl).HasMaxLength(500);
    }
}
