using MediatR;
using Microsoft.EntityFrameworkCore;
using SalesApp.Application.Orders;
using SalesApp.Domain;

namespace SalesApp.Application;

public sealed record CreatePedidoCommand(CreatePedidoDto Dto) : IRequest<PedidoVm>;
public sealed record MarcarPedidoPagoCommand(long Id) : IRequest<PedidoVm>;
public sealed record MarcarPedidoEnviadoCommand(long Id) : IRequest<PedidoVm>;
public sealed record MarcarPedidoRecebidoCommand(long Id) : IRequest<PedidoVm>;

internal sealed class CreatePedidoHandler(IAppDb db) : IRequestHandler<CreatePedidoCommand, PedidoVm>
{
    public async Task<PedidoVm> Handle(CreatePedidoCommand request, CancellationToken ct)
    {
        // pessoa existe?
        var pessoaExists = await db.Pessoas.AnyAsync(p => p.Id == request.Dto.PessoaId, ct);
        if (!pessoaExists) throw new KeyNotFoundException("Pessoa não encontrada");

        // normaliza itens (agrupar produto, somar quantidades)
        var itensAgregados = request.Dto.Itens
          .GroupBy(i => i.ProdutoId)
          .Select(g => new { ProdutoId = g.Key, Quantidade = g.Sum(x => x.Quantidade) })
          .ToList();

        // carrega preços atuais dos produtos
        var produtoIds = itensAgregados.Select(x => x.ProdutoId).ToList();
        var produtos = await db.Produtos
          .Where(p => produtoIds.Contains(p.Id))
          .ToDictionaryAsync(p => p.Id, p => new { p.Valor, p.Nome }, ct);

        if (produtos.Count != produtoIds.Count)
            throw new KeyNotFoundException("Um ou mais produtos não foram encontrados");

        // monta pedido finalizado (snapshot de ValorUnitario)
        var pedido = new Pedido
        {
            PessoaId = request.Dto.PessoaId,
            FormaPagamento = request.Dto.FormaPagamento,
            Status = PedidoStatus.Pendente,
            DataVenda = DateTime.UtcNow,
            Itens = itensAgregados.Select(x => new PedidoItem
            {
                ProdutoId = x.ProdutoId,
                Quantidade = x.Quantidade,
                ValorUnitario = produtos[x.ProdutoId].Valor
            }).ToList()
        };

        await db.AddAsync(pedido, ct);
        await db.SaveChangesAsync(ct);

        // montar VM com nomes dos produtos
        var nomes = produtos.ToDictionary(k => k.Key, v => v.Value.Nome);
        return pedido.ToVm(nomes);
    }
}

internal static class PedidoStatusRules
{
    public static void MarcarPago(Pedido p)
    {
        if (p.Status != PedidoStatus.Pendente)
            throw new InvalidOperationException("Somente pedidos pendentes podem ser marcados como pagos");
        p.Status = PedidoStatus.Pago;
    }

    public static void MarcarEnviado(Pedido p)
    {
        if (p.Status != PedidoStatus.Pago)
            throw new InvalidOperationException("Somente pedidos pagos podem ser enviados");
        p.Status = PedidoStatus.Enviado;
    }

    public static void MarcarRecebido(Pedido p)
    {
        if (p.Status != PedidoStatus.Enviado)
            throw new InvalidOperationException("Somente pedidos enviados podem ser recebidos");
        p.Status = PedidoStatus.Recebido;
    }
}

internal abstract class StatusCommandHandlerBase(IAppDb db)
{
    protected async Task<Pedido> LoadPedido(long id, CancellationToken ct)
      => await db.Pedidos.Include("Itens").FirstOrDefaultAsync(p => p.Id == id, ct)
         ?? throw new KeyNotFoundException("Pedido não encontrado");

    protected async Task<PedidoVm> PersistAndVm(Pedido p, IAppDb db, CancellationToken ct)
    {
        await db.SaveChangesAsync(ct);

        // buscar nomes dos produtos para VM
        var ids = p.Itens.Select(i => i.ProdutoId).Distinct().ToList();
        var nomes = await db.Produtos.Where(x => ids.Contains(x.Id))
          .ToDictionaryAsync(x => x.Id, x => x.Nome, ct);

        return p.ToVm(nomes);
    }
}

internal sealed class MarcarPedidoPagoHandler(IAppDb db)
  : StatusCommandHandlerBase(db), IRequestHandler<MarcarPedidoPagoCommand, PedidoVm>
{
    public async Task<PedidoVm> Handle(MarcarPedidoPagoCommand request, CancellationToken ct)
    {
        var p = await LoadPedido(request.Id, ct);
        PedidoStatusRules.MarcarPago(p);
        return await PersistAndVm(p, db, ct);
    }
}

internal sealed class MarcarPedidoEnviadoHandler(IAppDb db)
  : StatusCommandHandlerBase(db), IRequestHandler<MarcarPedidoEnviadoCommand, PedidoVm>
{
    public async Task<PedidoVm> Handle(MarcarPedidoEnviadoCommand request, CancellationToken ct)
    {
        var p = await LoadPedido(request.Id, ct);
        PedidoStatusRules.MarcarEnviado(p);
        return await PersistAndVm(p, db, ct);
    }
}

internal sealed class MarcarPedidoRecebidoHandler(IAppDb db)
  : StatusCommandHandlerBase(db), IRequestHandler<MarcarPedidoRecebidoCommand, PedidoVm>
{
    public async Task<PedidoVm> Handle(MarcarPedidoRecebidoCommand request, CancellationToken ct)
    {
        var p = await LoadPedido(request.Id, ct);
        PedidoStatusRules.MarcarRecebido(p);
        return await PersistAndVm(p, db, ct);
    }
}
