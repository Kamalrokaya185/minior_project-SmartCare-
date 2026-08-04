using System;
using System.Collections.Generic;
using System.Text;

using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Patients.Commands.CreatePatientProfile;

public record CreatePatientProfileCommand(
    Guid UserId, string? Gender, DateOnly? DateOfBirth, string? NID,
    string? EmergencyContactName, string? EmergencyContactRelationship, string? EmergencyContactPhone)
    : IRequest<Result<Guid>>;
