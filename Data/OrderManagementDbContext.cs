using Microsoft.EntityFrameworkCore;
using OrderManagementApi.Models;

namespace OrderManagementApi.Data;

public class OrderManagementDbContext : DbContext
{
    public OrderManagementDbContext(
        DbContextOptions<OrderManagementDbContext>
            options
    ) : base(options)
    {
    }

    public DbSet<Produto> Produtos =>
        Set<Produto>();

    public DbSet<Cliente> Clientes =>
        Set<Cliente>();

    public DbSet<Pedido> Pedidos =>
        Set<Pedido>();

    public DbSet<ItemPedido> ItensPedido =>
        Set<ItemPedido>();

    public DbSet<Pagamento> Pagamentos =>
        Set<Pagamento>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder
    )
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(OrderManagementDbContext).Assembly
        );
    }
}
