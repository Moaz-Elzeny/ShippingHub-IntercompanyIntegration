using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShippingHub.Application.Features.Shipments;

namespace ShippingHub.Api.Controllers;

[ApiController]
[Route("api/shipments")]
[Authorize]
public class ShipmentsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ShipmentDto>>> Mine(CancellationToken ct)
        => await mediator.Send(new GetMyShipmentsQuery(), ct);

    [HttpPost]
    public async Task<ActionResult<ShipmentDto>> Create([FromBody] CreateShipmentRequestDto dto, CancellationToken ct)
    => await mediator.Send(new CreateShipmentCommand(dto.ReceiverCompanyId, dto), ct);

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ShipmentDto>> Update(int id, [FromBody] UpdateShipmentRequest req, CancellationToken ct)
        => await mediator.Send(new UpdateShipmentCommand(id, req.PayloadJson, req.Status), ct);
}
