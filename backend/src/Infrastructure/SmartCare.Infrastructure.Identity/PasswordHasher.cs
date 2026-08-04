using System;
using System.Collections.Generic;
using System.Text;

using SmartCare.Application.Common.Interfaces;

namespace SmartCare.Infrastructure.Identity;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);
    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
