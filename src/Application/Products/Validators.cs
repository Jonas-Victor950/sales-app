using FluentValidation;

namespace SalesApp.Application.Products;

public sealed class CreateProdutoDtoValidator : AbstractValidator<CreateProdutoDto>
{
    public CreateProdutoDtoValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Valor).GreaterThanOrEqualTo(0m);
    }
}

public sealed class UpdateProdutoDtoValidator : AbstractValidator<UpdateProdutoDto>
{
    public UpdateProdutoDtoValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Valor).GreaterThanOrEqualTo(0m);
    }
}
