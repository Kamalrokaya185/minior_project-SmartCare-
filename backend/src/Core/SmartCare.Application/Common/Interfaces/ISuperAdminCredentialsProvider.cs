namespace SmartCare.Application.Common.Interfaces;

public interface ISuperAdminCredentialsProvider
{
    Guid SuperAdminUserId { get; }
    bool Validate(string email, string password);
}
