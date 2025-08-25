namespace SalesApp.Application.Products;

public sealed record ProdutoVm(long Id, string Nome, string Codigo, decimal Valor);
public sealed record CreateProdutoDto(string Nome, string Codigo, decimal Valor);
public sealed record UpdateProdutoDto(string Nome, string Codigo, decimal Valor);
