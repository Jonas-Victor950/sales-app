using FluentValidation;

namespace SalesApp.Application.Orders;

public sealed class CreatePedidoDtoValidator : AbstractValidator<CreatePedidoDto>
{
    public CreatePedidoDtoValidator()
    {
        RuleFor(x => x.PessoaId).GreaterThan(0);
        RuleFor(x => x.Itens).NotEmpty().WithMessage("Pedido precisa de ao menos 1 item");
        RuleForEach(x => x.Itens).ChildRules(child =>
        {
            child.RuleFor(i => i.ProdutoId).GreaterThan(0);
            child.RuleFor(i => i.Quantidade).GreaterThan(0);
        });
    }
}
