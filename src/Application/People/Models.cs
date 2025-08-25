namespace SalesApp.Application.People;

public sealed record PessoaVm(long Id, string Nome, string Cpf, string? Endereco);
public sealed record CreatePessoaDto(string Nome, string Cpf, string? Endereco);
public sealed record UpdatePessoaDto(string Nome, string Cpf, string? Endereco);
