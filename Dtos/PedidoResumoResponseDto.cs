using OrderManagementApi.Enums;

namespace OrderManagementApi.Dtos;

public class PedidoResumoResponseDto
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    public string NomeCliente { get; set; } = string.Empty;

    public StatusPedido Status { get; set; }

    public DateTime CriadoEm { get; set; }

    public decimal Subtotal { get; set; }

    public decimal? ValorFinal { get; set; }
}
