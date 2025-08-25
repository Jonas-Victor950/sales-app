using MediatR;
using Microsoft.EntityFrameworkCore;
using SalesApp.Application.Orders;
using SalesApp.Domain;

namespace SalesApp.Application;

public sealed record GetPedidoByIdQuery(long Id) : IRequest<PedidoVm>;
public sealed record ListPedidosQuery(PedidoStatus? Status, long? PessoaId) : IRequest<List<PedidoVm>>;

internal sealed class GetPedidoByIdHandler(IAppDb db) : IRequestHandler<GetPedidoByIdQuery, PedidoVm>
{
    public async Task<PedidoVm> Handle(GetPedidoByIdQuery request, CancellationToken ct)
    {
        var p = await db.Pedidos.AsNoTracking()
          .Include("Itens")
          .FirstOrDefaultAsync(x => x.Id == request.Id, ct)
          ?? throw new KeyNotFoundException("Pedido não encontrado");

        var ids = p.Itens.Select(i => i.ProdutoId).Distinct().ToList();
        var nomes = await db.Produtos.Where(x => ids.Contains(x.Id))
          .ToDictionaryAsync(x => x.Id, x => x.Nome, ct);

        return p.ToVm(nomes);
    }
}

internal sealed class ListPedidosHandler(IAppDb db) : IRequestHandler<ListPedidosQuery, List<PedidoVm>>
{
    public async Task<List<PedidoVm>> Handle(ListPedidosQuery request, CancellationToken ct)
    {
        var q = db.Pedidos.AsNoTracking().Include("Itens").AsQueryable();
        if (request.Status is not null) q = q.Where(p => p.Status == request.Status);
        if (request.PessoaId is not null) q = q.Where(p => p.PessoaId == request.PessoaId);

        var list = await q.OrderByDescending(p => p.DataVenda).ToListAsync(ct);

        var idsProdutos = list.SelectMany(p => p.Itens.Select(i => i.ProdutoId)).Distinct().ToList();
        var nomes = await db.Produtos.Where(x => idsProdutos.Contains(x.Id))
          .ToDictionaryAsync(x => x.Id, x => x.Nome, ct);

        return list.Select(p => p.ToVm(nomes)).ToList();
    }
}
