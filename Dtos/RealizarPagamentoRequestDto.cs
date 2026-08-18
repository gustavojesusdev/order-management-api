using System.ComponentModel.DataAnnotations;
using OrderManagementApi.Enums;

namespace OrderManagementApi.Dtos;

public class RealizarPagamentoRequestDto
{
    [EnumDataType(
        typeof(TipoPagamento),
        ErrorMessage = "A forma de pagamento informada é inválida."
    )]
    public TipoPagamento Tipo { get; set; }
}
