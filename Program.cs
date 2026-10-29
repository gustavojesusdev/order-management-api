using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using OrderManagementApi.Data;
using OrderManagementApi.Repositories;
using OrderManagementApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
    });
builder.Services.AddOpenApi();

string connectionString =
    builder.Configuration.GetConnectionString(
        "OrderManagementApiDb"
    )
    ?? throw new InvalidOperationException(
        "A connection string " +
        "'OrderManagementApiDb' não foi encontrada."
    );

builder.Services.AddDbContext<
    OrderManagementDbContext
>(options =>
{
    options
        .UseSqlServer(connectionString)
        .UseSeeding(DatabaseSeed.Seed)
        .UseAsyncSeeding(
            DatabaseSeed.SeedAsync
        );
});

builder.Services.AddScoped<
    IClienteRepository,
    ClienteRepository
>();

builder.Services.AddScoped<
    IProdutoRepository,
    ProdutoRepository
>();

builder.Services.AddScoped<
    IPedidoRepository,
    PedidoRepository
>();

builder.Services.AddScoped<
    IClienteService,
    ClienteService
>();

builder.Services.AddScoped<
    IProdutoService,
    ProdutoService
>();

builder.Services.AddScoped<
    IPedidoService,
    PedidoService
>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program
{
}