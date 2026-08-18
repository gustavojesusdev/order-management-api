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
        typeof(decimal),
        "0.01",
        "99999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true,
        ErrorMessage =
            "O preço deve estar entre 0,01 e 99.999.999,99."
    )]
    public decimal Preco { get; set; }

    public bool Ativo { get; set; } = true;
}
