using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesApp.Application;
using SalesApp.Application.Products;

namespace SalesApp.WebApi.Controllers;

[ApiController]
[Route("api/produtos")]
public class ProdutosController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ProdutoVm>>> List(
      [FromQuery] string? nome,
      [FromQuery] string? codigo,
      [FromQuery] decimal? valorMin,
      [FromQuery] decimal? valorMax)
      => Ok(await mediator.Send(new ListProdutosQuery(nome, codigo, valorMin, valorMax)));

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ProdutoVm>> Get(long id)
      => Ok(await mediator.Send(new GetProdutoByIdQuery(id)));

    [HttpPost]
    public async Task<ActionResult<ProdutoVm>> Create([FromBody] CreateProdutoDto dto)
    {
        try
        {
            var vm = await mediator.Send(new CreateProdutoCommand(dto));
            return CreatedAtAction(nameof(Get), new { id = vm.Id }, vm);
        }
        catch (InvalidOperationException e) { return Conflict(new { error = e.Message }); }
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ProdutoVm>> Update(long id, [FromBody] UpdateProdutoDto dto)
    {
        try
        {
            var vm = await mediator.Send(new UpdateProdutoCommand(id, dto));
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
            await mediator.Send(new DeleteProdutoCommand(id));
            return NoContent();
        }
        catch (KeyNotFoundException e) { return NotFound(new { error = e.Message }); }
    }
}
