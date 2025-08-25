using MediatR;
using Microsoft.EntityFrameworkCore;
using SalesApp.Application.People;

namespace SalesApp.Application;

public sealed record GetPessoaByIdQuery(long Id) : IRequest<PessoaVm>;
public sealed record ListPessoasQuery(string? Nome, string? Cpf) : IRequest<List<PessoaVm>>;

internal sealed class GetPessoaByIdHandler(IAppDb db) : IRequestHandler<GetPessoaByIdQuery, PessoaVm>
{
  public async Task<PessoaVm> Handle(GetPessoaByIdQuery request, CancellationToken ct)
  {
    var p = await db.Pessoas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id, ct)
      ?? throw new KeyNotFoundException("Pessoa não encontrada");
    return p.ToVm();
  }
}

internal sealed class ListPessoasHandler(IAppDb db) : IRequestHandler<ListPessoasQuery, List<PessoaVm>>
{
  public async Task<List<PessoaVm>> Handle(ListPessoasQuery request, CancellationToken ct)
  {
    var q = db.Pessoas.AsNoTracking().AsQueryable();
    if (!string.IsNullOrWhiteSpace(request.Nome))
      q = q.Where(p => p.Nome.Contains(request.Nome));
    if (!string.IsNullOrWhiteSpace(request.Cpf))
      q = q.Where(p => p.Cpf == request.Cpf);

    var list = await q
      .OrderBy(p => p.Nome)
      .Select(p => p.ToVm())
      .ToListAsync(ct);

    return list;
  }
}
