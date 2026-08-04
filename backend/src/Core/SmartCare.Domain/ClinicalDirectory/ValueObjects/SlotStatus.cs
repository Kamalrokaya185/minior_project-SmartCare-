namespace SmartCare.Domain.ClinicalDirectory;

// Available is never physically stored — a row only exists once a slot is first touched.
// Cancelled means "was touched, is free again" — the availability query treats it as open,
// same as a slot with no row at all.
public enum SlotStatus
{
    Available = 0,   // never stored — computed by absence of a row
    Reserved = 1,
    Booked = 2,
    Completed = 3,
    Blocked = 4,
    Cancelled = 5
}
