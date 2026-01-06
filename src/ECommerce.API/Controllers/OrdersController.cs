using MediatR;
using Microsoft.AspNetCore.Mvc;
using Orders.Application.Commands.CancelOrder;
using Orders.Application.Commands.CreateOrder;
using Orders.Application.Commands.PayOrder;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
    {
        var orderId = await _mediator.Send(command);
        return Ok(orderId);
    }

    [HttpPost("{orderId:guid}/pay")]
    public async Task<IActionResult> Pay(Guid orderId, CancellationToken ct)
    {
        await _mediator.Send(new PayOrderCommand(orderId), ct);
        return NoContent();
    }

    [HttpPost("{orderId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid orderId, CancellationToken ct)
    {
        await _mediator.Send(new CancelOrderCommand(orderId), ct);
        return NoContent();
    }
}
