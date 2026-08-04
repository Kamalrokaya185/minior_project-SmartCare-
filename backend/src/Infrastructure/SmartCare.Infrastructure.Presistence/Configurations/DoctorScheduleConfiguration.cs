using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCare.Domain.ClinicalDirectory;
using SmartCare.Domain.Tenancy;

namespace SmartCare.Infrastructure.Presistence.Configurations;

public class DoctorScheduleConfiguration : IEntityTypeConfiguration<DoctorSchedule>
{
    public void Configure(EntityTypeBuilder<DoctorSchedule> b)
    {
        b.ToTable("DoctorSchedules");
        b.HasKey(s => s.Id);

        b.HasOne<ClinicMembership>().WithMany().HasForeignKey(s => s.ClinicMembershipId).OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(s => new { s.ClinicMembershipId, s.IsActive });
    }
}
