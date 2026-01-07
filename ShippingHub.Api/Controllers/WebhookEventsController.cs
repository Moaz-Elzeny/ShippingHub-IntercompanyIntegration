using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShippingHub.Application.Features.WebhookEvents;

namespace ShippingHub.Api.Controllers;

[ApiController]
[Route("api/webhook-events")]
public class WebhookEventsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<WebhookEventDto>>> GetAll(CancellationToken ct)
        => await mediator.Send(new GetWebhookEventsQuery(), ct);
}
