using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShippingHub.Application.Features.Webhooks;

namespace ShippingHub.Api.Controllers;

[ApiController]
[Route("api/webhooks")]
[Authorize]
public class WebhooksController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<WebhookDto>>> GetMine(CancellationToken ct)
        => await mediator.Send(new GetMyWebhooksQuery(), ct);

    [HttpPost]
    public async Task<ActionResult<WebhookDto>> Create([FromBody] CreateWebhookRequest req, CancellationToken ct)
        => await mediator.Send(new CreateWebhookCommand(req.EventId, req.Url), ct);

    [HttpPatch("{id:int}/active")]
    public async Task<IActionResult> Toggle(int id, [FromBody] ToggleWebhookRequest req, CancellationToken ct)
    {
        await mediator.Send(new ToggleWebhookCommand(id, req.IsActive), ct);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await mediator.Send(new DeleteWebhookCommand(id), ct);
        return NoContent();
    }
}
