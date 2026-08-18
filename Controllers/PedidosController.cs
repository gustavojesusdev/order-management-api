using Microsoft.AspNetCore.Mvc;
using OrderManagementApi.Dtos;
using OrderManagementApi.Enums;
using OrderManagementApi.Services;

namespace OrderManagementApi.Controllers;

[ApiController]
[Route("api/pedidos")]
public class PedidosController : ControllerBase
{
    private readonly IPedidoService _service;

    public PedidosController(IPedidoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<
        ActionResult<IEnumerable<PedidoResumoResponseDto>>
    > ObterTodos(
        [FromQuery] int? clienteId,
        [FromQuery] StatusPedido? status,
        CancellationToken cancellationToken
    )
    {
        if (clienteId.HasValue && clienteId.Value <= 0)
        {
            return CriarProblema(
                StatusCodes.Status400BadRequest,
                "Filtro inválido",
                "O identificador do cliente deve ser maior que zero."
            );
        }

        if (status.HasValue && !Enum.IsDefined(status.Value))
        {
            return CriarProblema(
                StatusCodes.Status400BadRequest,
                "Filtro inválido",
                "O status informado é inválido."
            );
        }

        IReadOnlyList<PedidoResumoResponseDto> pedidos =
            await _service.ObterTodosAsync(
                clienteId,
                status,
                cancellationToken
            );

        return Ok(pedidos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PedidoDetalheResponseDto>>
        ObterPorId(
            int id,
            CancellationToken cancellationToken
        )
    {
        PedidoDetalheResponseDto? pedido =
            await _service.ObterPorIdAsync(
                id,
                cancellationToken
            );

        if (pedido is null)
        {
            return CriarProblema(
                StatusCodes.Status404NotFound,
                "Pedido não encontrado",
                $"Não existe um pedido com o identificador {id}."
            );
        }

        return Ok(pedido);
    }

    [HttpPost]
    public async Task<ActionResult<PedidoDetalheResponseDto>>
        Criar(
            CriarPedidoRequestDto dto,
            CancellationToken cancellationToken
        )
    {
        try
        {
            PedidoDetalheResponseDto pedido =
                await _service.CriarAsync(
                    dto,
                    cancellationToken
                );

            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = pedido.Id },
                pedido
            );
        }
        catch (KeyNotFoundException ex)
        {
            return CriarProblema(
                StatusCodes.Status404NotFound,
                "Recurso não encontrado",
                ex.Message
            );
        }
        catch (ArgumentException ex)
        {
            return CriarProblema(
                StatusCodes.Status400BadRequest,
                "Pedido inválido",
                ex.Message
            );
        }
        catch (InvalidOperationException ex)
        {
            return CriarProblema(
                StatusCodes.Status409Conflict,
                "Pedido não permitido",
                ex.Message
            );
        }
    }

    [HttpPost("{id:int}/pagamento")]
    public async Task<ActionResult<PedidoDetalheResponseDto>>
        RealizarPagamento(
            int id,
            RealizarPagamentoRequestDto dto,
            CancellationToken cancellationToken
        )
    {
        try
        {
            PedidoDetalheResponseDto pedido =
                await _service.RealizarPagamentoAsync(
                    id,
                    dto,
                    cancellationToken
                );

            return Ok(pedido);
        }
        catch (KeyNotFoundException ex)
        {
            return CriarProblema(
                StatusCodes.Status404NotFound,
                "Pedido não encontrado",
                ex.Message
            );
        }
        catch (ArgumentException ex)
        {
            return CriarProblema(
                StatusCodes.Status400BadRequest,
                "Pagamento inválido",
                ex.Message
            );
        }
        catch (InvalidOperationException ex)
        {
            return CriarProblema(
                StatusCodes.Status409Conflict,
                "Pagamento não permitido",
                ex.Message
            );
        }
    }

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<PedidoDetalheResponseDto>>
        AtualizarStatus(
            int id,
            AtualizarStatusPedidoRequestDto dto,
            CancellationToken cancellationToken
        )
    {
        try
        {
            PedidoDetalheResponseDto pedido =
                await _service.AtualizarStatusAsync(
                    id,
                    dto,
                    cancellationToken
                );

            return Ok(pedido);
        }
        catch (KeyNotFoundException ex)
        {
            return CriarProblema(
                StatusCodes.Status404NotFound,
                "Pedido não encontrado",
                ex.Message
            );
        }
        catch (ArgumentException ex)
        {
            return CriarProblema(
                StatusCodes.Status400BadRequest,
                "Status inválido",
                ex.Message
            );
        }
        catch (InvalidOperationException ex)
        {
            return CriarProblema(
                StatusCodes.Status409Conflict,
                "Transição não permitida",
                ex.Message
            );
        }
    }

    private ObjectResult CriarProblema(
        int statusCode,
        string titulo,
        string detalhe
    )
    {
        return Problem(
            statusCode: statusCode,
            title: titulo,
            detail: detalhe
        );
    }
}
