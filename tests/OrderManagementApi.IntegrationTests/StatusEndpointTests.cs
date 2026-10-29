using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace OrderManagementApi.IntegrationTests;

public class StatusEndpointTests
{
    [Fact]
    public async Task GetStatus_DeveRetornarOkComIdentificacaoDaApi()
    {
        // Preparar: iniciar a aplicacao no servidor de testes.
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.UseEnvironment("Testing"));

        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

        // Executar: enviar uma requisicao HTTP para a rota real.
        using var response = await client.GetAsync("/api/status");

        // Conferir o codigo HTTP, o formato e os dados da resposta.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);
        var root = json.RootElement;

        Assert.Equal("OrderManagementApi", root.GetProperty("aplicacao").GetString());
        Assert.Equal("online", root.GetProperty("status").GetString());
        Assert.True(root.GetProperty("dataUtc").TryGetDateTime(out var dataUtc));
        Assert.Equal(DateTimeKind.Utc, dataUtc.Kind);
    }
}
