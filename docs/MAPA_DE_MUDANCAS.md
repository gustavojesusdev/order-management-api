# Mapa de mudanças: Semana 6 -> Semana 7

Este arquivo é o índice para comparar o projeto que você construiu com a versão integradora.

## Arquivos que já existiam e evoluíram

| Arquivo/área | Mudança principal | O que estudar |
|---|---|---|
| `Program.cs` | novas injeções, seed geral e enums em JSON | registro de dependências |
| `Models/Produto.cs` | navegação para itens de pedido | relacionamento inverso |
| `Data/OrderManagementDbContext.cs` | cinco `DbSet` e configurações por assembly | descoberta da Fluent API |
| `ProdutoRepository` | `CancellationToken`, busca em lote e vínculo com pedidos | consulta assíncrona |
| `ProdutoService` | response DTO e proteção do histórico | entidade versus contrato |
| `ProdutosController` | DTOs e `409 Conflict` | semântica HTTP |
| `OrderManagementApi.http` | fluxo de ponta a ponta | teste manual reproduzível |
| `DatabaseScripts/` | preservação do script antigo e geração do completo | evolução do schema |

## Áreas novas

| Pasta/arquivo | Papel |
|---|---|
| `Enums/` | estados e tipos de pagamento controlados |
| `Models/Cliente.cs` | cliente que realiza pedidos |
| `Models/Pedido.cs` | raiz do agregado de compra |
| `Models/ItemPedido.cs` | quantidade e snapshot de preço |
| `Models/Pagamento.cs` | resultado financeiro do pagamento |
| `Data/Configurations/` | mapeamento, FKs, índices, precisão e constraints |
| `Data/DatabaseSeed.cs` | clientes e produtos iniciais |
| DTOs de response | formato público separado das entidades |
| CRUD de clientes | segundo CRUD persistente, já no padrão final |
| fluxo de pedidos | consultas, criação, pagamento e status |

## Caminho de uma requisição

Use `POST /api/pedidos` como trilha de leitura:

1. `CriarPedidoRequestDto` descreve o JSON aceito.
2. `PedidosController.Criar` traduz HTTP para uma chamada do service.
3. `PedidoService.CriarAsync` executa as regras.
4. `ClienteRepository` e `ProdutoRepository` consultam as dependências.
5. `PedidoService` monta `Pedido` e `ItemPedido` com snapshot.
6. `PedidoRepository.AdicionarAsync` salva o grafo.
7. `PedidoRepository.ObterPorIdAsync` recarrega os relacionamentos.
8. `PedidoService.MapearDetalhe` cria o response DTO.
9. O controller devolve `201 Created`.

Depois repita a mesma leitura para:

- `POST /api/pedidos/{id}/pagamento`;
- `PATCH /api/pedidos/{id}/status`;
- `GET /api/pedidos/{id}`.

## O que foi preservado de propósito

- as duas migrations da Semana 6;
- o target `.NET 10` e os pacotes existentes;
- a connection string de desenvolvimento;
- o endpoint de status;
- a separação Controller -> Service -> Repository;
- o projeto original enviado, que permanece fora desta cópia.

## O que não entrou

- estoque;
- login, usuário e autorização;
- AutoMapper ou MediatR;
- eventos, mensageria ou cache;
- testes automatizados;
- deploy em nuvem.

Esses itens não são falhas da versão. Eles foram adiados para que você consolide primeiro a integração entre API, EF Core e SQL Server.

