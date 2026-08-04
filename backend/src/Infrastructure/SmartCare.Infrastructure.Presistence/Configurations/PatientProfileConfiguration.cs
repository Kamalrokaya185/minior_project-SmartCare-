using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCare.Domain.Identity;
using SmartCare.Domain.Patients;

namespace SmartCare.Infrastructure.Presistence.Configurations;

public class PatientProfileConfiguration : IEntityTypeConfiguration<PatientProfile>
{
    public void Configure(EntityTypeBuilder<PatientProfile> b)
    {
        b.ToTable("PatientProfiles");
        b.HasKey(p => p.Id);
        b.Property(p => p.Gender).HasMaxLength(20);
        b.Property(p => p.NID).HasMaxLength(50);
        b.HasIndex(p => p.NID).IsUnique().HasFilter("NID IS NOT NULL");
        b.HasIndex(p => p.UserId).IsUnique();
        b.HasOne<User>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
