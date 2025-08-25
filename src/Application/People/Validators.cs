using FluentValidation;
using SalesApp.Application.Common;

namespace SalesApp.Application.People;

public sealed class CreatePessoaDtoValidator : AbstractValidator<CreatePessoaDto>
{
  public CreatePessoaDtoValidator()
  {
    RuleFor(x => x.Nome).NotEmpty().MaximumLength(150);
    RuleFor(x => x.Cpf)
      .NotEmpty().Must(CpfUtils.IsValid).WithMessage("CPF inválido");
  }
}

public sealed class UpdatePessoaDtoValidator : AbstractValidator<UpdatePessoaDto>
{
  public UpdatePessoaDtoValidator()
  {
    RuleFor(x => x.Nome).NotEmpty().MaximumLength(150);
    RuleFor(x => x.Cpf)
      .NotEmpty().Must(CpfUtils.IsValid).WithMessage("CPF inválido");
  }
}
