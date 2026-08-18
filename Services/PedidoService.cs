using OrderManagementApi.Dtos;
using OrderManagementApi.Enums;
using OrderManagementApi.Models;
using OrderManagementApi.Repositories;

namespace OrderManagementApi.Services;

public class PedidoService : IPedidoService
{
    private const decimal ValorMonetarioMaximo =
        99_999_999.99m;

    private readonly IPedidoRepository _pedidoRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IProdutoRepository _produtoRepository;

    public PedidoService(
        IPedidoRepository pedidoRepository,
        IClienteRepository clienteRepository,
        IProdutoRepository produtoRepository
    )
    {
        _pedidoRepository = pedidoRepository;
        _clienteRepository = clienteRepository;
        _produtoRepository = produtoRepository;
    }

    public async Task<
        IReadOnlyList<PedidoResumoResponseDto>
    > ObterTodosAsync(
        int? clienteId = null,
        StatusPedido? status = null,
        CancellationToken cancellationToken = default
    )
    {
        IReadOnlyList<Pedido> pedidos =
            await _pedidoRepository.ObterTodosAsync(
                clienteId,
                status,
                cancellationToken
            );

        return pedidos
            .Select(MapearResumo)
            .ToList();
    }

    public async Task<PedidoDetalheResponseDto?>
        ObterPorIdAsync(
            int id,
            CancellationToken cancellationToken = default
        )
    {
        Pedido? pedido =
            await _pedidoRepository.ObterPorIdAsync(
                id,
                cancellationToken
            );

        return pedido is null
            ? null
            : MapearDetalhe(pedido);
    }

    public async Task<PedidoDetalheResponseDto>
        CriarAsync(
            CriarPedidoRequestDto dto,
            CancellationToken cancellationToken = default
        )
    {
        ValidarItensInformados(dto.Itens);

        Cliente? cliente =
            await _clienteRepository.ObterPorIdAsync(
                dto.ClienteId,
                cancellationToken
            );

        if (cliente is null)
        {
            throw new KeyNotFoundException(
                $"Cliente {dto.ClienteId} não encontrado."
            );
        }

        if (!cliente.Ativo)
        {
            throw new InvalidOperationException(
                "Não é possível criar um pedido para um cliente inativo."
            );
        }

        int[] produtoIds = dto.Itens
            .Select(item => item.ProdutoId)
            .Distinct()
            .ToArray();

        IReadOnlyList<Produto> produtos =
            await _produtoRepository.ObterPorIdsAsync(
                produtoIds,
                cancellationToken
            );

        int[] produtosNaoEncontrados = produtoIds
            .Except(produtos.Select(produto => produto.Id))
            .OrderBy(id => id)
            .ToArray();

        if (produtosNaoEncontrados.Length > 0)
        {
            throw new KeyNotFoundException(
                "Produto(s) não encontrado(s): " +
                string.Join(", ", produtosNaoEncontrados) +
                "."
            );
        }

        int[] produtosInativos = produtos
            .Where(produto => !produto.Ativo)
            .Select(produto => produto.Id)
            .OrderBy(id => id)
            .ToArray();

        if (produtosInativos.Length > 0)
        {
            throw new InvalidOperationException(
                "Produto(s) inativo(s) não podem ser adicionados: " +
                string.Join(", ", produtosInativos) +
                "."
            );
        }

        Dictionary<int, Produto> produtosPorId =
            produtos.ToDictionary(produto => produto.Id);

        Pedido pedido = new()
        {
            ClienteId = cliente.Id,
            Status = StatusPedido.Criado,
            CriadoEm = DateTime.UtcNow
        };

        foreach (ItemPedidoRequestDto itemDto in dto.Itens)
        {
            Produto produto =
                produtosPorId[itemDto.ProdutoId];

            pedido.Itens.Add(new ItemPedido
            {
                ProdutoId = produto.Id,
                Quantidade = itemDto.Quantidade,
                PrecoUnitario = produto.Preco
            });
        }

        pedido.Status = StatusPedido.AguardandoPagamento;

        ValidarLimiteMonetario(
            CalcularSubtotal(pedido),
            "O subtotal do pedido"
        );

        await _pedidoRepository.AdicionarAsync(
            pedido,
            cancellationToken
        );

        Pedido? pedidoCriado =
            await _pedidoRepository.ObterPorIdAsync(
                pedido.Id,
                cancellationToken
            );

        if (pedidoCriado is null)
        {
            throw new System.IO.InvalidDataException(
                "O pedido foi criado, mas não pôde ser recarregado."
            );
        }

        return MapearDetalhe(pedidoCriado);
    }

