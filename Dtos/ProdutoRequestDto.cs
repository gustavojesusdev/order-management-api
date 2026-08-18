using System.ComponentModel.DataAnnotations;

namespace OrderManagementApi.Dtos;

public class ProdutoRequestDto
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(
        100,
        MinimumLength = 2,
        ErrorMessage = "O nome deve ter entre 2 e 100 caracteres."
    )]
    public string Nome { get; set; } = string.Empty;

    [Range(
        0.01,
        double.MaxValue,
        ErrorMessage = "O preço deve ser maior que zero."
    )]
    public decimal Preco { get; set; }

    public bool Ativo { get; set; }
}