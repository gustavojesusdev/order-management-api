using NSubstitute;
using OrderManagementApi.Dtos;
using OrderManagementApi.Enums;
using OrderManagementApi.Models;
using OrderManagementApi.Repositories;
using OrderManagementApi.Services;

namespace OrderManagementApi.Tests;

public class PedidoServiceTests
{
    [Fact]
    public async Task RealizarPagamentoAsync_ComCartao_DeveAplicarTaxaDeTresPorCento()
    {
        // Preparar: tres unidades a R$120, aguardando pagamento.
        var pedido = CriarPedido();

        var pedidoRepository = CriarRepositorio(pedido);
        var service = CriarService(pedidoRepository);

        // Executar: chamar a regra de pagamento verdadeira.
        var resultado = await service.RealizarPagamentoAsync(
            1,
            new RealizarPagamentoRequestDto { Tipo = TipoPagamento.Cartao }
        );

        // Conferir: resultados esperados e solicitacao de gravacao.
        Assert.Equal(StatusPedido.Pago, resultado.Status);
        Assert.NotNull(resultado.Pagamento);
        Assert.Equal(TipoPagamento.Cartao, resultado.Pagamento.Tipo);
        Assert.Equal(360m, resultado.Pagamento.ValorBase);
        Assert.Equal(10.80m, resultado.Pagamento.Taxa);
        Assert.Equal(370.80m, resultado.Pagamento.ValorFinal);
        await pedidoRepository.Received(1)
            .SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }
    [Fact]
    public async Task RealizarPagamentoAsync_ComPix_NaoDeveCobrarTaxa()
    {
        // Preparar: tres unidades a R$120, aguardando pagamento.
        var pedido = CriarPedido();

        var pedidoRepository = CriarRepositorio(pedido);
        var service = CriarService(pedidoRepository);

        // Executar: chamar a regra de pagamento verdadeira.
        var resultado = await service.RealizarPagamentoAsync(
            1,
            new RealizarPagamentoRequestDto { Tipo = TipoPagamento.Pix }
        );

        // Conferir: resultados esperados e solicitacao de gravacao.
        Assert.Equal(StatusPedido.Pago, resultado.Status);
        Assert.NotNull(resultado.Pagamento);
        Assert.Equal(TipoPagamento.Pix, resultado.Pagamento.Tipo);
        Assert.Equal(360m, resultado.Pagamento.ValorBase);
        Assert.Equal(0m, resultado.Pagamento.Taxa);
        Assert.Equal(360m, resultado.Pagamento.ValorFinal);
        await pedidoRepository.Received(1)
            .SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RealizarPagamentoAsync_QuandoJaPago_DeveRejeitarSegundoPagamento()
    {
        // Preparar: tres unidades a R$120, aguardando pagamento.
        var pedido = CriarPedido();

        var pedidoRepository = CriarRepositorio(pedido);
        var service = CriarService(pedidoRepository);

        // Executar: chamar a regra de pagamento verdadeira.
        var resultado = await service.RealizarPagamentoAsync(
            1,
            new RealizarPagamentoRequestDto { Tipo = TipoPagamento.Pix }
        );

        // O primeiro pagamento já foi realizado por Pix.
        var pagamentoOriginal = pedido.Pagamento;
        Assert.NotNull(pagamentoOriginal);
        Assert.Equal(StatusPedido.Pago, resultado.Status);

        // Tentar pagar novamente, agora por cartão.
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.RealizarPagamentoAsync(
                1,
                new RealizarPagamentoRequestDto
                {
                    Tipo = TipoPagamento.Cartao
                }
            )
        );

        // O pagamento original deve permanecer intacto.
        Assert.Same(pagamentoOriginal, pedido.Pagamento);
        Assert.Equal(StatusPedido.Pago, pedido.Status);
        Assert.Equal(TipoPagamento.Pix, pagamentoOriginal.Tipo);
        Assert.Equal(0m, pagamentoOriginal.Taxa);
        Assert.Equal(360m, pagamentoOriginal.ValorFinal);

        // Apenas o primeiro pagamento solicitou gravação.
        await pedidoRepository.Received(1)
            .SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(StatusPedido.Pago, StatusPedido.Entregue)]
    [InlineData(StatusPedido.Cancelado, StatusPedido.Enviado)]
    [InlineData(StatusPedido.Entregue, StatusPedido.Cancelado)]
    public async Task AtualizarStatusAsync_ComTransicaoProibida_DeveRejeitar(
        StatusPedido statusAtual,
        StatusPedido novoStatus)
    {
        // Preparar o estado que queremos testar.
        var pedido = CriarPedido(statusAtual);

        var pedidoRepository = CriarRepositorio(pedido);
        var service = CriarService(pedidoRepository);

        // A mudança proibida deve ser recusada.
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AtualizarStatusAsync(
                1,
                new AtualizarStatusPedidoRequestDto
                {
                    Status = novoStatus
                }
            )
        );

