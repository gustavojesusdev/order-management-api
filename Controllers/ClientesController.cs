using Microsoft.AspNetCore.Mvc;
using OrderManagementApi.Dtos;
using OrderManagementApi.Services;

namespace OrderManagementApi.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController : ControllerBase
{
    private readonly IClienteService _service;

    public ClientesController(IClienteService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClienteResponseDto>>>
        ObterTodos(CancellationToken cancellationToken)
    {
        IReadOnlyList<ClienteResponseDto> clientes =
            await _service.ObterTodosAsync(cancellationToken);

        return Ok(clientes);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteResponseDto>> ObterPorId(
        int id,
        CancellationToken cancellationToken
    )
    {
        ClienteResponseDto? cliente =
            await _service.ObterPorIdAsync(id, cancellationToken);

        if (cliente is null)
        {
            return NotFound();
        }

        return Ok(cliente);
    }

    [HttpPost]
    public async Task<ActionResult<ClienteResponseDto>> Criar(
        ClienteRequestDto dto,
        CancellationToken cancellationToken
    )
    {
        try
        {
            ClienteResponseDto novoCliente =
                await _service.CriarAsync(dto, cancellationToken);

            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = novoCliente.Id },
                novoCliente
            );
        }
        catch (InvalidOperationException exception)
        {
            return CriarRespostaDeConflito(exception.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(
        int id,
        ClienteRequestDto dto,
        CancellationToken cancellationToken
    )
    {
        try
        {
            bool atualizado = await _service.AtualizarAsync(
                id,
                dto,
                cancellationToken
            );

            if (!atualizado)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (InvalidOperationException exception)
        {
            return CriarRespostaDeConflito(exception.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(
        int id,
        CancellationToken cancellationToken
    )
    {
        try
        {
            bool excluido = await _service.ExcluirAsync(
                id,
                cancellationToken
            );

            if (!excluido)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (InvalidOperationException exception)
        {
            return CriarRespostaDeConflito(exception.Message);
        }
    }

    private ConflictObjectResult CriarRespostaDeConflito(string detalhe)
    {
        return Conflict(new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Operação não permitida para o cliente",
            Detail = detalhe
        });
    }
}
