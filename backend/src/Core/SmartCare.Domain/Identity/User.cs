using System;
using System.Collections.Generic;
using System.Text;

using SmartCare.SharedKernel;

namespace SmartCare.Domain.Identity;


public class User : AggregateRoot
{
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public string FullName { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    private User() { } // EF Core

    public static User Register(string email, string passwordHash, string fullName)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required");
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("Full name is required");

        var user = new User
        {
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            FullName = fullName,
        };
        return user;
    }
}
