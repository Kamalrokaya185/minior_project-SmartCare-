// DepartmentConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCare.Domain.Tenancy;

namespace SmartCare.Infrastructure.Presistence.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> b)
    {
        b.ToTable("Department");
        b.HasKey(d => d.Id);
        b.Property(d => d.Name).IsRequired().HasMaxLength(100);
        b.HasIndex(d => new { d.ClinicId, d.Name }).IsUnique();
        b.HasOne<Clinic>().WithMany().HasForeignKey(d => d.ClinicId).OnDelete(DeleteBehavior.Cascade);
    }
}