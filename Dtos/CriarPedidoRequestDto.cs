using System.ComponentModel.DataAnnotations;

namespace OrderManagementApi.Dtos;

public class CriarPedidoRequestDto
{
    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "O identificador do cliente deve ser maior que zero."
    )]
    public int ClienteId { get; set; }

    [Required(ErrorMessage = "Informe os itens do pedido.")]
    [MinLength(
        1,
        ErrorMessage = "O pedido deve possuir pelo menos um item."
    )]
    public List<ItemPedidoRequestDto> Itens { get; set; } =
        new();
}
