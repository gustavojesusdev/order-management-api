using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagementApi.Models;

namespace OrderManagementApi.Data.Configurations;

public sealed class PedidoConfiguration :
    IEntityTypeConfiguration<Pedido>
{
    public void Configure(
        EntityTypeBuilder<Pedido> builder
    )
    {
        builder.ToTable(
            "Pedidos",
            tableBuilder =>
                tableBuilder.HasCheckConstraint(
                    "CK_Pedidos_Status_Valido",
                    "[Status] IN ('Criado', " +
                    "'AguardandoPagamento', 'Pago', " +
                    "'Enviado', 'Entregue', 'Cancelado')"
                )
        );

        builder.HasKey(pedido => pedido.Id);

        builder.Property(pedido => pedido.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(pedido => pedido.CriadoEm)
            .IsRequired();

        builder.HasMany(pedido => pedido.Itens)
            .WithOne(item => item.Pedido)
            .HasForeignKey(item => item.PedidoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pedido => pedido.Pagamento)
            .WithOne(pagamento => pagamento.Pedido)
            .HasForeignKey<Pagamento>(
                pagamento => pagamento.PedidoId
            )
            .OnDelete(DeleteBehavior.Restrict);
    }
}
