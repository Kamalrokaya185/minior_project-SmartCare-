namespace SmartCare.Domain.Appointments;

public enum AppointmentStatus
{
    Pending = 0,      // Booked, awaiting payment proof + verification
    Confirmed = 1,    // Payment verified by receptionist
    CheckedIn = 2,    // Patient has arrived at the clinic
    Completed = 3,    // Consultation done
    Cancelled = 4,    // Cancelled by patient or clinic
    Rejected = 5,     // Clinic rejected the booking outright
    NoShow = 6,       // Patient never arrived
    Expired = 7        // Payment window lapsed with no proof submitted
}

public enum PaymentStatus
{
    NotPaid = 0,
    AwaitingVerification = 1,
    Verified = 2,
    Rejected = 3
}
