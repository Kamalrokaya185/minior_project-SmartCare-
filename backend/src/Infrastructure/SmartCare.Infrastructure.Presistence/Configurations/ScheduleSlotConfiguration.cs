using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCare.Domain.ClinicalDirectory;
using SmartCare.Domain.Tenancy;

namespace SmartCare.Infrastructure.Presistence.Configurations;

public class ScheduleSlotConfiguration : IEntityTypeConfiguration<ScheduleSlot>
{
    public void Configure(EntityTypeBuilder<ScheduleSlot> b)
    {
        b.ToTable("ScheduleSlots");
        b.HasKey(s => s.Id);
        b.Property(s => s.Status).HasConversion<int>();

        b.HasOne<DoctorSchedule>().WithMany().HasForeignKey(s => s.DoctorScheduleId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne<ClinicMembership>().WithMany().HasForeignKey(s => s.ClinicMembershipId).OnDelete(DeleteBehavior.Cascade);

        // The actual double-booking safety net: no two rows can exist for the same
        // doctor+clinic at the same date+time, no matter which code path created them.
        b.HasIndex(s => new { s.ClinicMembershipId, s.SlotDate, s.StartTime }).IsUnique();

        b.HasIndex(s => new { s.ClinicMembershipId, s.SlotDate }); // fast availability lookups
    }
}
