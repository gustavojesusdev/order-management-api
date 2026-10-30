using Microsoft.AspNetCore.Mvc;
using OrderManagementApi.Dtos;
using OrderManagementApi.Services;

namespace OrderManagementApi.Controllers;

[ApiController]
[Route("api/produtos")]
public class ProdutosController : ControllerBase
{
    private readonly IProdutoService _service;

    public ProdutosController(IProdutoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProdutoResponseDto>>>
        ObterTodos(CancellationToken cancellationToken)
    {
        IReadOnlyList<ProdutoResponseDto> produtos =
            await _service.ObterTodosAsync(cancellationToken);

        return Ok(produtos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProdutoResponseDto>> ObterPorId(
        int id,
        CancellationToken cancellationToken
    )
    {
        ProdutoResponseDto? produto =
            await _service.ObterPorIdAsync(id, cancellationToken);

        if (produto is null)
        {
            return NotFound();
        }

        return Ok(produto);
    }

    [HttpPost]
    public async Task<ActionResult<ProdutoResponseDto>> Criar(
        ProdutoRequestDto dto,
        CancellationToken cancellationToken
    )
    {
        ProdutoResponseDto novoProduto =
            await _service.CriarAsync(dto, cancellationToken);

        return CreatedAtAction(
            nameof(ObterPorId),
            new { id = novoProduto.Id },
            novoProduto
        );
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(
        int id,
        ProdutoRequestDto dto,
        CancellationToken cancellationToken
    )
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
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Produto não pode ser excluído",
                Detail = exception.Message
            });
        }
    }
}
