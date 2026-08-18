using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagementApi.Models;

namespace OrderManagementApi.Data.Configurations;

public sealed class ProdutoConfiguration :
    IEntityTypeConfiguration<Produto>
{
    public void Configure(
        EntityTypeBuilder<Produto> builder
    )
    {
        builder.ToTable(
            "Produtos",
            tableBuilder =>
                tableBuilder.HasCheckConstraint(
                    "CK_Produtos_Preco_Positivo",
                    "[Preco] > 0"
                )
        );

        builder.HasKey(produto => produto.Id);

        builder.Property(produto => produto.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(produto => produto.Preco)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(produto => produto.Ativo)
            .IsRequired();

        builder.HasMany(produto => produto.ItensPedido)
            .WithOne(item => item.Produto)
            .HasForeignKey(item => item.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
