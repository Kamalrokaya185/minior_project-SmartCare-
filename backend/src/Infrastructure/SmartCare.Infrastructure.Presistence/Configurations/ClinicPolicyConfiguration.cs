using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCare.Domain.Tenancy;

namespace SmartCare.Infrastructure.Presistence.Configurations;

public class ClinicPolicyConfiguration : IEntityTypeConfiguration<ClinicPolicy>
{
    public void Configure(EntityTypeBuilder<ClinicPolicy> b)
    {
        b.ToTable("ClinicPolicies");
        b.HasKey(p => p.Id);
        b.Property(p => p.DepositPercentage).HasColumnType("decimal(5,2)");
        b.Property(p => p.RefundPercentage).HasColumnType("decimal(5,2)");
        b.Property(p => p.NoShowPenaltyAmount).HasColumnType("decimal(18,2)");
        b.Property(p => p.MinAttendancePercentage).HasColumnType("decimal(5,2)");

        b.HasOne<Clinic>().WithMany().HasForeignKey(p => p.ClinicId).OnDelete(DeleteBehavior.Cascade);

        // Only one row should have IsCurrent=true per clinic. SQLite has no partial unique index,
        // so this is enforced in the Application handler (close the old one before inserting new),
        // same compensating-control pattern we used for the Receptionist-single-clinic rule.
        b.HasIndex(p => new { p.ClinicId, p.IsCurrent });
    }
}