    public async Task<PedidoDetalheResponseDto>
        RealizarPagamentoAsync(
            int id,
            RealizarPagamentoRequestDto dto,
            CancellationToken cancellationToken = default
        )
    {
        if (!Enum.IsDefined(dto.Tipo))
        {
            throw new ArgumentException(
                "A forma de pagamento informada é inválida.",
                nameof(dto)
            );
        }

        Pedido pedido =
            await ObterPedidoParaAlteracaoAsync(
                id,
                cancellationToken
            );

        if (pedido.Pagamento is not null)
        {
            throw new InvalidOperationException(
                "Este pedido já possui um pagamento."
            );
        }

        if (pedido.Status !=
            StatusPedido.AguardandoPagamento)
        {
            throw new InvalidOperationException(
                "Somente pedidos aguardando pagamento podem ser pagos."
            );
        }

        decimal subtotal = CalcularSubtotal(pedido);
        decimal taxa = dto.Tipo == TipoPagamento.Cartao
            ? decimal.Round(
                subtotal * 0.03m,
                2,
                MidpointRounding.AwayFromZero
            )
            : 0m;

        decimal valorFinal = subtotal + taxa;

        ValidarLimiteMonetario(
            valorFinal,
            "O valor final do pagamento"
        );

        pedido.Pagamento = new Pagamento
        {
            Tipo = dto.Tipo,
            ValorBase = subtotal,
            Taxa = taxa,
            ValorFinal = valorFinal,
            PagoEm = DateTime.UtcNow
        };

        pedido.Status = StatusPedido.Pago;

        await _pedidoRepository.SalvarAlteracoesAsync(
            cancellationToken
        );

        return MapearDetalhe(pedido);
    }

    public async Task<PedidoDetalheResponseDto>
        AtualizarStatusAsync(
            int id,
            AtualizarStatusPedidoRequestDto dto,
            CancellationToken cancellationToken = default
        )
    {
        if (!Enum.IsDefined(dto.Status))
        {
            throw new ArgumentException(
                "O status informado é inválido.",
                nameof(dto)
            );
        }

        Pedido pedido =
            await ObterPedidoParaAlteracaoAsync(
                id,
                cancellationToken
            );

        if (pedido.Status is StatusPedido.Entregue or
            StatusPedido.Cancelado)
        {
            throw new InvalidOperationException(
                "Não é possível alterar um pedido finalizado."
            );
        }

        ValidarTransicao(pedido.Status, dto.Status);

        pedido.Status = dto.Status;

        await _pedidoRepository.SalvarAlteracoesAsync(
            cancellationToken
        );

        return MapearDetalhe(pedido);
    }

    private async Task<Pedido>
        ObterPedidoParaAlteracaoAsync(
            int id,
            CancellationToken cancellationToken
        )
    {
        Pedido? pedido =
            await _pedidoRepository
                .ObterPorIdParaAlteracaoAsync(
                    id,
                    cancellationToken
                );

        return pedido ?? throw new KeyNotFoundException(
            $"Pedido {id} não encontrado."
        );
    }

