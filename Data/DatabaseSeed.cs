using Microsoft.EntityFrameworkCore;
using OrderManagementApi.Models;

namespace OrderManagementApi.Data;

public static class DatabaseSeed
{
    public static void Seed(
        DbContext context,
        bool _
    )
    {
        bool existemAlteracoes = false;

        if (!context.Set<Produto>().Any())
        {
            context.Set<Produto>().AddRange(
                CriarProdutosIniciais()
            );

            existemAlteracoes = true;
        }

        if (!context.Set<Cliente>().Any())
        {
            context.Set<Cliente>().AddRange(
                CriarClientesIniciais()
            );

            existemAlteracoes = true;
        }

        if (existemAlteracoes)
        {
            context.SaveChanges();
        }
    }

    public static async Task SeedAsync(
        DbContext context,
        bool _,
        CancellationToken cancellationToken
    )
    {
        bool existemAlteracoes = false;

        bool existemProdutos =
            await context.Set<Produto>().AnyAsync(
                cancellationToken
            );

        if (!existemProdutos)
        {
            context.Set<Produto>().AddRange(
                CriarProdutosIniciais()
            );

            existemAlteracoes = true;
        }

        bool existemClientes =
            await context.Set<Cliente>().AnyAsync(
                cancellationToken
            );

        if (!existemClientes)
        {
            context.Set<Cliente>().AddRange(
                CriarClientesIniciais()
            );

            existemAlteracoes = true;
        }

        if (existemAlteracoes)
        {
            await context.SaveChangesAsync(
                cancellationToken
            );
        }
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

    private static Cliente[]
        CriarClientesIniciais()
    {
        return new[]
        {
            new Cliente
            {
                Nome = "Ana Souza",
                Email = "ana.souza@example.com",
                Ativo = true
            },
            new Cliente
            {
                Nome = "Carlos Lima",
                Email = "carlos.lima@example.com",
                Ativo = true
            }
        };
    }
}
