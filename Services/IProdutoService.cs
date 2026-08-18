using OrderManagementApi.Dtos;

namespace OrderManagementApi.Services;

public interface IProdutoService
{
    Task<IReadOnlyList<ProdutoResponseDto>> ObterTodosAsync(
        CancellationToken cancellationToken = default
    );

    Task<ProdutoResponseDto?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default
    );

    Task<ProdutoResponseDto> CriarAsync(
        ProdutoRequestDto dto,
        CancellationToken cancellationToken = default
    );

    Task<bool> AtualizarAsync(
        int id,
        ProdutoRequestDto dto,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExcluirAsync(
        int id,
        CancellationToken cancellationToken = default
    );
}
