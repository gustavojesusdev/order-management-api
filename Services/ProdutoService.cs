using OrderManagementApi.Dtos;
using OrderManagementApi.Models;
using OrderManagementApi.Repositories;

namespace OrderManagementApi.Services;

public class ProdutoService : IProdutoService
{
    private readonly IProdutoRepository _repository;

    public ProdutoService(IProdutoRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ProdutoResponseDto>> ObterTodosAsync(
        CancellationToken cancellationToken = default
    )
    {
        IReadOnlyList<Produto> produtos =
            await _repository.ObterTodosAsync(cancellationToken);

        return produtos.Select(MapearResposta).ToList();
    }

    public async Task<ProdutoResponseDto?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        Produto? produto = await _repository.ObterPorIdAsync(
            id,
            cancellationToken
        );

        return produto is null ? null : MapearResposta(produto);
    }

    public async Task<ProdutoResponseDto> CriarAsync(
        ProdutoRequestDto dto,
        CancellationToken cancellationToken = default
    )
    {
        Produto produto = new()
        {
            Nome = dto.Nome.Trim(),
            Preco = ArredondarPreco(dto.Preco),
            Ativo = dto.Ativo
        };

        Produto produtoCriado = await _repository.AdicionarAsync(
            produto,
            cancellationToken
        );

        return MapearResposta(produtoCriado);
    }

    public async Task<bool> AtualizarAsync(
        int id,
        ProdutoRequestDto dto,
        CancellationToken cancellationToken = default
    )
    {
        Produto produtoAtualizado = new()
        {
            Id = id,
            Nome = dto.Nome.Trim(),
            Preco = ArredondarPreco(dto.Preco),
            Ativo = dto.Ativo
        };

        return await _repository.AtualizarAsync(
            id,
            produtoAtualizado,
            cancellationToken
        );
    }

    public async Task<bool> ExcluirAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        if (await _repository.PossuiItensPedidoAsync(id, cancellationToken))
        {
            throw new InvalidOperationException(
                "Não é possível excluir um produto que pertence a um pedido. " +
                "Desative-o para impedir novas vendas sem apagar o histórico."
            );
        }

        return await _repository.ExcluirAsync(id, cancellationToken);
    }

    private static ProdutoResponseDto MapearResposta(Produto produto)
    {
        return new ProdutoResponseDto
        {
            Id = produto.Id,
            Nome = produto.Nome,
            Preco = produto.Preco,
            Ativo = produto.Ativo
        };
    }

    private static decimal ArredondarPreco(decimal preco)
    {
        return decimal.Round(
            preco,
            2,
            MidpointRounding.AwayFromZero
        );
    }
}
