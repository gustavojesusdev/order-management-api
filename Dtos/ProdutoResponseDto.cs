namespace OrderManagementApi.Dtos;

public class ProdutoResponseDto
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public decimal Preco { get; set; }

    public bool Ativo { get; set; }
}
