using SmartCare.SharedKernel;

namespace SmartCare.Domain.ClinicalDirectory;

public class DoctorSchedule : AggregateRoot
{
    public Guid ClinicMembershipId { get; private set; }
    public int? DayOfWeek { get; private set; }        // 1=Monday ... 7=Sunday, per spec. Null if SpecificDate is used.
    public DateOnly? SpecificDate { get; private set; } // Used for one-time schedules instead of recurring days.
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public int SlotDurationMinutes { get; private set; }
    public bool IsRecurring { get; private set; }
    public DateOnly EffectiveFrom { get; private set; }
    public DateOnly? EffectiveTo { get; private set; }
    public bool IsActive { get; private set; } = true;

    private DoctorSchedule() { }

    public static DoctorSchedule CreateRecurring(Guid clinicMembershipId, int dayOfWeek,
        TimeOnly startTime, TimeOnly endTime, int slotDurationMinutes,
        DateOnly effectiveFrom, DateOnly? effectiveTo)
    {
        Validate(dayOfWeek, startTime, endTime, slotDurationMinutes);

        return new DoctorSchedule
        {
            ClinicMembershipId = clinicMembershipId,
            DayOfWeek = dayOfWeek,
            IsRecurring = true,
            StartTime = startTime,
            EndTime = endTime,
            SlotDurationMinutes = slotDurationMinutes,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo
        };
    }

    public static DoctorSchedule CreateOneTime(Guid clinicMembershipId, DateOnly specificDate,
        TimeOnly startTime, TimeOnly endTime, int slotDurationMinutes)
    {
        Validate(null, startTime, endTime, slotDurationMinutes);

        return new DoctorSchedule
        {
            ClinicMembershipId = clinicMembershipId,
            SpecificDate = specificDate,
            IsRecurring = false,
            StartTime = startTime,
            EndTime = endTime,
            SlotDurationMinutes = slotDurationMinutes,
            EffectiveFrom = specificDate,
            EffectiveTo = specificDate
        };
    }

    private static void Validate(int? dayOfWeek, TimeOnly startTime, TimeOnly endTime, int slotDurationMinutes)
    {
        if (dayOfWeek is < 1 or > 7) throw new ArgumentException("DayOfWeek must be 1 (Monday) to 7 (Sunday).");
        if (endTime <= startTime) throw new ArgumentException("EndTime must be after StartTime.");
        if (slotDurationMinutes <= 0) throw new ArgumentException("SlotDurationMinutes must be positive.");
    }

    public void Reactivate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    /// <summary>Generates the raw candidate time slices for this schedule, with no knowledge of what's already taken.</summary>
    public IReadOnlyList<(TimeOnly Start, TimeOnly End)> GenerateCandidateSlots()
    {
        var slots = new List<(TimeOnly, TimeOnly)>();
        var cursor = StartTime;
        while (cursor.AddMinutes(SlotDurationMinutes) <= EndTime)
        {
            var slotEnd = cursor.AddMinutes(SlotDurationMinutes);
            slots.Add((cursor, slotEnd));
            cursor = slotEnd;
        }
        return slots;
    }
}
