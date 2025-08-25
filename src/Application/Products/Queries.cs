using MediatR;
using Microsoft.EntityFrameworkCore;
using SalesApp.Application.Products;

namespace SalesApp.Application;

public sealed record GetProdutoByIdQuery(long Id) : IRequest<ProdutoVm>;
public sealed record ListProdutosQuery(string? Nome, string? Codigo, decimal? ValorMin, decimal? ValorMax)
  : IRequest<List<ProdutoVm>>;

internal sealed class GetProdutoByIdHandler(IAppDb db) : IRequestHandler<GetProdutoByIdQuery, ProdutoVm>
{
    public async Task<ProdutoVm> Handle(GetProdutoByIdQuery request, CancellationToken ct)
    {
        var p = await db.Produtos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id, ct)
          ?? throw new KeyNotFoundException("Produto não encontrado");
        return p.ToVm();
    }
}

internal sealed class ListProdutosHandler(IAppDb db) : IRequestHandler<ListProdutosQuery, List<ProdutoVm>>
{
    public async Task<List<ProdutoVm>> Handle(ListProdutosQuery request, CancellationToken ct)
    {
        var q = db.Produtos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Nome))
            q = q.Where(p => p.Nome.Contains(request.Nome));
        if (!string.IsNullOrWhiteSpace(request.Codigo))
            q = q.Where(p => p.Codigo.Contains(request.Codigo));

        if (request.ValorMin is not null) q = q.Where(p => p.Valor >= request.ValorMin);
        if (request.ValorMax is not null) q = q.Where(p => p.Valor <= request.ValorMax);

        return await q
          .OrderBy(p => p.Nome)
          .Select(p => p.ToVm())
          .ToListAsync(ct);
    }
}
