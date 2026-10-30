using OrderManagementApi.Enums;

namespace OrderManagementApi.Models;

public class Pedido
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    public Cliente Cliente { get; set; } = null!;

    public StatusPedido Status { get; set; } =
        StatusPedido.Criado;

    public DateTime CriadoEm { get; set; } =
        DateTime.UtcNow;

    public ICollection<ItemPedido> Itens { get; set; } =
        new List<ItemPedido>();

    public Pagamento? Pagamento { get; set; }
}
