using Microsoft.EntityFrameworkCore;
using OrderManagementApi.Data;
using OrderManagementApi.Models;

namespace OrderManagementApi.Repositories;

public class ProdutoRepository : IProdutoRepository
{
    private readonly OrderManagementDbContext _context;

    public ProdutoRepository(OrderManagementDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Produto>> ObterTodosAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Produtos
            .AsNoTracking()
            .OrderBy(produto => produto.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Produto?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Produtos
            .AsNoTracking()
            .FirstOrDefaultAsync(
                produto => produto.Id == id,
                cancellationToken
            );
    }

    public async Task<IReadOnlyList<Produto>> ObterPorIdsAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default
    )
    {
        int[] idsDistintos = ids.Distinct().ToArray();

        if (idsDistintos.Length == 0)
        {
            return Array.Empty<Produto>();
        }

        return await _context.Produtos
            .AsNoTracking()
            .Where(produto => idsDistintos.Contains(produto.Id))
            .OrderBy(produto => produto.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Produto> AdicionarAsync(
        Produto produto,
        CancellationToken cancellationToken = default
    )
    {
        _context.Produtos.Add(produto);

        await _context.SaveChangesAsync(cancellationToken);

        return produto;
    }

    public async Task<bool> AtualizarAsync(
        int id,
        Produto produtoAtualizado,
        CancellationToken cancellationToken = default
    )
    {
        Produto? produtoExistente = await _context.Produtos
            .FirstOrDefaultAsync(
                produto => produto.Id == id,
                cancellationToken
            );

        if (produtoExistente is null)
        {
            return false;
        }

        produtoExistente.Nome = produtoAtualizado.Nome;
        produtoExistente.Preco = produtoAtualizado.Preco;
        produtoExistente.Ativo = produtoAtualizado.Ativo;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ExcluirAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        Produto? produtoExistente = await _context.Produtos
            .FirstOrDefaultAsync(
                produto => produto.Id == id,
                cancellationToken
            );

        if (produtoExistente is null)
        {
            return false;
        }

        _context.Produtos.Remove(produtoExistente);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> PossuiItensPedidoAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.ItensPedido.AnyAsync(
            item => item.ProdutoId == id,
            cancellationToken
        );
    }
}
