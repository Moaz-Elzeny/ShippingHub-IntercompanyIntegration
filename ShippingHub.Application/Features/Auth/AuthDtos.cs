namespace ShippingHub.Application.Features.Auth;

public sealed record RegisterCompanyRequest(string Name);
public sealed record LoginCompanyRequest(int CompanyId, string ApiKey);

public sealed record AuthResponse(int CompanyId, string Name, string Token);
