using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesApp.Application;
using SalesApp.Application.People;

namespace SalesApp.WebApi.Controllers;

[ApiController]
[Route("api/pessoas")]
public class PessoasController(IMediator mediator) : ControllerBase
{
  [HttpGet]
  public async Task<ActionResult<List<PessoaVm>>> List([FromQuery] string? nome, [FromQuery] string? cpf)
    => Ok(await mediator.Send(new ListPessoasQuery(nome, cpf)));

  [HttpGet("{id:long}")]
  public async Task<ActionResult<PessoaVm>> Get(long id)
    => Ok(await mediator.Send(new GetPessoaByIdQuery(id)));

  [HttpPost]
  public async Task<ActionResult<PessoaVm>> Create([FromBody] CreatePessoaDto dto)
  {
    try
    {
      var vm = await mediator.Send(new CreatePessoaCommand(dto));
      return CreatedAtAction(nameof(Get), new { id = vm.Id }, vm);
    }
    catch (InvalidOperationException e) { return Conflict(new { error = e.Message }); }
  }

  [HttpPut("{id:long}")]
  public async Task<ActionResult<PessoaVm>> Update(long id, [FromBody] UpdatePessoaDto dto)
  {
    try
    {
      var vm = await mediator.Send(new UpdatePessoaCommand(id, dto));
      return Ok(vm);
    }
    catch (KeyNotFoundException e) { return NotFound(new { error = e.Message }); }
    catch (InvalidOperationException e) { return Conflict(new { error = e.Message }); }
  }

  [HttpDelete("{id:long}")]
  public async Task<IActionResult> Delete(long id)
  {
    try
    {
      await mediator.Send(new DeletePessoaCommand(id));
      return NoContent();
    }
    catch (KeyNotFoundException e) { return NotFound(new { error = e.Message }); }
  }
}
