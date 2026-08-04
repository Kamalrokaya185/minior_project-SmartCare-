using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCare.Domain.Identity;

namespace SmartCare.Infrastructure.Presistence.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> b)
    {
        b.ToTable("UserRoles");
        b.HasKey(ur => ur.Id);
        b.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();

        b.HasOne<User>().WithMany().HasForeignKey(ur => ur.UserId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne<Role>().WithMany().HasForeignKey(ur => ur.RoleId).OnDelete(DeleteBehavior.Restrict);
        // ProfileId is intentionally NOT a real FK (polymorphic — points to different tables
        // depending on RoleId). Validity is checked in the Application layer, not the database.
    }
}
