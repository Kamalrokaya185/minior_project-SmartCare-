using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartCare.Domain.Identity;

namespace SmartCare.Infrastructure.Presistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> b)
    {
        b.ToTable("Roles");
        b.HasKey(r => r.Id);
        b.Property(r => r.Name).HasConversion<int>();
        b.HasIndex(r => r.Name).IsUnique();
        b.Property(r => r.Description).HasMaxLength(250);

        b.HasData(
            SeedRole(SystemRoles.SuperAdminId, RoleName.SuperAdmin, "Platform administrator"),
            SeedRole(SystemRoles.ClinicId, RoleName.Clinic, "Clinic owner / front-desk staff"),
            SeedRole(SystemRoles.PatientId, RoleName.Patient, "Patient booking appointments")
        );
    }

    private static Role SeedRole(Guid id, RoleName name, string description)
    {
        var role = Role.Create(name, description, true);

        // Set static Id
        typeof(Role).GetProperty(nameof(Role.Id))!.SetValue(role, id);

        // ✨ FIX: Set static date so EF Core stops throwing the warning
        var staticDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        typeof(Role).GetProperty(nameof(Role.CreatedAtUtc))!.SetValue(role, staticDate);

        return role;
    }

}
