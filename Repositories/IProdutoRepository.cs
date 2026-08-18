using OrderManagementApi.Models;

namespace OrderManagementApi.Repositories;

public interface IProdutoRepository
{
    Task<IReadOnlyList<Produto>> ObterTodosAsync(
        CancellationToken cancellationToken = default
    );

    Task<Produto?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<Produto>> ObterPorIdsAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default
    );

    Task<Produto> AdicionarAsync(
        Produto produto,
        CancellationToken cancellationToken = default
    );

    Task<bool> AtualizarAsync(
        int id,
        Produto produtoAtualizado,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExcluirAsync(
        int id,
        CancellationToken cancellationToken = default
    );

    Task<bool> PossuiItensPedidoAsync(
        int id,
        CancellationToken cancellationToken = default
    );
}
