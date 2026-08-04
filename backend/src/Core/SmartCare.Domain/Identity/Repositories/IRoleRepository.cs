using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SmartCare.Domain.Identity;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(RoleName name, CancellationToken ct = default);
    Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default);
}
