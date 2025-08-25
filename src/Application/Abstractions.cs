using SalesApp.Domain;

namespace SalesApp.Application;

public interface IAppDb
{
  IQueryable<Pessoa> Pessoas { get; }
  IQueryable<Produto> Produtos { get; }
  IQueryable<Pedido> Pedidos { get; }

  Task AddAsync<T>(T entity, CancellationToken ct = default) where T : class;
  Task UpdateAsync<T>(T entity, CancellationToken ct = default) where T : class;
  Task DeleteAsync<T>(T entity, CancellationToken ct = default) where T : class;
  Task<int> SaveChangesAsync(CancellationToken ct = default);
}
