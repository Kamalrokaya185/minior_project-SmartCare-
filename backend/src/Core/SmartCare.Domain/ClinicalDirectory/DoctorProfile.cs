using SmartCare.SharedKernel;

namespace SmartCare.Domain.ClinicalDirectory;

public class DoctorProfile : AggregateRoot
{
    public string FullName { get; private set; } = default!;   // ← new
    public string LicenseNumber { get; private set; } = default!;
    public string? Qualification { get; private set; }
    public string? Specialization { get; private set; }
    public int? ExperienceYear { get; private set; }
    public string? Gender { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? PhotoUrl { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? Biography { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    private DoctorProfile() { }

    public static DoctorProfile Create(string fullName, string licenseNumber, string? qualification,
        string? specialization, int? experienceYear, string? gender, string? phone, string? email,
        string? photoUrl, string? biography)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Doctor's full name is required.");
        if (string.IsNullOrWhiteSpace(licenseNumber))
            throw new ArgumentException("License number is required.");

        return new DoctorProfile
        {
            FullName = fullName,
            LicenseNumber = licenseNumber,
            Qualification = qualification,
            Specialization = specialization,
            ExperienceYear = experienceYear,
            Gender = gender,
            Phone = phone,
            Email = email,
            PhotoUrl = photoUrl,
            Biography = biography
        };
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}