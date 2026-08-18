using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagementApi.Models;

namespace OrderManagementApi.Data.Configurations;

public sealed class ItemPedidoConfiguration :
    IEntityTypeConfiguration<ItemPedido>
{
    public void Configure(
        EntityTypeBuilder<ItemPedido> builder
    )
    {
        builder.ToTable(
            "ItensPedido",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_ItensPedido_Quantidade_Positiva",
                    "[Quantidade] > 0"
                );

                tableBuilder.HasCheckConstraint(
                    "CK_ItensPedido_PrecoUnitario_Positivo",
                    "[PrecoUnitario] > 0"
                );
            }
        );

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Quantidade)
            .IsRequired();

        builder.Property(item => item.PrecoUnitario)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Ignore(item => item.Subtotal);

        builder.HasIndex(
            item => new
            {
                item.PedidoId,
                item.ProdutoId
            }
        ).IsUnique();
    }
}
