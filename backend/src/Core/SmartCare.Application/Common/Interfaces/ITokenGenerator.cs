using System;
using System.Collections.Generic;
using System.Text;
using SmartCare.Domain.Identity;

namespace SmartCare.Application.Common.Interfaces;

public interface ITokenGenerator
{
    string GenerateAccessToken(User user, IEnumerable<string> roles);
}
