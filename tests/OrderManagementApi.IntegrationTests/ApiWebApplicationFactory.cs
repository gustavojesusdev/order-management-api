using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using OrderManagementApi.Data;

namespace OrderManagementApi.IntegrationTests;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName =
        $"OrderManagementApiTests_{Guid.NewGuid():N}";

    private readonly string _connectionString;
    private bool _databaseInitialized;

    public ApiWebApplicationFactory()
    {
        _connectionString = new SqlConnectionStringBuilder
        {
            DataSource = @"(localdb)\MSSQLLocalDB",
            InitialCatalog = _databaseName,
            IntegratedSecurity = true,
            TrustServerCertificate = true,
            Pooling = false
        }.ConnectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // Substituir a configuracao original pelo banco exclusivo deste teste.
            services.RemoveAll<OrderManagementDbContext>();
            services.RemoveAll<DbContextOptions<OrderManagementDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<OrderManagementDbContext>>();

            services.AddDbContext<OrderManagementDbContext>(options =>
                options.UseSqlServer(_connectionString));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        try
        {
            using var scope = host.Services.CreateScope();
            var context = scope.ServiceProvider
                .GetRequiredService<OrderManagementDbContext>();

            var actualConnection = new SqlConnectionStringBuilder(
                context.Database.GetConnectionString() ?? string.Empty);

            // Confirmar o isolamento antes de executar qualquer migration.
            if (actualConnection.InitialCatalog != _databaseName)
            {
                throw new InvalidOperationException(
                    "A API de testes nao esta usando o banco isolado esperado.");
            }

            _databaseInitialized = true;
            context.Database.Migrate();
            return host;
        }
        catch
        {
            host.Dispose();
            throw;
        }
    }

    protected override void Dispose(bool disposing)
    {
        try
        {
            if (disposing && _databaseInitialized)
            {
                // A limpeza usa somente a conexao gerada por esta instancia.
                var options = new DbContextOptionsBuilder<OrderManagementDbContext>()
                    .UseSqlServer(_connectionString)
                    .Options;

                using var context = new OrderManagementDbContext(options);
                context.Database.EnsureDeleted();
                _databaseInitialized = false;
            }
        }
        finally
        {
            base.Dispose(disposing);
        }
    }
}