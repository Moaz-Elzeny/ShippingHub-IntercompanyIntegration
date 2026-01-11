using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShippingHub.Application.Features.Companies;

namespace ShippingHub.Api.Controllers;

[ApiController]
[Route("api/companies")]
[Authorize]
public class CompaniesController(IMediator mediator) : ControllerBase
{
    [HttpGet("available")]
    public async Task<ActionResult<List<AvailableCompanyDto>>> Available(CancellationToken ct)
        => await mediator.Send(new GetAvailableCompaniesQuery(), ct);
}
