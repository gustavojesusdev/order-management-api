using OrderManagementApi.Dtos;

namespace OrderManagementApi.Services;

public interface IClienteService
{
    Task<IReadOnlyList<ClienteResponseDto>> ObterTodosAsync(
        CancellationToken cancellationToken = default
    );

    Task<ClienteResponseDto?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default
    );

    Task<ClienteResponseDto> CriarAsync(
        ClienteRequestDto dto,
        CancellationToken cancellationToken = default
    );

    Task<bool> AtualizarAsync(
        int id,
        ClienteRequestDto dto,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExcluirAsync(
        int id,
        CancellationToken cancellationToken = default
    );
}
