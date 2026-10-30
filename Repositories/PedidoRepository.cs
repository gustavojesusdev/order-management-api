using Microsoft.EntityFrameworkCore;
using OrderManagementApi.Data;
using OrderManagementApi.Enums;
using OrderManagementApi.Models;

namespace OrderManagementApi.Repositories;

public class PedidoRepository : IPedidoRepository
{
    private readonly OrderManagementDbContext _context;

    public PedidoRepository(
        OrderManagementDbContext context
    )
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Pedido>>
        ObterTodosAsync(
            int? clienteId = null,
            StatusPedido? status = null,
            CancellationToken cancellationToken = default
        )
    {
        IQueryable<Pedido> consulta =
            CriarConsultaDetalhada()
                .AsNoTracking();

        if (clienteId.HasValue)
        {
            consulta = consulta.Where(
                pedido => pedido.ClienteId ==
                    clienteId.Value
            );
        }

        if (status.HasValue)
        {
            consulta = consulta.Where(
                pedido => pedido.Status == status.Value
            );
        }

        return await consulta
            .OrderByDescending(pedido => pedido.CriadoEm)
            .ThenByDescending(pedido => pedido.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Pedido?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        return await CriarConsultaDetalhada()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                pedido => pedido.Id == id,
                cancellationToken
            );
    }

    public async Task<Pedido?>
        ObterPorIdParaAlteracaoAsync(
            int id,
            CancellationToken cancellationToken = default
        )
    {
        return await CriarConsultaDetalhada()
            .FirstOrDefaultAsync(
                pedido => pedido.Id == id,
                cancellationToken
            );
    }

    public async Task<Pedido> AdicionarAsync(
        Pedido pedido,
        CancellationToken cancellationToken = default
    )
    {
        _context.Pedidos.Add(pedido);

        await _context.SaveChangesAsync(
            cancellationToken
        );

        return pedido;
    }

    public async Task SalvarAlteracoesAsync(
        CancellationToken cancellationToken = default
    )
    {
        await _context.SaveChangesAsync(
            cancellationToken
        );
    }

    private IQueryable<Pedido> CriarConsultaDetalhada()
    {
        return _context.Pedidos
            .Include(pedido => pedido.Cliente)
            .Include(pedido => pedido.Itens)
                .ThenInclude(item => item.Produto)
            .Include(pedido => pedido.Pagamento)
            .AsSplitQuery();
    }
}
