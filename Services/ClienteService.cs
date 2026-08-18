using OrderManagementApi.Dtos;
using OrderManagementApi.Models;
using OrderManagementApi.Repositories;

namespace OrderManagementApi.Services;

public class ClienteService : IClienteService
{
    private readonly IClienteRepository _repository;

    public ClienteService(IClienteRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<ClienteResponseDto>> ObterTodosAsync(
        CancellationToken cancellationToken = default
    )
    {
        IReadOnlyList<Cliente> clientes =
            await _repository.ObterTodosAsync(cancellationToken);

        return clientes.Select(MapearResposta).ToList();
    }

    public async Task<ClienteResponseDto?> ObterPorIdAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        Cliente? cliente = await _repository.ObterPorIdAsync(
            id,
            cancellationToken
        );

        return cliente is null ? null : MapearResposta(cliente);
    }

    public async Task<ClienteResponseDto> CriarAsync(
        ClienteRequestDto dto,
        CancellationToken cancellationToken = default
    )
    {
        string email = dto.Email.Trim();

        if (await _repository.EmailExisteAsync(
                email,
                cancellationToken: cancellationToken
            ))
        {
            throw new InvalidOperationException(
                "Já existe um cliente cadastrado com este e-mail."
            );
        }

        Cliente cliente = new()
        {
            Nome = dto.Nome.Trim(),
            Email = email,
            Ativo = dto.Ativo
        };

        Cliente clienteCriado = await _repository.AdicionarAsync(
            cliente,
            cancellationToken
        );

        return MapearResposta(clienteCriado);
    }

    public async Task<bool> AtualizarAsync(
        int id,
        ClienteRequestDto dto,
        CancellationToken cancellationToken = default
    )
    {
        if (await _repository.ObterPorIdAsync(id, cancellationToken) is null)
        {
            return false;
        }

        string email = dto.Email.Trim();

        if (await _repository.EmailExisteAsync(
                email,
                id,
                cancellationToken
            ))
        {
            throw new InvalidOperationException(
                "Já existe outro cliente cadastrado com este e-mail."
            );
        }

        Cliente clienteAtualizado = new()
        {
            Id = id,
            Nome = dto.Nome.Trim(),
            Email = email,
            Ativo = dto.Ativo
        };

        return await _repository.AtualizarAsync(
            id,
            clienteAtualizado,
            cancellationToken
        );
    }

    public async Task<bool> ExcluirAsync(
        int id,
        CancellationToken cancellationToken = default
    )
    {
        if (await _repository.PossuiPedidosAsync(id, cancellationToken))
        {
            throw new InvalidOperationException(
                "Não é possível excluir um cliente que possui pedidos, " +
                "pois o histórico precisa ser preservado."
            );
        }

        return await _repository.ExcluirAsync(id, cancellationToken);
    }

    private static ClienteResponseDto MapearResposta(Cliente cliente)
    {
        return new ClienteResponseDto
        {
            Id = cliente.Id,
            Nome = cliente.Nome,
            Email = cliente.Email,
            Ativo = cliente.Ativo
        };
    }
}
