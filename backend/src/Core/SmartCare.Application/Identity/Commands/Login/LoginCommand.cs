using System;
using System.Collections.Generic;
using System.Text;

using MediatR;
using SmartCare.SharedKernel;

namespace SmartCare.Application.Identity.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<Result<string>>; // returns JWT
