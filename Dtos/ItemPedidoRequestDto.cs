using System.ComponentModel.DataAnnotations;

namespace OrderManagementApi.Dtos;

public class ItemPedidoRequestDto
{
    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "O identificador do produto deve ser maior que zero."
    )]
    public int ProdutoId { get; set; }

    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "A quantidade deve ser maior que zero."
    )]
    public int Quantidade { get; set; }
}
