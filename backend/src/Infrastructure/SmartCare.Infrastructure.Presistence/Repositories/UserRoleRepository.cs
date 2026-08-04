using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Identity;

namespace SmartCare.Infrastructure.Presistence.Repositories;

public class UserRoleRepository : IUserRoleRepository
{
    private readonly SmartCareDbContext _context;
    public UserRoleRepository(SmartCareDbContext context) => _context = context;

    public Task<UserRole?> GetByUserAndRoleAsync(Guid userId, Guid roleId, CancellationToken ct = default) =>
        _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, ct);

    public async Task<IReadOnlyList<RoleName>> GetRoleNamesForUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await (from ur in _context.UserRoles
                      join r in _context.Roles on ur.RoleId equals r.Id
                      where ur.UserId == userId
                      select r.Name).ToListAsync(ct);
    }

    public async Task AddAsync(UserRole userRole, CancellationToken ct = default) =>
        await _context.UserRoles.AddAsync(userRole, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}
