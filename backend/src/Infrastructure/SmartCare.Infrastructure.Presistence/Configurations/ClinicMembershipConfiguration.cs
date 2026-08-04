using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCare.Domain.ClinicalDirectory;
using SmartCare.Domain.Tenancy;

namespace SmartCare.Infrastructure.Presistence.Configurations;

public class ClinicMembershipConfiguration : IEntityTypeConfiguration<ClinicMembership>
{
    public void Configure(EntityTypeBuilder<ClinicMembership> b)
    {
        b.ToTable("ClinicMembership");
        b.HasKey(m => m.Id);
        b.Property(m => m.ConsultationFee).HasColumnType("decimal(18,2)");

        b.HasOne<Clinic>().WithMany().HasForeignKey(m => m.ClinicId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<DoctorProfile>().WithMany().HasForeignKey(m => m.DoctorId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Department>().WithMany().HasForeignKey(m => m.DepartmentId).OnDelete(DeleteBehavior.SetNull);

        // A given doctor should only have one (active) membership row per clinic
        b.HasIndex(m => new { m.ClinicId, m.DoctorId }).IsUnique();
    }
}
