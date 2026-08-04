using SmartCare.SharedKernel;

namespace SmartCare.Domain.Identity;

public class Role : AggregateRoot
{
    public RoleName Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsSystemRole { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    private Role() { }

    public static Role Create(RoleName name, string? description, bool isSystemRole)
        => new() { Name = name, Description = description, IsSystemRole = isSystemRole };
}
