using Microsoft.EntityFrameworkCore;
using OrderManagementApi.Data;
using OrderManagementApi.Repositories;
using OrderManagementApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
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
        .UseSeeding(ProdutoSeed.Seed)
        .UseAsyncSeeding(
            ProdutoSeed.SeedAsync
        );
});

builder.Services.AddScoped<
    IProdutoRepository,
    ProdutoRepository
>();

builder.Services.AddScoped<
    IProdutoService,
    ProdutoService
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