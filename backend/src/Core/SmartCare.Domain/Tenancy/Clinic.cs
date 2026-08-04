using SmartCare.SharedKernel;

namespace SmartCare.Domain.Tenancy;

public enum ClinicStatus { Pending = 0, Approved = 1, Suspended = 2, Active = 3 }

public class Clinic : AggregateRoot
{
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }
    public string? LogoUrl { get; private set; }
    public ClinicStatus Status { get; private set; } = ClinicStatus.Pending;
    public DateTime? ApprovedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public bool IsDeleted { get; private set; }

    private Clinic() { }

    public static Clinic Register(string name, string slug, string? email, string? phone,
        string? address, string? city, string? state)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Clinic name is required");
        if (string.IsNullOrWhiteSpace(slug)) throw new ArgumentException("Clinic slug is required");

        return new Clinic
        {
            Name = name,
            Slug = slug.Trim().ToLowerInvariant(),
            Email = email,
            Phone = phone,
            Address = address,
            City = city,
            State = state
        };
    }

    public void Approve()
    {
        if (Status != ClinicStatus.Pending)
            throw new InvalidOperationException($"Cannot approve a clinic in status '{Status}'.");

        Status = ClinicStatus.Active;
        ApprovedAtUtc = DateTime.UtcNow;
    }

    public void Suspend()
    {
        if (Status != ClinicStatus.Active)
            throw new InvalidOperationException("Only an active clinic can be suspended.");

        Status = ClinicStatus.Suspended;
    }
}