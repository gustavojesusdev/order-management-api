using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagementApi.Models;

namespace OrderManagementApi.Data.Configurations;

public sealed class ClienteConfiguration :
    IEntityTypeConfiguration<Cliente>
{
    public void Configure(
        EntityTypeBuilder<Cliente> builder
    )
    {
        builder.ToTable("Clientes");

        builder.HasKey(cliente => cliente.Id);

        builder.Property(cliente => cliente.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(cliente => cliente.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(cliente => cliente.Ativo)
            .IsRequired();

        builder.HasIndex(cliente => cliente.Email)
            .IsUnique();

        builder.HasMany(cliente => cliente.Pedidos)
            .WithOne(pedido => pedido.Cliente)
            .HasForeignKey(pedido => pedido.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
