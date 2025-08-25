namespace SalesApp.Domain;

public enum PedidoStatus { Pendente = 0, Pago = 1, Enviado = 2, Recebido = 3 }
public enum FormaPagamento { Dinheiro = 0, Cartao = 1, Boleto = 2 }

public sealed class Pessoa
{
    public long Id { get; set; }
    public string Nome { get; set; } = "";
    public string Cpf { get; set; } = "";
    public string? Endereco { get; set; }
}

public sealed class Produto
{
    public long Id { get; set; }
    public string Nome { get; set; } = "";
    public string Codigo { get; set; } = "";
    public decimal Valor { get; set; }
}

public sealed class PedidoItem
{
    public long Id { get; set; }
    public long ProdutoId { get; set; }
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal Subtotal => Quantidade * ValorUnitario;
}

public sealed class Pedido
{
    public long Id { get; set; }
    public long PessoaId { get; set; }
    public DateTime DataVenda { get; set; } = DateTime.UtcNow;
    public FormaPagamento FormaPagamento { get; set; } = FormaPagamento.Dinheiro;
    public PedidoStatus Status { get; set; } = PedidoStatus.Pendente;
    public List<PedidoItem> Itens { get; set; } = new();
    public decimal ValorTotal => Itens.Sum(i => i.Subtotal);

    public void MarcarPago()
    {
        if (Status != PedidoStatus.Pendente) return;
        Status = PedidoStatus.Pago;
    }
    public void MarcarEnviado()
    {
        if (Status == PedidoStatus.Pago) Status = PedidoStatus.Enviado;
    }
    public void MarcarRecebido()
    {
        if (Status == PedidoStatus.Enviado) Status = PedidoStatus.Recebido;
    }
}