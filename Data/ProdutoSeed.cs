using Microsoft.EntityFrameworkCore;
using OrderManagementApi.Models;

namespace OrderManagementApi.Data;

public static class ProdutoSeed
{
    public static void Seed(
        DbContext context,
        bool _
    )
    {
        if (context.Set<Produto>().Any())
        {
            return;
        }

        context.Set<Produto>().AddRange(
            CriarProdutosIniciais()
        );

        context.SaveChanges();
    }

    public static async Task SeedAsync(
        DbContext context,
        bool _,
        CancellationToken cancellationToken
    )
    {
        bool existemProdutos =
            await context.Set<Produto>().AnyAsync(
                cancellationToken
            );

        if (existemProdutos)
        {
            return;
        }

        context.Set<Produto>().AddRange(
            CriarProdutosIniciais()
        );

        await context.SaveChangesAsync(
            cancellationToken
        );
    }

    private static Produto[]
        CriarProdutosIniciais()
    {
        return new[]
        {
            new Produto
            {
                Nome = "Notebook",
                Preco = 3500.00m,
                Ativo = true
            },
            new Produto
            {
                Nome = "Mouse",
                Preco = 120.00m,
                Ativo = true
            },
            new Produto
            {
                Nome = "Teclado",
                Preco = 250.00m,
                Ativo = true
            },
            new Produto
            {
                Nome = "Monitor",
                Preco = 1200.00m,
                Ativo = false
            }
        };
    }
}