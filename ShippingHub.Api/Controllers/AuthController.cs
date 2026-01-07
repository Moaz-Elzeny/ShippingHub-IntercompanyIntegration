using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShippingHub.Application.Features.Auth;

namespace ShippingHub.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterCompanyRequest req, CancellationToken ct)
        => await mediator.Send(new RegisterCompanyCommand(req.Name), ct);

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginCompanyRequest req, CancellationToken ct)
        => await mediator.Send(new LoginCompanyCommand(req.CompanyId, req.ApiKey), ct);
}
