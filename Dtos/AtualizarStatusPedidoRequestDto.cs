using System.ComponentModel.DataAnnotations;
using OrderManagementApi.Enums;

namespace OrderManagementApi.Dtos;

public class AtualizarStatusPedidoRequestDto
{
    [EnumDataType(
        typeof(StatusPedido),
        ErrorMessage = "O status informado é inválido."
    )]
    public StatusPedido Status { get; set; }
}
