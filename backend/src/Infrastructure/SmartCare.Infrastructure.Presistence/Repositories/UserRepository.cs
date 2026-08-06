using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using SmartCare.Domain.Identity;
using SmartCare.Domain.Identity.Repositories;

namespace SmartCare.Infrastructure.Presistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly SmartCareDbContext _context;
    public UserRepository(SmartCareDbContext context) => _context = context;

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        _context.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower(), ct);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        await _context.Users.AddAsync(user, ct);
        //await _context.SaveChangesAsync(ct);
    }
}
