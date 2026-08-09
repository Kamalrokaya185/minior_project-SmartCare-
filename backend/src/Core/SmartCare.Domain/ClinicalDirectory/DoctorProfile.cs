using SmartCare.SharedKernel;

namespace SmartCare.Domain.ClinicalDirectory;

public class DoctorProfile : AggregateRoot
{
    public string FullName { get; private set; } = default!;
    public string LicenseNumber { get; private set; } = default!;
    public string Specialization { get; private set; } = default!;
    public string? Gender { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    private DoctorProfile() { }

    public static DoctorProfile Create(string fullName, string licenseNumber, string specialization, string? gender)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Doctor's full name is required.");
        if (string.IsNullOrWhiteSpace(licenseNumber))
            throw new ArgumentException("License number is required.");
        if (string.IsNullOrWhiteSpace(specialization))
            throw new ArgumentException("Specialization is required.");

        return new DoctorProfile
        {
            FullName = fullName,
            LicenseNumber = licenseNumber,
            Specialization = specialization,
            Gender = gender
        };
    }
    public void UpdateDetails(string fullName,string specialization, string? gender)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Doctor's full name is required.");
        if (string.IsNullOrWhiteSpace(specialization))
            throw new ArgumentException("Doctor's specislization is required.");
        FullName = fullName;
        Specialization = specialization;
        Gender = gender;
    }
}
