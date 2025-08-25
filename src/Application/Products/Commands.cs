using MediatR;
using Microsoft.EntityFrameworkCore;
using SalesApp.Application.Products;
using SalesApp.Domain;

namespace SalesApp.Application;

public sealed record CreateProdutoCommand(CreateProdutoDto Dto) : IRequest<ProdutoVm>;
public sealed record UpdateProdutoCommand(long Id, UpdateProdutoDto Dto) : IRequest<ProdutoVm>;
public sealed record DeleteProdutoCommand(long Id) : IRequest<Unit>;

internal sealed class CreateProdutoHandler(IAppDb db) : IRequestHandler<CreateProdutoCommand, ProdutoVm>
{
    public async Task<ProdutoVm> Handle(CreateProdutoCommand request, CancellationToken ct)
    {
        var dto = request.Dto;

        var exists = await db.Produtos.AnyAsync(p => p.Codigo == dto.Codigo, ct);
        if (exists) throw new InvalidOperationException("Código já cadastrado");

        var entity = new Produto { Nome = dto.Nome.Trim(), Codigo = dto.Codigo.Trim(), Valor = dto.Valor };
        await db.AddAsync(entity, ct);
        await db.SaveChangesAsync(ct);
        return entity.ToVm();
    }
}

internal sealed class UpdateProdutoHandler(IAppDb db) : IRequestHandler<UpdateProdutoCommand, ProdutoVm>
{
    public async Task<ProdutoVm> Handle(UpdateProdutoCommand request, CancellationToken ct)
    {
        var p = await db.Produtos.FirstOrDefaultAsync(x => x.Id == request.Id, ct)
          ?? throw new KeyNotFoundException("Produto não encontrado");

        if (!string.Equals(p.Codigo, request.Dto.Codigo, StringComparison.Ordinal))
        {
            var dup = await db.Produtos.AnyAsync(x => x.Codigo == request.Dto.Codigo && x.Id != p.Id, ct);
            if (dup) throw new InvalidOperationException("Código já cadastrado");
        }

        p.Nome = request.Dto.Nome.Trim();
        p.Codigo = request.Dto.Codigo.Trim();
        p.Valor = request.Dto.Valor;

        await db.UpdateAsync(p, ct);
        await db.SaveChangesAsync(ct);
        return p.ToVm();
    }
}

internal sealed class DeleteProdutoHandler(IAppDb db) : IRequestHandler<DeleteProdutoCommand, Unit>
{
    public async Task<Unit> Handle(DeleteProdutoCommand request, CancellationToken ct)
    {
        var p = await db.Produtos.FirstOrDefaultAsync(x => x.Id == request.Id, ct)
          ?? throw new KeyNotFoundException("Produto não encontrado");
        await db.DeleteAsync(p, ct);
        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
