using OrderManagementApi.Enums;

namespace OrderManagementApi.Models;

public class Pagamento
{
    public int Id { get; set; }

    public int PedidoId { get; set; }

    public Pedido Pedido { get; set; } = null!;

    public TipoPagamento Tipo { get; set; }

    public decimal ValorBase { get; set; }

    public decimal Taxa { get; set; }

    public decimal ValorFinal { get; set; }

    public DateTime PagoEm { get; set; } =
        DateTime.UtcNow;
}
