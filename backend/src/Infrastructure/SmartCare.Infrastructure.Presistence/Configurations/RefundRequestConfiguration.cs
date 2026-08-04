using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCare.Domain.Appointments;

namespace SmartCare.Infrastructure.Presistence.Configurations;

public class RefundRequestConfiguration : IEntityTypeConfiguration<RefundRequest>
{
    public void Configure(EntityTypeBuilder<RefundRequest> b)
    {
        b.ToTable("RefundRequests");
        b.HasKey(r => r.Id);
        b.Property(r => r.Status).HasConversion<int>();
        b.Property(r => r.RequestedAmount).HasColumnType("decimal(18,2)");
        b.Property(r => r.ApprovedAmount).HasColumnType("decimal(18,2)");
        b.Property(r => r.Reason).HasMaxLength(300);

        b.HasOne<Appointment>().WithMany().HasForeignKey(r => r.AppointmentId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(r => new { r.AppointmentId, r.Status });
    }
}