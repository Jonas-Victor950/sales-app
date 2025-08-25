using SalesApp.Domain;

namespace SalesApp.Application.People;

public static class PeopleMappings
{
  public static PessoaVm ToVm(this Pessoa p) => new(p.Id, p.Nome, p.Cpf, p.Endereco);
}
