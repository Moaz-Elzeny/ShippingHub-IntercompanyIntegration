using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShippingHub.Application.Features.WebhookDeliveries;
using ShippingHub.Domain.Enums;

namespace ShippingHub.Api.Controllers;

[ApiController]
[Route("api/webhook-deliveries")]
[Authorize]
public class WebhookDeliveriesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<WebhookDeliveryListItemDto>>> List(
        [FromQuery] WebhookDeliveryStatus? status,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
        => await mediator.Send(new GetWebhookDeliveriesQuery(status, fromUtc, toUtc, page, pageSize), ct);

    [HttpGet("summary")]
    public async Task<ActionResult<WebhookDeliverySummaryDto>> Summary(CancellationToken ct)
        => await mediator.Send(new GetWebhookDeliveriesSummaryQuery(), ct);

    [HttpGet("{id:long}")]
    public async Task<ActionResult<WebhookDeliveryDetailsDto>> Get(int id, CancellationToken ct)
        => await mediator.Send(new GetWebhookDeliveryByIdQuery(id), ct);

    [HttpPost("{id:long}/retry")]
    public async Task<IActionResult> Retry(int id, CancellationToken ct)
    {
        await mediator.Send(new RetryWebhookDeliveryCommand(id), ct);
        return NoContent();
    }
}