    private static void ValidarItensInformados(
        IReadOnlyCollection<ItemPedidoRequestDto>? itens
    )
    {
        if (itens is null || itens.Count == 0)
        {
            throw new ArgumentException(
                "O pedido deve possuir pelo menos um item.",
                nameof(itens)
            );
        }

        if (itens.Any(item => item is null))
        {
            throw new ArgumentException(
                "Todos os itens do pedido devem ser preenchidos.",
                nameof(itens)
            );
        }

        ItemPedidoRequestDto? itemInvalido = itens
            .FirstOrDefault(item =>
                item.ProdutoId <= 0 ||
                item.Quantidade <= 0
            );

        if (itemInvalido is not null)
        {
            throw new ArgumentException(
                "Produto e quantidade devem ser maiores que zero.",
                nameof(itens)
            );
        }

        int? produtoRepetido = itens
            .GroupBy(item => item.ProdutoId)
            .Where(grupo => grupo.Count() > 1)
            .Select(grupo => (int?)grupo.Key)
            .FirstOrDefault();

        if (produtoRepetido.HasValue)
        {
            throw new ArgumentException(
                $"O produto {produtoRepetido.Value} foi informado mais de uma vez.",
                nameof(itens)
            );
        }
    }

    private static void ValidarTransicao(
        StatusPedido statusAtual,
        StatusPedido novoStatus
    )
    {
        bool transicaoPermitida =
            (
                statusAtual == StatusPedido.Pago &&
                novoStatus == StatusPedido.Enviado
            ) ||
            (
                statusAtual == StatusPedido.Enviado &&
                novoStatus == StatusPedido.Entregue
            ) ||
            (
                (
                    statusAtual == StatusPedido.Criado ||
                    statusAtual ==
                        StatusPedido.AguardandoPagamento ||
                    statusAtual == StatusPedido.Pago
                ) &&
                novoStatus == StatusPedido.Cancelado
            );

        if (!transicaoPermitida)
        {
            throw new InvalidOperationException(
                $"A transição de {statusAtual} para {novoStatus} não é permitida."
            );
        }
    }

    private static PedidoResumoResponseDto MapearResumo(
        Pedido pedido
    )
    {
        return new PedidoResumoResponseDto
        {
            Id = pedido.Id,
            ClienteId = pedido.ClienteId,
            NomeCliente = pedido.Cliente.Nome,
            Status = pedido.Status,
            CriadoEm = pedido.CriadoEm,
            Subtotal = CalcularSubtotal(pedido),
            ValorFinal = pedido.Pagamento?.ValorFinal
        };
    }

    private static PedidoDetalheResponseDto MapearDetalhe(
        Pedido pedido
    )
    {
        return new PedidoDetalheResponseDto
        {
            Id = pedido.Id,
            Cliente = new ClienteResponseDto
            {
                Id = pedido.Cliente.Id,
                Nome = pedido.Cliente.Nome,
                Email = pedido.Cliente.Email,
                Ativo = pedido.Cliente.Ativo
            },
            Status = pedido.Status,
            CriadoEm = pedido.CriadoEm,
            Itens = pedido.Itens
                .OrderBy(item => item.Id)
                .Select(item => new ItemPedidoResponseDto
                {
                    ProdutoId = item.ProdutoId,
                    NomeProduto = item.Produto.Nome,
                    Quantidade = item.Quantidade,
                    PrecoUnitario = item.PrecoUnitario,
                    Subtotal = item.Subtotal
                })
                .ToList(),
            Subtotal = CalcularSubtotal(pedido),
            Pagamento = pedido.Pagamento is null
                ? null
                : new PagamentoResponseDto
                {
                    Tipo = pedido.Pagamento.Tipo,
                    ValorBase = pedido.Pagamento.ValorBase,
                    Taxa = pedido.Pagamento.Taxa,
                    ValorFinal = pedido.Pagamento.ValorFinal,
                    PagoEm = pedido.Pagamento.PagoEm
                }
        };
    }

    private static decimal CalcularSubtotal(
        Pedido pedido
    )
    {
        return pedido.Itens.Sum(item => item.Subtotal);
    }

    private static void ValidarLimiteMonetario(
        decimal valor,
        string campo
    )
    {
        if (valor > ValorMonetarioMaximo)
        {
            throw new InvalidOperationException(
                $"{campo} ultrapassa o limite de " +
                $"{ValorMonetarioMaximo:N2}."
            );
        }
    }
}
