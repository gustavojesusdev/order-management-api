using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagementApi.Models;

namespace OrderManagementApi.Data.Configurations;

public sealed class PagamentoConfiguration :
    IEntityTypeConfiguration<Pagamento>
{
    public void Configure(
        EntityTypeBuilder<Pagamento> builder
    )
    {
        builder.ToTable(
            "Pagamentos",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_Pagamentos_Tipo_Valido",
                    "[Tipo] IN ('Pix', 'Cartao')"
                );

                tableBuilder.HasCheckConstraint(
                    "CK_Pagamentos_ValorBase_Positivo",
                    "[ValorBase] > 0"
                );

                tableBuilder.HasCheckConstraint(
                    "CK_Pagamentos_Taxa_NaoNegativa",
                    "[Taxa] >= 0"
                );

                tableBuilder.HasCheckConstraint(
                    "CK_Pagamentos_ValorFinal_Positivo",
                    "[ValorFinal] > 0"
                );
            }
        );

        builder.HasKey(pagamento => pagamento.Id);

        builder.Property(pagamento => pagamento.Tipo)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(pagamento => pagamento.ValorBase)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(pagamento => pagamento.Taxa)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(pagamento => pagamento.ValorFinal)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(pagamento => pagamento.PagoEm)
            .IsRequired();

        builder.HasIndex(pagamento => pagamento.PedidoId)
            .IsUnique();
    }
}
