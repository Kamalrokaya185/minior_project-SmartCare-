using System;
using System.Collections.Generic;
using System.Text;

namespace SmartCare.Domain.Identity;

public static class SystemRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Clinic = "Clinic";
    public const string Patient = "Patient";

    public static readonly Guid SuperAdminId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid ClinicId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    public static readonly Guid PatientId = Guid.Parse("00000000-0000-0000-0000-000000000003");
}