        // O estado permanece igual, sem solicitar gravação.
        Assert.Equal(statusAtual, pedido.Status);
        await pedidoRepository.DidNotReceive()
            .SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(StatusPedido.Pago, StatusPedido.Enviado)]
    [InlineData(StatusPedido.Enviado, StatusPedido.Entregue)]
    [InlineData(StatusPedido.Pago, StatusPedido.Cancelado)]
    public async Task AtualizarStatusAsync_ComTransicaoPermitida_DeveAtualizar(
        StatusPedido statusAtual,
        StatusPedido novoStatus)
    {
        // Preparar um pedido pago com seus dados relacionados.
        var pagamentoOriginal = new Pagamento
        {
            Tipo = TipoPagamento.Pix,
            ValorBase = 360m,
            Taxa = 0m,
            ValorFinal = 360m
        };

        var pedido = CriarPedido(statusAtual);
        pedido.Pagamento = pagamentoOriginal;

        var pedidoRepository = CriarRepositorio(pedido);
        var service = CriarService(pedidoRepository);

        // Executar uma mudanca permitida.
        var resultado = await service.AtualizarStatusAsync(
            1,
            new AtualizarStatusPedidoRequestDto { Status = novoStatus }
        );

        // Conferir a mudanca, a preservacao do pagamento e a gravacao.
        Assert.Equal(novoStatus, pedido.Status);
        Assert.Equal(novoStatus, resultado.Status);
        Assert.Same(pagamentoOriginal, pedido.Pagamento);
        Assert.NotNull(resultado.Pagamento);
        Assert.Equal(360m, resultado.Pagamento.ValorFinal);
        await pedidoRepository.Received(1)
            .SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RealizarPagamentoAsync_AposAlterarPrecoDoProduto_DeveUsarPrecoDoPedido()
    {
        // Preparar: tres unidades a R$120, aguardando pagamento.
        var pedido = CriarPedido();

        var pedidoRepository = CriarRepositorio(pedido);
        var service = CriarService(pedidoRepository);

        // O preço do catálogo mudou depois da criação do pedido.
        pedido.Itens.Single().Produto.Preco = 199.90m;

        // Executar: chamar a regra de pagamento verdadeira.
        var resultado = await service.RealizarPagamentoAsync(
            1,
            new RealizarPagamentoRequestDto { Tipo = TipoPagamento.Pix }
        );

        // Conferir: resultados esperados e solicitacao de gravacao.
        Assert.Equal(StatusPedido.Pago, resultado.Status);
        Assert.NotNull(resultado.Pagamento);
        Assert.Equal(TipoPagamento.Pix, resultado.Pagamento.Tipo);
        Assert.Equal(360m, resultado.Pagamento.ValorBase);
        Assert.Equal(0m, resultado.Pagamento.Taxa);
        Assert.Equal(360m, resultado.Pagamento.ValorFinal);

        Assert.Equal(199.90m, pedido.Itens.Single().Produto.Preco);
        Assert.Equal(120m, resultado.Itens.Single().PrecoUnitario);

        await pedidoRepository.Received(1)
            .SalvarAlteracoesAsync(Arg.Any<CancellationToken>());
    }


    // Cada chamada cria dados novos: os testes nao compartilham pedidos.
    private static Pedido CriarPedido(
        StatusPedido status = StatusPedido.AguardandoPagamento)
    {
        return new Pedido
        {
            Id = 1,
            ClienteId = 1,
            Cliente = new Cliente
            {
                Id = 1,
                Nome = "Cliente de teste",
                Email = "cliente@example.com"
            },
            Status = status,
            Itens = new List<ItemPedido>
            {
                new ItemPedido
                {
                    ProdutoId = 2,
                    Produto = new Produto { Id = 2, Nome = "Mouse", Preco = 120m },
                    Quantidade = 3,
                    PrecoUnitario = 120m
                }
            }
        };
    }

    private static IPedidoRepository CriarRepositorio(Pedido pedido)
    {
        var repository = Substitute.For<IPedidoRepository>();
        repository
            .ObterPorIdParaAlteracaoAsync(pedido.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Pedido?>(pedido));
        repository
            .SalvarAlteracoesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        return repository;
    }

    private static PedidoService CriarService(IPedidoRepository repository)
    {
        return new PedidoService(
            repository,
            Substitute.For<IClienteRepository>(),
            Substitute.For<IProdutoRepository>()
        );
    }
}

