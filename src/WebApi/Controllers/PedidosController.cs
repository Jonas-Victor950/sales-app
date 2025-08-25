using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesApp.Application;
using SalesApp.Application.Orders;
using SalesApp.Domain;

namespace SalesApp.WebApi.Controllers;

[ApiController]
[Route("api/pedidos")]
public class PedidosController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PedidoVm>>> List(
      [FromQuery] PedidoStatus? status,
      [FromQuery] long? pessoaId)
      => Ok(await mediator.Send(new ListPedidosQuery(status, pessoaId)));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<PedidoVm>> Get(long id)
      => Ok(await mediator.Send(new GetPedidoByIdQuery(id)));

    [HttpPost]
    public async Task<ActionResult<PedidoVm>> Create([FromBody] CreatePedidoDto dto)
    {
        try
        {
            var vm = await mediator.Send(new CreatePedidoCommand(dto));
            return CreatedAtAction(nameof(Get), new { id = vm.Id }, vm);
        }
        catch (KeyNotFoundException e) { return NotFound(new { error = e.Message }); }
        catch (InvalidOperationException e) { return Conflict(new { error = e.Message }); }
    }

    [HttpPost("{id:long}/marcar-pago")]
    public async Task<ActionResult<PedidoVm>> MarcarPago(long id)
    {
        try { return Ok(await mediator.Send(new MarcarPedidoPagoCommand(id))); }
        catch (KeyNotFoundException e) { return NotFound(new { error = e.Message }); }
        catch (InvalidOperationException e) { return Conflict(new { error = e.Message }); }
    }

    [HttpPost("{id:long}/marcar-enviado")]
    public async Task<ActionResult<PedidoVm>> MarcarEnviado(long id)
    {
        try { return Ok(await mediator.Send(new MarcarPedidoEnviadoCommand(id))); }
        catch (KeyNotFoundException e) { return NotFound(new { error = e.Message }); }
        catch (InvalidOperationException e) { return Conflict(new { error = e.Message }); }
    }

    [HttpPost("{id:long}/marcar-recebido")]
    public async Task<ActionResult<PedidoVm>> MarcarRecebido(long id)
    {
        try { return Ok(await mediator.Send(new MarcarPedidoRecebidoCommand(id))); }
        catch (KeyNotFoundException e) { return NotFound(new { error = e.Message }); }
        catch (InvalidOperationException e) { return Conflict(new { error = e.Message }); }
    }
}
