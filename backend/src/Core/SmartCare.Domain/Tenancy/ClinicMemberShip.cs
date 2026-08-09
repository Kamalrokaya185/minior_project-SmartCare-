using SmartCare.SharedKernel;

namespace SmartCare.Domain.Tenancy;

public class ClinicMembership : AggregateRoot
{
    public Guid ClinicId { get; private set; }
    public Guid DoctorId { get; private set; }
    public Guid? DepartmentId { get; private set; }
    public decimal? ConsultationFee { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime JoinedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? LeftAtUtc { get; private set; }

    private ClinicMembership() { }

    public static ClinicMembership Create(Guid clinicId, Guid doctorId,
        Guid? departmentId = null, decimal? consultationFee = null)
    {
        if (consultationFee is < 0)
            throw new ArgumentException("Consultation fee cannot be negative.");

        return new ClinicMembership
        {
            ClinicId = clinicId,
            DoctorId = doctorId,
            DepartmentId = departmentId,
            ConsultationFee = consultationFee
        };
    }

    public void UpdateDetails(Guid? departmentId, decimal? consultationFee)
    {
        if (consultationFee is < 0)
            throw new ArgumentException("Consultation fee cannot be negative.");

        DepartmentId = departmentId;
        ConsultationFee = consultationFee;
    }

    public void Deactivate()
    {
        IsActive = false;
        LeftAtUtc = DateTime.UtcNow;
    }

    public void Reactivate()
    {
        IsActive = true;
        LeftAtUtc = null;
    }
}

