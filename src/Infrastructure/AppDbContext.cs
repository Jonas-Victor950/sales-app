using Microsoft.EntityFrameworkCore;
using SalesApp.Application;
using SalesApp.Domain;

namespace SalesApp.Infrastructure;

public sealed class AppDbContext(DbContextOptions<AppDbContext> opt) 
  : DbContext(opt), IAppDb
{
  public DbSet<Pessoa> Pessoas => Set<Pessoa>();
  public DbSet<Produto> Produtos => Set<Produto>();
  public DbSet<Pedido> Pedidos => Set<Pedido>();
  public DbSet<PedidoItem> PedidoItens => Set<PedidoItem>();

  IQueryable<Pessoa> IAppDb.Pessoas => Pessoas.AsQueryable();
  IQueryable<Produto> IAppDb.Produtos => Produtos.AsQueryable();
  IQueryable<Pedido> IAppDb.Pedidos => Pedidos.AsQueryable();

  public async Task AddAsync<T>(T entity, CancellationToken ct) where T : class
    => await Set<T>().AddAsync(entity, ct);

  public Task UpdateAsync<T>(T entity, CancellationToken ct) where T : class
  { Update(entity); return Task.CompletedTask; }

  public Task DeleteAsync<T>(T entity, CancellationToken ct) where T : class
  { Remove(entity); return Task.CompletedTask; }

  protected override void OnModelCreating(ModelBuilder b)
  {
    b.Entity<Pessoa>(e =>
    {
      e.ToTable("pessoas");
      e.HasKey(x => x.Id);
      e.Property(x => x.Nome).HasMaxLength(150).IsRequired();
      e.Property(x => x.Cpf).HasMaxLength(11).IsRequired();
      e.HasIndex(x => x.Cpf).IsUnique();
    });

    b.Entity<Produto>(e =>
    {
      e.ToTable("produtos");
      e.HasKey(x => x.Id);
      e.Property(x => x.Nome).HasMaxLength(150).IsRequired();
      e.Property(x => x.Codigo).HasMaxLength(50).IsRequired();
      e.HasIndex(x => x.Codigo).IsUnique();
      e.Property(x => x.Valor).HasColumnType("decimal(18,2)").IsRequired();
    });

    b.Entity<Pedido>(e =>
    {
      e.ToTable("pedidos");
      e.HasKey(x => x.Id);
      e.Property(x => x.DataVenda).IsRequired();
      e.Property(x => x.Status).HasConversion<int>().IsRequired();
      e.Property(x => x.FormaPagamento).HasConversion<int>().IsRequired();
      e.Ignore(x => x.ValorTotal);
      e.HasMany<PedidoItem>("Itens").WithOne().HasForeignKey("PedidoId").OnDelete(DeleteBehavior.Cascade);
      e.Property<long>("PessoaId");
    });

    b.Entity<PedidoItem>(e =>
    {
      e.ToTable("pedido_itens");
      e.HasKey(x => x.Id);
      e.Property(x => x.Quantidade).IsRequired();
      e.Property(x => x.ValorUnitario).HasColumnType("decimal(18,2)").IsRequired();
      e.Ignore(x => x.Subtotal);
      e.Property<long>("PedidoId");
      e.Property<long>("ProdutoId").IsRequired();
    });
  }

  public Task<int> SaveChangesAsync(CancellationToken ct = default) 
    => base.SaveChangesAsync(ct);
}
