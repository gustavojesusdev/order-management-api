using OrderManagementApi.Models;

namespace OrderManagementApi.Repositories;

public interface IProdutoRepository
{
    Task<IReadOnlyList<Produto>> ObterTodosAsync();

    Task<Produto?> ObterPorIdAsync(int id);

    Task<Produto> AdicionarAsync(Produto produto);

    Task<bool> AtualizarAsync(
        int id,
        Produto produtoAtualizado
    );

    Task<bool> ExcluirAsync(int id);
}