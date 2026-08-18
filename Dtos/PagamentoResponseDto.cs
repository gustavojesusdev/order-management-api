using OrderManagementApi.Enums;

namespace OrderManagementApi.Dtos;

public class PagamentoResponseDto
{
    public TipoPagamento Tipo { get; set; }

    public decimal ValorBase { get; set; }

    public decimal Taxa { get; set; }

    public decimal ValorFinal { get; set; }

    public DateTime PagoEm { get; set; }
}
