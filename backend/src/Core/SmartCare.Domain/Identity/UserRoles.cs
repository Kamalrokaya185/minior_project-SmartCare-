using System;
using System.Collections.Generic;
using System.Text;

using SmartCare.SharedKernel;

namespace SmartCare.Domain.Identity;

public class UserRole : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public Guid? ProfileId { get; private set; }   // Points to PatientProfiles/DoctorProfile/Clinic depending on RoleId
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    private UserRole() { }

    public static UserRole Create(Guid userId, Guid roleId, Guid? profileId)
        => new() { UserId = userId, RoleId = roleId, ProfileId = profileId };
}
