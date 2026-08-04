using System;
using System.Collections.Generic;
using System.Text;

namespace SmartCare.Application.Common.Interfaces;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
