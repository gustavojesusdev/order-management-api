using Microsoft.AspNetCore.Mvc;
using OrderManagementApi.Dtos;
using OrderManagementApi.Models;
using OrderManagementApi.Services;

namespace OrderManagementApi.Controllers;

[ApiController]
[Route("api/Produtos")]
public class ProdutosController : ControllerBase
{
    private readonly IProdutoService _service;

    public ProdutosController(
        IProdutoService service
    )
    {
        _service = service;
    }

    [HttpGet]
    public async Task<
        ActionResult<IEnumerable<Produto>>
    > ObterTodos()
    {
        IReadOnlyList<Produto> produtos =
            await _service.ObterTodosAsync();

        return Ok(produtos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Produto>>
        ObterPorId(int id)
    {
        Produto? produto =
            await _service.ObterPorIdAsync(id);

        if (produto is null)
        {
            return NotFound();
        }

        return Ok(produto);
    }

    [HttpPost]
    public async Task<ActionResult<Produto>> Criar(
        ProdutoRequestDto dto
    )
    {
        Produto novoProduto =
            await _service.CriarAsync(dto);

        return CreatedAtAction(
            nameof(ObterPorId),
            new { id = novoProduto.Id },
            novoProduto
        );
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(
        int id,
        ProdutoRequestDto dto
    )
    {
        bool atualizado =
            await _service.AtualizarAsync(id, dto);

        if (!atualizado)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        bool excluido =
            await _service.ExcluirAsync(id);

        if (!excluido)
        {
            return NotFound();
        }

        return NoContent();
    }
}