using Microsoft.Extensions.Configuration;
using SmartCare.Application.Common.Interfaces;

namespace SmartCare.Infrastructure.Identity;

public class SuperAdminCredentialsProvider : ISuperAdminCredentialsProvider
{
    private readonly string _email;
    private readonly string _password;

    // Fixed, well-known ID — same idea as SystemRoles' fixed GUIDs. Never appears in the Users table.
    public Guid SuperAdminUserId { get; } = Guid.Parse("00000000-0000-0000-0000-000000000099");

    public SuperAdminCredentialsProvider(IConfiguration config)
    {
        _email = config["SuperAdmin:Email"]
            ?? throw new InvalidOperationException("SuperAdmin:Email is not configured.");
        _password = config["SuperAdmin:Password"]
            ?? throw new InvalidOperationException("SuperAdmin:Password is not configured.");
    }

    public bool Validate(string email, string password) =>
        string.Equals(email?.Trim(), _email, StringComparison.OrdinalIgnoreCase)
        && password == _password;
}
