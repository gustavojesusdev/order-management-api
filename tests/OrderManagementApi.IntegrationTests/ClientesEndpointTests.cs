using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using OrderManagementApi.Dtos;

namespace OrderManagementApi.IntegrationTests;

public class ClientesEndpointTests
{
    [Fact]
    public async Task PostCliente_DevePersistirEPermitirConsulta()
    {
        // Preparar a API com banco exclusivo deste teste.
        using var factory = new ApiWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

        var request = new ClienteRequestDto
        {
            Nome = "Cliente integracao",
            Email = "cliente.integracao@example.com",
            Ativo = true
        };

        // Executar o cadastro pela rota real.
        using var createResponse = await client.PostAsJsonAsync("/api/clientes", request);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<ClienteResponseDto>();
        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal(request.Nome, created.Nome);
        Assert.Equal(request.Email, created.Email);
        Assert.True(created.Ativo);

        var location = createResponse.Headers.Location;
        Assert.NotNull(location);
        Assert.EndsWith($"/api/clientes/{created.Id}", location.OriginalString);

        // Uma nova requisicao deve recuperar os dados gravados.
        using var getResponse = await client.GetAsync(location);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var retrieved = await getResponse.Content.ReadFromJsonAsync<ClienteResponseDto>();
        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved.Id);
        Assert.Equal(request.Nome, retrieved.Nome);
        Assert.Equal(request.Email, retrieved.Email);
        Assert.True(retrieved.Ativo);
    }

    [Fact]
    public async Task PostCliente_ComEmailRepetido_DeveRetornarConflitoEPreservarOriginal()
    {
        using var factory = new ApiWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });

        var originalRequest = new ClienteRequestDto
        {
            Nome = "Cliente original",
            Email = "cliente.original@example.com",
            Ativo = true
        };

        using var firstResponse = await client.PostAsJsonAsync("/api/clientes", originalRequest);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        var original = await firstResponse.Content.ReadFromJsonAsync<ClienteResponseDto>();
        Assert.NotNull(original);

        // Outro nome e outro estado, mas o mesmo e-mail.
        var duplicateRequest = new ClienteRequestDto
        {
            Nome = "Outro cliente",
            Email = originalRequest.Email,
            Ativo = false
        };

        using var duplicateResponse = await client.PostAsJsonAsync("/api/clientes", duplicateRequest);
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
        Assert.Equal("application/problem+json",
            duplicateResponse.Content.Headers.ContentType?.MediaType);

        var problem = await duplicateResponse.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(409, problem.Status);
        Assert.Equal("Operação não permitida para o cliente", problem.Title);
        Assert.Equal("Já existe um cliente cadastrado com este e-mail.", problem.Detail);

        // Uma nova consulta deve encontrar apenas o cadastro original.
        using var listResponse = await client.GetAsync("/api/clientes");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var clientes = await listResponse.Content.ReadFromJsonAsync<List<ClienteResponseDto>>();
        Assert.NotNull(clientes);

        var persisted = Assert.Single(clientes);
        Assert.Equal(original.Id, persisted.Id);
        Assert.Equal(originalRequest.Nome, persisted.Nome);
        Assert.Equal(originalRequest.Email, persisted.Email);
        Assert.True(persisted.Ativo);
    }
}

