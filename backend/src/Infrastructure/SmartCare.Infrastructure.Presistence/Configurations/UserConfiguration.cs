using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCare.Domain.Identity;

namespace SmartCare.Infrastructure.Presistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("Users");
        b.HasKey(u => u.Id);
        b.Property(u => u.Email).IsRequired().HasMaxLength(256);
        b.HasIndex(u => u.Email).IsUnique();
        b.Property(u => u.PasswordHash).IsRequired();
        b.Property(u => u.FullName).IsRequired().HasMaxLength(200);
    }
}

