using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Identity;

namespace SmartCare.Infrastructure.Presistence.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly SmartCareDbContext _context;
    public RoleRepository(SmartCareDbContext context) => _context = context;

    public Task<Role?> GetByNameAsync(RoleName name, CancellationToken ct = default) =>
        _context.Roles.FirstOrDefaultAsync(r => r.Name == name, ct);

    public Task<Role?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.Roles.FirstOrDefaultAsync(r => r.Id == id, ct);
}
