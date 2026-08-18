using OrderManagementApi.Models;

namespace OrderManagementApi.Repositories;

public interface IClienteRepository
{
    Task<IReadOnlyList<Cliente>> ObterTodosAsync(
        CancellationToken cancellationToken = default
    );

    Task<Cliente?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default
    );

    Task<bool> EmailExisteAsync(
        string email,
        int? ignorarClienteId = null,
        CancellationToken cancellationToken = default
    );

    Task<Cliente> AdicionarAsync(
        Cliente cliente,
        CancellationToken cancellationToken = default
    );

    Task<bool> AtualizarAsync(
        int id,
        Cliente clienteAtualizado,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExcluirAsync(
        int id,
        CancellationToken cancellationToken = default
    );

    Task<bool> PossuiPedidosAsync(
        int id,
        CancellationToken cancellationToken = default
    );
}
