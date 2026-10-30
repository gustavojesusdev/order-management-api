# Guia acelerado da Semana 7

## Objetivo

Você não vai reescrever mecanicamente todos os CRUDs. A meta é entender o que realmente mudou entre o checkpoint da Semana 6 e esta versão e provar isso executando os cenários principais.

Tempo sugerido: quatro blocos de 60 a 90 minutos. Podem ser feitos em dois dias, desde que você pare para responder às perguntas de revisão.

## Antes e depois

| Semana 6 | Semana 7 | Conceito novo |
|---|---|---|
| uma entidade principal | cinco entidades relacionadas | modelo relacional real |
| `DbSet<Produto>` | vários `DbSet` e navegações | relacionamentos e FKs |
| configuração dentro do contexto | uma configuração por entidade | Fluent API organizada |
| CRUD simples | caso de uso com várias regras | service como regra de negócio |
| response usando entidade | request e response DTOs | contrato HTTP separado |
| consulta de uma tabela | agregado com vários níveis | `Include` e `ThenInclude` |
| preço atual do produto | preço copiado no item | snapshot histórico |
| sem fluxo | pagamento e status | regras de negócio e máquina de estados |
| exclusão direta | conflito se houver histórico | integridade referencial |

## Bloco 1 — Modelo relacional e EF Core

### Leia nesta ordem

1. `Models/Cliente.cs`, `Pedido.cs`, `ItemPedido.cs` e `Pagamento.cs`.
2. `Models/Produto.cs` e as propriedades de navegação adicionadas.
3. `Enums/StatusPedido.cs` e `TipoPagamento.cs`.
4. `Data/Configurations/`.
5. `Data/OrderManagementDbContext.cs`.
6. A migration `AddOrderDomain` gerada por você.

### Priorize

- FK versus propriedade de navegação;
- 1:N versus 1:0..1;
- `DeleteBehavior.Restrict`;
- índices únicos;
- enum convertido para texto;
- por que dinheiro continua sendo `decimal`.

### Prática

Gere e aplique a migration. No SSMS, desenhe ou inspecione as quatro relações e localize cada FK.

### Você terminou quando explicar

- por que `ItemPedido` precisa de `ProdutoId` e `PedidoId`;
- por que `Pagamento.PedidoId` é único;
- por que não podemos excluir um produto já comprado;
- diferença entre a entidade `Produto` e o snapshot em `ItemPedido`.

Material oficial:

- [Relationships — EF Core](https://learn.microsoft.com/en-us/ef/core/modeling/relationships)
- [Indexes — EF Core](https://learn.microsoft.com/en-us/ef/core/modeling/indexes)
- [Migrations overview](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)

## Bloco 2 — Criação do pedido

### Leia nesta ordem

1. `Dtos/CriarPedidoRequestDto.cs` e `ItemPedidoRequestDto.cs`.
2. `Services/PedidoService.cs`, apenas o método de criação.
3. `Repositories/PedidoRepository.cs`, métodos de gravação.
4. `Controllers/PedidosController.cs`, endpoint `POST`.

### Priorize

- validação estrutural no DTO;
- validação de negócio no service;
- carregamento dos produtos em lote;
- rejeição de IDs repetidos e produtos inativos;
- construção do grafo `Pedido -> Itens`;
- uma chamada de `SaveChangesAsync`.

### Experimento obrigatório: snapshot

1. Crie um pedido contendo um produto.
2. Anote `precoUnitario` no response.
3. Altere `Produto.Preco` com `PUT /api/produtos/{id}`.
4. Consulte novamente o pedido.
5. Confirme que `precoUnitario` e subtotal antigos não mudaram.

### Você terminou quando explicar

- por que o service não salva apenas o `ProdutoId` e consulta o preço toda vez;
- por que DTO validation não substitui as regras do service;
- por que um único `SaveChangesAsync` protege o conjunto da gravação.

Material oficial:

- [Saving related data](https://learn.microsoft.com/en-us/ef/core/saving/related-data)
- [Transactions — EF Core](https://learn.microsoft.com/en-us/ef/core/saving/transactions)

## Bloco 3 — Consultas, pagamento e status

### Leia nesta ordem

1. consultas do `PedidoRepository` com `Include`/`ThenInclude`;
2. mapeamentos para `PedidoResumoResponseDto` e `PedidoDetalheResponseDto`;
3. cálculo e persistência do pagamento no `PedidoService`;
4. transições de status no `PedidoService`;
5. endpoints de pagamento e status no controller.

### Priorize

- `AsNoTracking` em leitura e tracking em alteração;
- `AsSplitQuery` quando uma consulta inclui coleções e várias navegações;
- por que entidade completa não é response DTO;
- taxa Pix 0% e cartão 3%;
- pagamento 1:1;
- estado atual versus estado solicitado;
- filtros opcionais na query.

### Prática

Execute no arquivo HTTP:

- pagamento Pix;
- pagamento cartão e conferência da taxa;
- segundo pagamento no mesmo pedido;
- entrega antes do envio;
- fluxo correto `Pago -> Enviado -> Entregue`;
- filtro por `clienteId` e `status`.

### Você terminou quando explicar

- diferença entre `Include` e `ThenInclude`;
- por que a consulta de atualização não usa `AsNoTracking`;
- onde a taxa deve ser calculada e por quê;
- por que um enum sozinho não garante transições válidas.

Material oficial:

- [Eager loading — EF Core](https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager)
- [Single versus split queries](https://learn.microsoft.com/en-us/ef/core/querying/single-split-queries)
- [Tracking versus no-tracking](https://learn.microsoft.com/en-us/ef/core/querying/tracking)

## Bloco 4 — Teste, revisão e fechamento

### Faça

1. Execute todos os casos de `docs/SETUP_LOCAL.md`.
2. Observe no SSMS as linhas criadas em cada tabela.
3. Rode `dotnet build` novamente.
4. Leia o diff entre seu checkpoint da Semana 6 e esta pasta.
5. Responda sem consultar o código às perguntas abaixo.

### Revisão oral

1. O que controller, service e repository fazem?
2. Qual a diferença entre FK e navegação?
3. Por que usar request e response DTOs separados?
4. Como o snapshot protege o histórico?
5. Como o banco impede dois pagamentos para um pedido?
6. O que `Include` e `ThenInclude` carregam?
7. Quando usar `AsNoTracking`?
8. Que regra impede entregar um pedido pago, mas ainda não enviado?
9. O que acontece se uma das inserções falhar no `SaveChangesAsync`?
10. Por que estoque e autenticação ficaram fora desta semana?

## Critério para começar a Semana 8

Você pode avançar quando:

- a migration gerar e aplicar sem erro;
- o build passar;
- os dez casos obrigatórios funcionarem;
- você conseguir responder pelo menos oito das dez perguntas acima;
- o projeto estiver no GitHub sem `bin`, `obj` ou segredo;
- você souber apontar no código onde cada regra principal vive.

Não precisa memorizar todas as linhas. Precisa compreender o caminho completo de uma requisição e saber localizar cada responsabilidade.

Deixe para depois `rowversion`, níveis de isolamento, tratamento global de `DbUpdateException` e concorrência otimista. Saber que esses problemas existem basta nesta semana; implementá-los agora desviaria do objetivo central.
