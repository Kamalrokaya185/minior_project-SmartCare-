using SmartCare.Domain.Appointments;
using SmartCare.Domain.Tenancy;

namespace SmartCare.Application.Appointments.Services;

public static class RefundCalculationService
{
    /// <summary>Returns the eligible refund amount, or null if the cancellation isn't eligible at all.</summary>
    public static decimal? CalculateEligibleRefund(Appointment appointment, ClinicPolicy? policy, DateTime nowUtc)
    {
        if (policy is null) return null; // no policy configured — safest default is no automatic refund

        var appointmentDateTime = appointment.AppointmentDate.ToDateTime(appointment.AppointmentTime);
        var hoursUntilAppointment = (appointmentDateTime - nowUtc).TotalHours;

        if (hoursUntilAppointment < policy.CancellationWindowHours)
            return null; // cancelled too close to the appointment — not eligible, per policy

        var refundAmount = appointment.FeeAtBooking * (policy.RefundPercentage / 100m);
        return refundAmount;
    }
}
