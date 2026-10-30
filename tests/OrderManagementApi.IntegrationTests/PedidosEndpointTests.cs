using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using OrderManagementApi.Dtos;
using OrderManagementApi.Enums;

namespace OrderManagementApi.IntegrationTests;

public class PedidosEndpointTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    [Fact]
    public async Task CriarPedido_AposAlterarPrecoDoProduto_DevePreservarPrecoHistorico()
    {
        using var factory = new ApiWebApplicationFactory();
        using var client = CriarHttpClient(factory);

        var cliente = await CadastrarClienteAsync(client);
        var produto = await CadastrarProdutoAsync(client);

        // O pedido informa produto e quantidade; o servidor deve copiar o preco.
        var request = new CriarPedidoRequestDto
        {
            ClienteId = cliente.Id,
            Itens = new List<ItemPedidoRequestDto>
            {
                new ItemPedidoRequestDto { ProdutoId = produto.Id, Quantidade = 2 }
            }
        };

        using var createResponse = await client.PostAsJsonAsync("/api/pedidos", request, JsonOptions);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<PedidoDetalheResponseDto>(JsonOptions);
        Assert.NotNull(created);
        Assert.True(created.Id > 0);
        Assert.Equal(cliente.Id, created.Cliente.Id);
        Assert.Equal(StatusPedido.AguardandoPagamento, created.Status);
        Assert.Null(created.Pagamento);
        Assert.Equal(200m, created.Subtotal);
        Assert.Equal(100m, Assert.Single(created.Itens).PrecoUnitario);

        var location = createResponse.Headers.Location;
        Assert.NotNull(location);
        Assert.EndsWith($"/api/pedidos/{created.Id}", location.OriginalString);

        // Alterar somente o preco atual do produto no catalogo.
        using var updateResponse = await client.PutAsJsonAsync(
            $"/api/produtos/{produto.Id}",
            new ProdutoRequestDto
            {
                Nome = produto.Nome,
                Preco = 199.90m,
                Ativo = true
            });
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        // Consultas independentes devem mostrar o preco novo e o historico antigo.
        using var productResponse = await client.GetAsync($"/api/produtos/{produto.Id}");
        Assert.Equal(HttpStatusCode.OK, productResponse.StatusCode);
        var currentProduct = await productResponse.Content.ReadFromJsonAsync<ProdutoResponseDto>();
        Assert.NotNull(currentProduct);
        Assert.Equal(199.90m, currentProduct.Preco);

        using var orderResponse = await client.GetAsync(location);
        Assert.Equal(HttpStatusCode.OK, orderResponse.StatusCode);
        var persisted = await orderResponse.Content.ReadFromJsonAsync<PedidoDetalheResponseDto>(JsonOptions);
        Assert.NotNull(persisted);
        Assert.Equal(created.Id, persisted.Id);
        Assert.Equal(cliente.Id, persisted.Cliente.Id);
        Assert.Equal(StatusPedido.AguardandoPagamento, persisted.Status);
        Assert.Null(persisted.Pagamento);
        Assert.Equal(200m, persisted.Subtotal);

        var item = Assert.Single(persisted.Itens);
        Assert.Equal(produto.Id, item.ProdutoId);
        Assert.Equal(produto.Nome, item.NomeProduto);
        Assert.Equal(2, item.Quantidade);
        Assert.Equal(100m, item.PrecoUnitario);
        Assert.Equal(200m, item.Subtotal);
    }

    [Theory]
    [InlineData(false, true, "cliente inativo")]
    [InlineData(true, false, "Produto(s) inativo(s)")]
    public async Task CriarPedido_ComClienteOuProdutoInativo_DeveRetornarConflito(
        bool clienteAtivo,
        bool produtoAtivo,
        string motivoEsperado)
    {
        using var factory = new ApiWebApplicationFactory();
        using var client = CriarHttpClient(factory);

        var cliente = await CadastrarClienteAsync(client, clienteAtivo);
        var produto = await CadastrarProdutoAsync(client, produtoAtivo);
        Assert.Equal(clienteAtivo, cliente.Ativo);
        Assert.Equal(produtoAtivo, produto.Ativo);

        var request = new CriarPedidoRequestDto
        {
            ClienteId = cliente.Id,
            Itens = new List<ItemPedidoRequestDto>
            {
                new ItemPedidoRequestDto { ProdutoId = produto.Id, Quantidade = 1 }
            }
        };

        using var response = await client.PostAsJsonAsync("/api/pedidos", request, JsonOptions);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(409, problem.Status);
        Assert.Equal("Pedido não permitido", problem.Title);
        Assert.Contains(motivoEsperado, problem.Detail ?? string.Empty);

        // A recusa deve deixar a lista de pedidos vazia.
        using var listResponse = await client.GetAsync("/api/pedidos");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var pedidos = await listResponse.Content.ReadFromJsonAsync<List<PedidoResumoResponseDto>>(JsonOptions);
        Assert.NotNull(pedidos);
        Assert.Empty(pedidos);
    }

    [Fact]
    public async Task CriarPedido_ComProdutoRepetido_DeveRetornarBadRequest()
    {
        using var factory = new ApiWebApplicationFactory();
        using var client = CriarHttpClient(factory);

        var cliente = await CadastrarClienteAsync(client);
        var produto = await CadastrarProdutoAsync(client);
        var request = new CriarPedidoRequestDto
        {
            ClienteId = cliente.Id,
            Itens = new List<ItemPedidoRequestDto>
            {
                new ItemPedidoRequestDto { ProdutoId = produto.Id, Quantidade = 1 },
                new ItemPedidoRequestDto { ProdutoId = produto.Id, Quantidade = 2 }
            }
        };

        using var response = await client.PostAsJsonAsync("/api/pedidos", request, JsonOptions);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Equal("Pedido inválido", problem.Title);
        Assert.Contains($"O produto {produto.Id} foi informado mais de uma vez.",
            problem.Detail ?? string.Empty);

        using var listResponse = await client.GetAsync("/api/pedidos");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var pedidos = await listResponse.Content.ReadFromJsonAsync<List<PedidoResumoResponseDto>>(JsonOptions);
        Assert.NotNull(pedidos);
        Assert.Empty(pedidos);
    }

    [Fact]
    public async Task GetPedido_Inexistente_DeveRetornarNotFound()
    {
        using var factory = new ApiWebApplicationFactory();
        using var client = CriarHttpClient(factory);

        // Nenhum pedido foi criado neste banco exclusivo.
        using var response = await client.GetAsync("/api/pedidos/1");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
        Assert.Equal("Pedido não encontrado", problem.Title);
        Assert.Equal("Não existe um pedido com o identificador 1.", problem.Detail);
    }

    [Theory]
    [InlineData(TipoPagamento.Pix, 0, 200)]
    [InlineData(TipoPagamento.Cartao, 6, 206)]
    public async Task Pedido_ComPagamento_DeveRecusarDuplicidadeEConcluirEntrega(
        TipoPagamento tipo,
        int taxaEsperada,
        int valorFinalEsperado)
    {
        using var factory = new ApiWebApplicationFactory();
        using var client = CriarHttpClient(factory);

        var cliente = await CadastrarClienteAsync(client);
        var produto = await CadastrarProdutoAsync(client);
        var request = new CriarPedidoRequestDto
        {
            ClienteId = cliente.Id,
            Itens = new List<ItemPedidoRequestDto>
            {
                new ItemPedidoRequestDto { ProdutoId = produto.Id, Quantidade = 2 }
            }
        };

        using var createResponse = await client.PostAsJsonAsync("/api/pedidos", request, JsonOptions);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var pedido = await createResponse.Content.ReadFromJsonAsync<PedidoDetalheResponseDto>(JsonOptions);
        Assert.NotNull(pedido);
        Assert.Equal(StatusPedido.AguardandoPagamento, pedido.Status);
        Assert.Equal(200m, pedido.Subtotal);
        Assert.Null(pedido.Pagamento);

        // Registrar o primeiro pagamento pela API.
        var paymentRequest = new RealizarPagamentoRequestDto { Tipo = tipo };
        using var paymentResponse = await client.PostAsJsonAsync(
            $"/api/pedidos/{pedido.Id}/pagamento", paymentRequest, JsonOptions);
        Assert.Equal(HttpStatusCode.OK, paymentResponse.StatusCode);

        var paid = await paymentResponse.Content.ReadFromJsonAsync<PedidoDetalheResponseDto>(JsonOptions);
        Assert.NotNull(paid);
        Assert.Equal(StatusPedido.Pago, paid.Status);
        Assert.NotNull(paid.Pagamento);
        Assert.Equal(tipo, paid.Pagamento.Tipo);
        Assert.Equal(200m, paid.Pagamento.ValorBase);
        Assert.Equal((decimal)taxaEsperada, paid.Pagamento.Taxa);
        Assert.Equal((decimal)valorFinalEsperado, paid.Pagamento.ValorFinal);
        var pagamentoOriginal = paid.Pagamento;

        // O mesmo pedido nao pode receber um segundo pagamento.
        using var duplicateResponse = await client.PostAsJsonAsync(
            $"/api/pedidos/{pedido.Id}/pagamento", paymentRequest, JsonOptions);
        var duplicateProblem = await LerConflitoAsync(duplicateResponse, "Pagamento não permitido");
        Assert.Equal("Este pedido já possui um pagamento.", duplicateProblem.Detail);

        // Ainda nao foi enviado: entregar agora deve ser recusado.
        using var invalidStatusResponse = await client.PatchAsJsonAsync(
            $"/api/pedidos/{pedido.Id}/status",
            new AtualizarStatusPedidoRequestDto { Status = StatusPedido.Entregue },
            JsonOptions);
        var statusProblem = await LerConflitoAsync(invalidStatusResponse, "Transição não permitida");
        Assert.Equal("A transição de Pago para Entregue não é permitida.", statusProblem.Detail);

        // Consultar novamente para conferir que as recusas preservaram o pedido.
        var afterRejected = await ObterPedidoAsync(client, pedido.Id);
        Assert.Equal(StatusPedido.Pago, afterRejected.Status);
        Assert.NotNull(afterRejected.Pagamento);
        Assert.Equal(tipo, afterRejected.Pagamento.Tipo);
        Assert.Equal((decimal)taxaEsperada, afterRejected.Pagamento.Taxa);
        Assert.Equal((decimal)valorFinalEsperado, afterRejected.Pagamento.ValorFinal);
        Assert.Equal(pagamentoOriginal.PagoEm, afterRejected.Pagamento.PagoEm);

        // Seguir a ordem permitida: enviar, depois entregar.
        using var sentResponse = await client.PatchAsJsonAsync(
            $"/api/pedidos/{pedido.Id}/status",
            new AtualizarStatusPedidoRequestDto { Status = StatusPedido.Enviado },
            JsonOptions);
        Assert.Equal(HttpStatusCode.OK, sentResponse.StatusCode);
        var sent = await sentResponse.Content.ReadFromJsonAsync<PedidoDetalheResponseDto>(JsonOptions);
        Assert.NotNull(sent);
        Assert.Equal(StatusPedido.Enviado, sent.Status);

        using var deliveredResponse = await client.PatchAsJsonAsync(
            $"/api/pedidos/{pedido.Id}/status",
            new AtualizarStatusPedidoRequestDto { Status = StatusPedido.Entregue },
            JsonOptions);
        Assert.Equal(HttpStatusCode.OK, deliveredResponse.StatusCode);
        var delivered = await deliveredResponse.Content.ReadFromJsonAsync<PedidoDetalheResponseDto>(JsonOptions);
        Assert.NotNull(delivered);
        Assert.Equal(StatusPedido.Entregue, delivered.Status);

        // A leitura final deve recuperar o fluxo e os valores persistidos.
        var persisted = await ObterPedidoAsync(client, pedido.Id);
        Assert.Equal(StatusPedido.Entregue, persisted.Status);
        Assert.Equal(cliente.Id, persisted.Cliente.Id);
        Assert.Equal(200m, persisted.Subtotal);
        var item = Assert.Single(persisted.Itens);
        Assert.Equal(produto.Id, item.ProdutoId);
        Assert.Equal(2, item.Quantidade);
        Assert.Equal(100m, item.PrecoUnitario);

        Assert.NotNull(persisted.Pagamento);
        Assert.Equal(tipo, persisted.Pagamento.Tipo);
        Assert.Equal(200m, persisted.Pagamento.ValorBase);
        Assert.Equal((decimal)taxaEsperada, persisted.Pagamento.Taxa);
        Assert.Equal((decimal)valorFinalEsperado, persisted.Pagamento.ValorFinal);
        Assert.Equal(pagamentoOriginal.PagoEm, persisted.Pagamento.PagoEm);
    }

    private static HttpClient CriarHttpClient(ApiWebApplicationFactory factory)
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false
        });
    }

    private static async Task<PedidoDetalheResponseDto> ObterPedidoAsync(HttpClient client, int id)
    {
        using var response = await client.GetAsync($"/api/pedidos/{id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var pedido = await response.Content.ReadFromJsonAsync<PedidoDetalheResponseDto>(JsonOptions);
        Assert.NotNull(pedido);
        return pedido;
    }

    private static async Task<ProblemDetails> LerConflitoAsync(
        HttpResponseMessage response, string titulo)
    {
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(409, problem.Status);
        Assert.Equal(titulo, problem.Title);
        return problem;
    }

    private static async Task<ClienteResponseDto> CadastrarClienteAsync(
        HttpClient client, bool ativo = true)
    {
        using var response = await client.PostAsJsonAsync("/api/clientes", new ClienteRequestDto
        {
            Nome = "Cliente dos pedidos",
            Email = "cliente.pedidos@example.com",
            Ativo = ativo
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var cliente = await response.Content.ReadFromJsonAsync<ClienteResponseDto>();
        Assert.NotNull(cliente);
        return cliente;
    }

    private static async Task<ProdutoResponseDto> CadastrarProdutoAsync(
        HttpClient client, bool ativo = true)
    {
        using var response = await client.PostAsJsonAsync("/api/produtos", new ProdutoRequestDto
        {
            Nome = "Produto historico",
            Preco = 100m,
            Ativo = ativo
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var produto = await response.Content.ReadFromJsonAsync<ProdutoResponseDto>();
        Assert.NotNull(produto);
        return produto;
    }
}

