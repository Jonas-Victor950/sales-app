using MediatR;
using Microsoft.EntityFrameworkCore;
using SalesApp.Application.People;
using SalesApp.Domain;

namespace SalesApp.Application;

public sealed record CreatePessoaCommand(CreatePessoaDto Dto) : IRequest<PessoaVm>;
public sealed record UpdatePessoaCommand(long Id, UpdatePessoaDto Dto) : IRequest<PessoaVm>;
public sealed record DeletePessoaCommand(long Id) : IRequest<Unit>;

internal sealed class CreatePessoaHandler(IAppDb db) : IRequestHandler<CreatePessoaCommand, PessoaVm>
{
  public async Task<PessoaVm> Handle(CreatePessoaCommand request, CancellationToken ct)
  {
    var dto = request.Dto;
    var exists = await db.Pessoas.AnyAsync(p => p.Cpf == dto.Cpf, ct);
    if (exists) throw new InvalidOperationException("CPF já cadastrado");

    var entity = new Pessoa { Nome = dto.Nome, Cpf = dto.Cpf, Endereco = dto.Endereco };
    await db.AddAsync(entity, ct);
    await db.SaveChangesAsync(ct);
    return entity.ToVm();
  }
}

internal sealed class UpdatePessoaHandler(IAppDb db) : IRequestHandler<UpdatePessoaCommand, PessoaVm>
{
  public async Task<PessoaVm> Handle(UpdatePessoaCommand request, CancellationToken ct)
  {
    var p = await db.Pessoas.FirstOrDefaultAsync(x => x.Id == request.Id, ct)
      ?? throw new KeyNotFoundException("Pessoa não encontrada");

    if (p.Cpf != request.Dto.Cpf)
    {
      var exists = await db.Pessoas.AnyAsync(x => x.Cpf == request.Dto.Cpf && x.Id != p.Id, ct);
      if (exists) throw new InvalidOperationException("CPF já cadastrado");
    }

    p.Nome = request.Dto.Nome;
    p.Cpf = request.Dto.Cpf;
    p.Endereco = request.Dto.Endereco;

    await db.UpdateAsync(p, ct);
    await db.SaveChangesAsync(ct);
    return p.ToVm();
  }
}

internal sealed class DeletePessoaHandler(IAppDb db) : IRequestHandler<DeletePessoaCommand, Unit>
{
  public async Task<Unit> Handle(DeletePessoaCommand request, CancellationToken ct)
  {
    var p = await db.Pessoas.FirstOrDefaultAsync(x => x.Id == request.Id, ct)
      ?? throw new KeyNotFoundException("Pessoa não encontrada");
    await db.DeleteAsync(p, ct);
    await db.SaveChangesAsync(ct);
    return Unit.Value;
  }
}
