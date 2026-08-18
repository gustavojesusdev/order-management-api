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

    protected override void OnModelCreating(
        ModelBuilder modelBuilder
    )
    {
        base.OnModelCreating(modelBuilder);

        var produto =
            modelBuilder.Entity<Produto>();

        produto.HasKey(produto => produto.Id);

        produto.Property(produto => produto.Nome)
            .IsRequired()
            .HasMaxLength(100);

        produto.Property(produto => produto.Preco)
            .IsRequired()
            .HasPrecision(10, 2);

        produto.Property(produto => produto.Ativo)
            .IsRequired();

        produto.ToTable(table =>
            table.HasCheckConstraint(
                "CK_Produtos_Preco_Positivo",
                "[Preco] > 0"
            )
        );
    }
}