using SalesApp.Domain;

namespace SalesApp.Application.Orders;

public static class OrderMappings
{
    public static PedidoVm ToVm(this Pedido p, Dictionary<long, string> nomesProdutos)
    {
        var itensVm = p.Itens.Select(i =>
          new PedidoItemVm(
            i.Id, i.ProdutoId,
            nomesProdutos.TryGetValue(i.ProdutoId, out var nome) ? nome : $"Produto {i.ProdutoId}",
            i.Quantidade, i.ValorUnitario, i.Quantidade * i.ValorUnitario
          )).ToList();

        return new PedidoVm(
          p.Id, p.PessoaId, p.DataVenda, p.FormaPagamento, p.Status,
          itensVm, itensVm.Sum(x => x.Subtotal)
        );
    }
}
