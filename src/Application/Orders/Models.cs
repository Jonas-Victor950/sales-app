using SalesApp.Domain;

namespace SalesApp.Application.Orders;

public sealed record PedidoItemInputDto(long ProdutoId, int Quantidade);

public sealed record CreatePedidoDto(
  long PessoaId,
  FormaPagamento FormaPagamento,
  List<PedidoItemInputDto> Itens
);

public sealed record PedidoItemVm(
  long Id, long ProdutoId, string NomeProduto,
  int Quantidade, decimal ValorUnitario, decimal Subtotal
);

public sealed record PedidoVm(
  long Id, long PessoaId, DateTime DataVenda,
  FormaPagamento FormaPagamento, PedidoStatus Status,
  List<PedidoItemVm> Itens, decimal ValorTotal
);
