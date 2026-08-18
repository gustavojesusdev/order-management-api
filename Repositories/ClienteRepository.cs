using Microsoft.EntityFrameworkCore;
using OrderManagementApi.Data;
using OrderManagementApi.Models;

namespace OrderManagementApi.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly OrderManagementDbContext _context;

    public ClienteRepository(OrderManagementDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Cliente>> ObterTodosAsync(
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Clientes
            .AsNoTracking()
            .OrderBy(cliente => cliente.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Cliente?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(
                cliente => cliente.Id == id,
                cancellationToken
            );
    }

    public async Task<bool> EmailExisteAsync(
        string email,
        int? ignorarClienteId = null,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Clientes
            .AsNoTracking()
            .AnyAsync(
                cliente =>
                    cliente.Email == email &&
                    (!ignorarClienteId.HasValue ||
                     cliente.Id != ignorarClienteId.Value),
                cancellationToken
            );
    }

    public async Task<Cliente> AdicionarAsync(
        Cliente cliente,
        CancellationToken cancellationToken = default
    )
    {
        _context.Clientes.Add(cliente);

        await _context.SaveChangesAsync(cancellationToken);

        return cliente;
    }

    public async Task<bool> AtualizarAsync(
        int id,
        Cliente clienteAtualizado,
        CancellationToken cancellationToken = default
    )
    {
        Cliente? clienteExistente = await _context.Clientes
            .FirstOrDefaultAsync(
                cliente => cliente.Id == id,
                cancellationToken
            );

        if (clienteExistente is null)
        {
            return false;
        }

        clienteExistente.Nome = clienteAtualizado.Nome;
        clienteExistente.Email = clienteAtualizado.Email;
        clienteExistente.Ativo = clienteAtualizado.Ativo;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> ExcluirAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        Cliente? clienteExistente = await _context.Clientes
            .FirstOrDefaultAsync(
                cliente => cliente.Id == id,
                cancellationToken
            );

        if (clienteExistente is null)
        {
            return false;
        }

        _context.Clientes.Remove(clienteExistente);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> PossuiPedidosAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        return await _context.Pedidos.AnyAsync(
            pedido => pedido.ClienteId == id,
            cancellationToken
        );
    }
}
