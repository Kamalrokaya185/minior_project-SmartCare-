using MediatR;
using SmartCare.Domain.ClinicalDirectory;
using SmartCare.Domain.Tenancy;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Tenancy.Commands.RegisterDoctorAtClinic;

public class RegisterDoctorAtClinicCommandHandler : IRequestHandler<RegisterDoctorAtClinicCommand, Result<Guid>>
{
    private readonly IDoctorProfileRepository _doctorProfileRepository;
    private readonly IClinicMembershipRepository _membershipRepository;
    private readonly IDoctorScheduleRepository _scheduleRepository;

    public RegisterDoctorAtClinicCommandHandler(
        IDoctorProfileRepository doctorProfileRepository,
        IClinicMembershipRepository membershipRepository,
        IDoctorScheduleRepository scheduleRepository)
    {
        _doctorProfileRepository = doctorProfileRepository;
        _membershipRepository = membershipRepository;
        _scheduleRepository = scheduleRepository;
    }

    public async Task<Result<Guid>> Handle(RegisterDoctorAtClinicCommand request, CancellationToken ct)
    {
        // Step 1: search by License + Specialization
        var doctorProfile = await _doctorProfileRepository.GetByLicenseAndSpecializationAsync(
            request.LicenseNumber, request.Specialization, ct);

        // Step 2: not found -> create a new DoctorProfile
        if (doctorProfile is null)
        {
            try
            {
                doctorProfile = DoctorProfile.Create(request.FullName, request.LicenseNumber, request.Specialization, request.Gender);
            }
            catch (ArgumentException ex)
            {
                return Result<Guid>.Failure(ex.Message);
            }

            await _doctorProfileRepository.AddAsync(doctorProfile, ct);
            await _doctorProfileRepository.SaveChangesAsync(ct);
        }

        // Step 3: create the ClinicMembership
        var alreadyMember = await _membershipRepository.ExistsAsync(request.ClinicId, doctorProfile.Id, ct);
        if (alreadyMember)
            return Result<Guid>.Failure("This doctor is already assigned to this clinic.");

        var membership = ClinicMembership.Create(
            request.ClinicId, doctorProfile.Id, request.DepartmentId, request.ConsultationFee);

        await _membershipRepository.AddAsync(membership, ct);
        await _membershipRepository.SaveChangesAsync(ct); // commit so membership.Id exists for the schedule below

        // Step 4: optionally create the initial schedule, using the new membership's Id
        if (request.StartTime is not null && request.EndTime is not null && request.SlotDurationMinutes is not null)
        {
            try
            {
                DoctorSchedule schedule = (request.IsRecurring ?? true)
                    ? DoctorSchedule.CreateRecurring(membership.Id, request.DayOfWeek!.Value,
                        request.StartTime.Value, request.EndTime.Value, request.SlotDurationMinutes.Value,
                        request.EffectiveFrom ?? DateOnly.FromDateTime(DateTime.UtcNow.Date), request.EffectiveTo)
                    : DoctorSchedule.CreateOneTime(membership.Id, request.SpecificDate!.Value,
                        request.StartTime.Value, request.EndTime.Value, request.SlotDurationMinutes.Value);

                await _scheduleRepository.AddAsync(schedule, ct);
                await _scheduleRepository.SaveChangesAsync(ct);
            }
            catch (ArgumentException ex)
            {
                // Doctor + membership were already created successfully — only the schedule failed.
                // Membership stays valid; return a partial-success message rather than rolling everything back.
                return Result<Guid>.Failure(
                    $"Doctor added successfully, but the schedule could not be created: {ex.Message}. You can add a schedule later from Edit.");
            }
        }

        return Result<Guid>.Success(membership.Id);
    }
}
