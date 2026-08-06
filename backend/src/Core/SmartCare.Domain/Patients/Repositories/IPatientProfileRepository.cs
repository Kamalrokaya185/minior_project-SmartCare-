using System;
using System.Collections.Generic;
using System.Text;

namespace SmartCare.Domain.Patients;

public interface IPatientProfileRepository
{
    Task<PatientProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<PatientProfile?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(PatientProfile profile, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<PatientProfile>> GetAllAsync(CancellationToken ct = default);

}

