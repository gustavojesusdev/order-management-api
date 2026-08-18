using OrderManagementApi.Enums;

namespace OrderManagementApi.Dtos;

public class PedidoDetalheResponseDto
{
    public int Id { get; set; }

    public ClienteResponseDto Cliente { get; set; } = new();

    public StatusPedido Status { get; set; }

    public DateTime CriadoEm { get; set; }

    public List<ItemPedidoResponseDto> Itens { get; set; } =
        new();

    public decimal Subtotal { get; set; }

    public PagamentoResponseDto? Pagamento { get; set; }
}
