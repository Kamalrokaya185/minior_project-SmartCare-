using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCare.Domain.Appointments;

namespace SmartCare.Infrastructure.Presistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> b)
    {
        b.ToTable("Appointments");
        b.HasKey(a => a.Id);
        b.Property(a => a.Status).HasConversion<int>();
        b.Property(a => a.PaymentStatus).HasConversion<int>();
        b.Property(a => a.FeeAtBooking).HasColumnType("decimal(18,2)");
        b.Property(a => a.Notes).HasMaxLength(2000);
        b.Property(a => a.CancellationReason).HasMaxLength(300);
        b.Property(a => a.PaymentProofUrl).HasMaxLength(500);
        b.Property(a => a.PaymentMethod).HasMaxLength(20);

        b.HasIndex(a => a.PatientProfileId);
        b.HasIndex(a => new { a.ClinicId, a.AppointmentDate, a.Status });

        b.HasQueryFilter(a => !a.IsDeleted);
    }
}
