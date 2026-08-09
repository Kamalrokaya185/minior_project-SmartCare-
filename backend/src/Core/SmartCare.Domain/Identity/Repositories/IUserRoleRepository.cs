using System;
using System.Collections.Generic;
using System.Text;

namespace SmartCare.Domain.Identity;

public interface IUserRoleRepository
{
    Task<UserRole?> GetByUserAndRoleAsync(Guid userId, Guid roleId, CancellationToken ct = default);
    Task<IReadOnlyList<RoleName>> GetRoleNamesForUserAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(UserRole userRole, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task<Guid?> GetProfileIdNamesForUserAsync(Guid userId, CancellationToken ct = default);
}

