using OrderManagementApi.Dtos;
using OrderManagementApi.Enums;

namespace OrderManagementApi.Services;

public interface IPedidoService
{
    Task<IReadOnlyList<PedidoResumoResponseDto>>
        ObterTodosAsync(
            int? clienteId = null,
            StatusPedido? status = null,
            CancellationToken cancellationToken = default
        );

    Task<PedidoDetalheResponseDto?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default
    );

    Task<PedidoDetalheResponseDto> CriarAsync(
        CriarPedidoRequestDto dto,
        CancellationToken cancellationToken = default
    );

    Task<PedidoDetalheResponseDto> RealizarPagamentoAsync(
        int id,
        RealizarPagamentoRequestDto dto,
        CancellationToken cancellationToken = default
    );

    Task<PedidoDetalheResponseDto> AtualizarStatusAsync(
        int id,
        AtualizarStatusPedidoRequestDto dto,
        CancellationToken cancellationToken = default
    );
}
