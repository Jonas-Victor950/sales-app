using SalesApp.Domain;

namespace SalesApp.Infrastructure.Seed;

public static class DbInitializer
{
  public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
  {
    if (!db.Pessoas.Any())
    {
      db.Pessoas.AddRange(
        new Pessoa { Nome = "Ana Maria", Cpf = "11144477735", Endereco = "Rua A, 123" },
        new Pessoa { Nome = "Carlos Silva", Cpf = "98765432100", Endereco = "Av. B, 456" }
      );
    }

    if (!db.Produtos.Any())
    {
      db.Produtos.AddRange(
        new Produto { Nome = "Camiseta", Codigo = "CAM-001", Valor = 59.90m },
        new Produto { Nome = "Caneca",   Codigo = "CNC-010", Valor = 29.50m }
      );
    }

    await db.SaveChangesAsync(ct);
  }
}
