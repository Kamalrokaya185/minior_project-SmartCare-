using System;
using System.Collections.Generic;
using System.Text;

// ClinicConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCare.Domain.Tenancy;

namespace SmartCare.Infrastructure.Presistence.Configurations;

public class ClinicConfiguration : IEntityTypeConfiguration<Clinic>
{
    public void Configure(EntityTypeBuilder<Clinic> b)
    {
        b.ToTable("Clinic");
        b.HasKey(c => c.Id);
        b.Property(c => c.Name).IsRequired().HasMaxLength(150);
        b.Property(c => c.Slug).IsRequired().HasMaxLength(150);
        b.HasIndex(c => c.Slug).IsUnique();
        b.Property(c => c.Email).HasMaxLength(255);
        b.Property(c => c.Phone).HasMaxLength(20);
        b.Property(c => c.Status).HasConversion<int>();   // was HasConversion<string>()
        b.HasQueryFilter(c => !c.IsDeleted);
    }
}
