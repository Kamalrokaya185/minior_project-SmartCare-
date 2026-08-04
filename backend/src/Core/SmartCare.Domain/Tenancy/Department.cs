using SmartCare.SharedKernel;

namespace SmartCare.Domain.Tenancy;

public class Department : AggregateRoot
{
    public Guid ClinicId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    private Department() { }

    public static Department Create(Guid clinicId, string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Department name is required");
        return new Department { ClinicId = clinicId, Name = name, Description = description };
    }

    public void Deactivate() => IsActive = false;
}
