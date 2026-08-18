using OrderManagementApi.Enums;
using OrderManagementApi.Models;

namespace OrderManagementApi.Repositories;

public interface IPedidoRepository
{
    Task<IReadOnlyList<Pedido>> ObterTodosAsync(
        int? clienteId = null,
        StatusPedido? status = null,
        CancellationToken cancellationToken = default
    );

    Task<Pedido?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default
    );

    Task<Pedido?> ObterPorIdParaAlteracaoAsync(
        int id,
        CancellationToken cancellationToken = default
    );

    Task<Pedido> AdicionarAsync(
        Pedido pedido,
        CancellationToken cancellationToken = default
    );

    Task SalvarAlteracoesAsync(
        CancellationToken cancellationToken = default
    );
}
