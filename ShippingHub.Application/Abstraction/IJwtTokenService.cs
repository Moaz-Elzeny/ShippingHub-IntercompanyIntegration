namespace ShippingHub.Application.Abstractions;

public interface IJwtTokenService
{
    string GenerateToken(int companyId, string companyName);
}
