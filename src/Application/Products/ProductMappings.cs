using SalesApp.Domain;

namespace SalesApp.Application.Products;

public static class ProductMappings
{
  public static ProdutoVm ToVm(this Produto p) => new(p.Id, p.Nome, p.Codigo, p.Valor);
}
