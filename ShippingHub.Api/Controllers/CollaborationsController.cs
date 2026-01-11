using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShippingHub.Application.Features.Collaborations;

namespace ShippingHub.Api.Controllers;

[ApiController]
[Route("api/collaborations")]
[Authorize]
public class CollaborationsController(IMediator mediator) : ControllerBase
{
    [HttpPost("requests")]
    public async Task<ActionResult<object>> Send([FromBody] SendCollaborationRequestRequest req, CancellationToken ct)
    {
        var id = await mediator.Send(new SendCollaborationRequestCommand(req.ReceiverCompanyId), ct);
        return Ok(new { requestId = id });
    }

    [HttpGet("requests/incoming")]
    public async Task<ActionResult<List<CollaborationRequestDto>>> Incoming(CancellationToken ct)
        => await mediator.Send(new GetIncomingRequestsQuery(), ct);

    [HttpGet("requests/outgoing")]
    public async Task<ActionResult<List<CollaborationRequestDto>>> Outgoing(CancellationToken ct)
        => await mediator.Send(new GetOutgoingRequestsQuery(), ct);

    [HttpPost("requests/{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        await mediator.Send(new ApproveRequestCommand(id), ct);
        return NoContent();
    }

    [HttpPost("requests/{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, CancellationToken ct)
    {
        await mediator.Send(new RejectRequestCommand(id), ct);
        return NoContent();
    }
}
