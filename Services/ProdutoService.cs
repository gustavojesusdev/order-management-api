using OrderManagementApi.Dtos;
using OrderManagementApi.Models;
using OrderManagementApi.Repositories;

namespace OrderManagementApi.Services;

public class ProdutoService : IProdutoService
{
    private readonly IProdutoRepository _repository;

    public ProdutoService(
        IProdutoRepository repository
    )
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<Produto>>
        ObterTodosAsync()
    {
        return await _repository.ObterTodosAsync();
    }

    public async Task<Produto?> ObterPorIdAsync(
        int id
    )
    {
        return await _repository.ObterPorIdAsync(id);
    }

    public async Task<Produto> CriarAsync(
        ProdutoRequestDto dto
    )
    {
        Produto produto = new()
        {
            Nome = dto.Nome.Trim(),
            Preco = dto.Preco,
            Ativo = dto.Ativo
        };

        return await _repository.AdicionarAsync(
            produto
        );
    }

    public async Task<bool> AtualizarAsync(
        int id,
        ProdutoRequestDto dto
    )
    {
        Produto produtoAtualizado = new()
        {
            Id = id,
            Nome = dto.Nome.Trim(),
            Preco = dto.Preco,
            Ativo = dto.Ativo
        };

        return await _repository.AtualizarAsync(
            id,
            produtoAtualizado
        );
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        return await _repository.ExcluirAsync(id);
    }
}