namespace SmartCare.Domain.ClinicalDirectory;

public record AvailableTimeSlot(TimeOnly StartTime, TimeOnly EndTime, bool IsAvailable);

public static class SlotAvailabilityService
{
    /// <summary>
    /// Combines a doctor's schedule templates for a date with the list of times already taken,
    /// producing the full picture the frontend needs to render a slot picker.
    /// </summary>
    public static IReadOnlyList<AvailableTimeSlot> BuildAvailability(
        IEnumerable<DoctorSchedule> applicableSchedules,
        IEnumerable<TimeOnly> takenStartTimes)
    {
        var taken = takenStartTimes.ToHashSet();
        var result = new List<AvailableTimeSlot>();

        foreach (var schedule in applicableSchedules)
        {
            foreach (var (start, end) in schedule.GenerateCandidateSlots())
            {
                result.Add(new AvailableTimeSlot(start, end, IsAvailable: !taken.Contains(start)));
            }
        }

        return result.OrderBy(s => s.StartTime).ToList();
    }

    /// <summary>1=Monday ... 7=Sunday, matching the DoctorSchedules.DayOfWeek convention.</summary>
    public static int ToScheduleDayOfWeek(DateOnly date)
    {
        var dow = (int)date.DayOfWeek; // Sunday=0 ... Saturday=6
        return dow == 0 ? 7 : dow;
    }
}