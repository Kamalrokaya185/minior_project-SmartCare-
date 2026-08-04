using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCare.Domain.Appointments;

namespace SmartCare.Infrastructure.Presistence.Configurations;

public class AppointmentStatusHistoryConfiguration : IEntityTypeConfiguration<AppointmentStatusHistoryEntry>
{
    public void Configure(EntityTypeBuilder<AppointmentStatusHistoryEntry> b)
    {
        b.ToTable("AppointmentStatusHistory");
        b.HasKey(h => h.Id);
        b.Property(h => h.FromStatus).HasConversion<int?>();
        b.Property(h => h.ToStatus).HasConversion<int>();
        b.Property(h => h.Reason).HasMaxLength(300);

        b.HasOne<Appointment>().WithMany().HasForeignKey(h => h.AppointmentId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(h => new { h.AppointmentId, h.ChangedAtUtc });
    }
}
