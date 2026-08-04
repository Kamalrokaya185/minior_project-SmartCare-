using System;
using System.Collections.Generic;
using System.Text;

using SmartCare.SharedKernel;

namespace SmartCare.Domain.Patients;

public class PatientProfile : AggregateRoot
{
    public Guid UserId { get; private set; }
    public string? Gender { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public string? NID { get; private set; }
    public string? EmergencyContactName { get; private set; }
    public string? EmergencyContactRelationship { get; private set; }
    public string? EmergencyContactPhone { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; private set; }

    private PatientProfile() { }

    public static PatientProfile Create(Guid userId, string? gender, DateOnly? dob, string? nid,
        string? emergencyName, string? emergencyRelationship, string? emergencyPhone)
    {
        return new PatientProfile
        {
            UserId = userId,
            Gender = gender,
            DateOfBirth = dob,
            NID = nid,
            EmergencyContactName = emergencyName,
            EmergencyContactRelationship = emergencyRelationship,
            EmergencyContactPhone = emergencyPhone
        };
    }

    public void UpdateEmergencyContact(string? name, string? relationship, string? phone)
    {
        EmergencyContactName = name;
        EmergencyContactRelationship = relationship;
        EmergencyContactPhone = phone;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}

