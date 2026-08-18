using OrderManagementApi.Dtos;
using OrderManagementApi.Models;

namespace OrderManagementApi.Services;

public interface IProdutoService
{
    Task<IReadOnlyList<Produto>> ObterTodosAsync();

    Task<Produto?> ObterPorIdAsync(int id);

    Task<Produto> CriarAsync(ProdutoRequestDto dto);

    Task<bool> AtualizarAsync(
        int id,
        ProdutoRequestDto dto
    );

    Task<bool> ExcluirAsync(int id);
}