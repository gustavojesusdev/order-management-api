namespace OrderManagementApi.Models;

public class Produto
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public decimal Preco { get; set; }

    public bool Ativo { get; set; } = true;

    public ICollection<ItemPedido> ItensPedido { get; set; } =
        new List<ItemPedido>();
}
