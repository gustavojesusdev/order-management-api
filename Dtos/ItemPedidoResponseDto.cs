namespace OrderManagementApi.Dtos;

public class ItemPedidoResponseDto
{
    public int ProdutoId { get; set; }

    public string NomeProduto { get; set; } = string.Empty;

    public int Quantidade { get; set; }

    public decimal PrecoUnitario { get; set; }

    public decimal Subtotal { get; set; }
}
