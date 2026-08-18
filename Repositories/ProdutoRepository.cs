using Microsoft.EntityFrameworkCore;
using OrderManagementApi.Data;
using OrderManagementApi.Models;

namespace OrderManagementApi.Repositories;

public class ProdutoRepository : IProdutoRepository
{
    private readonly OrderManagementDbContext _context;

    public ProdutoRepository(
        OrderManagementDbContext context
    )
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Produto>>
        ObterTodosAsync()
    {
        return await _context.Produtos
            .AsNoTracking()
            .OrderBy(produto => produto.Id)
            .ToListAsync();
    }

    public async Task<Produto?> ObterPorIdAsync(int id)
    {
        return await _context.Produtos
            .AsNoTracking()
            .FirstOrDefaultAsync(
                produto => produto.Id == id
            );
    }

    public async Task<Produto> AdicionarAsync(
        Produto produto
    )
    {
        _context.Produtos.Add(produto);

        await _context.SaveChangesAsync();

        return produto;
    }

    public async Task<bool> AtualizarAsync(
        int id,
        Produto produtoAtualizado
    )
    {
        Produto? produtoExistente =
            await _context.Produtos.FindAsync(id);

        if (produtoExistente is null)
        {
            return false;
        }

        produtoExistente.Nome =
            produtoAtualizado.Nome;

        produtoExistente.Preco =
            produtoAtualizado.Preco;

        produtoExistente.Ativo =
            produtoAtualizado.Ativo;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        Produto? produtoExistente =
            await _context.Produtos.FindAsync(id);

        if (produtoExistente is null)
        {
            return false;
        }

        _context.Produtos.Remove(produtoExistente);

        await _context.SaveChangesAsync();

        return true;
    }
}