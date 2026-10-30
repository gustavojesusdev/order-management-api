# Arquitetura e regras da Semana 7

## Separação de responsabilidades

| Camada | Responsabilidade |
|---|---|
| Controller | HTTP, códigos de resposta e validação automática dos DTOs |
| Service | regras de negócio, cálculos, transições e mapeamento para responses |
| Repository | consultas e gravações com EF Core |
| Data | `DbContext`, configurações Fluent API e seed |
| Model | entidades persistidas e navegações |
| DTO | contrato público de entrada e saída da API |

O controller não recebe nem devolve entidades diretamente. Isso impede que detalhes internos do banco virem, por acidente, parte permanente do contrato HTTP.

## Relacionamentos

| Origem | Destino | Cardinalidade | Motivo |
|---|---|---:|---|
| Cliente | Pedido | 1:N | um cliente pode realizar vários pedidos |
| Pedido | ItemPedido | 1:N | um pedido reúne vários itens |
| Produto | ItemPedido | 1:N | o mesmo produto pode aparecer em pedidos diferentes |
| Pedido | Pagamento | 1:0..1 | pedido pode estar pendente ou possuir exatamente um pagamento |

As exclusões usam comportamento restritivo. Um cliente ou produto que já faz parte do histórico não pode ser removido fisicamente.

## Criação do pedido

1. O DTO exige cliente e pelo menos um item.
2. O service confirma que o cliente existe e está ativo.
3. IDs repetidos e quantidades inválidas são rejeitados.
4. Todos os produtos são carregados em uma consulta.
5. O service confirma que cada produto existe e está ativo.
6. Cada `ItemPedido` copia o preço atual para `PrecoUnitario`.
7. O grafo inteiro é salvo junto e o pedido fica `AguardandoPagamento`.

O preço copiado é um **snapshot histórico**. Se `Produto.Preco` mudar amanhã, o total de um pedido antigo não muda.

## Pagamento

| Tipo | Taxa | Fórmula |
|---|---:|---|
| Pix | 0% | `subtotal` |
| Cartão | 3% | `subtotal + (subtotal * 0,03)` |

A taxa do cartão é arredondada para duas casas com `MidpointRounding.AwayFromZero`. O pagamento armazena `ValorBase`, `Taxa` e `ValorFinal`, deixando explícito o resultado aplicado naquele momento e simplificando auditoria.

## Máquina de estados

Transições aceitas:

- criação com itens: `Criado -> AguardandoPagamento`;
- pagamento: `AguardandoPagamento -> Pago`;
- envio: `Pago -> Enviado`;
- entrega: `Enviado -> Entregue`;
- cancelamento: permitido antes de `Enviado` e `Entregue`;
- `Entregue` e `Cancelado`: nenhuma transição posterior.

Uma requisição não pode simplesmente escolher qualquer número do enum. O DTO valida o formato; o service valida se a mudança faz sentido no estado atual.

## Consultas relacionadas

O repositório de pedidos usa `Include` e `ThenInclude` para montar o agregado:

- cliente;
- itens;
- produto de cada item;
- pagamento.

Consultas somente de leitura usam `AsNoTracking`. Operações que alteram pagamento ou status carregam entidades rastreadas para que o EF Core detecte mudanças.

## Consistência e transação

A criação do pedido adiciona `Pedido` e todos os `ItemPedido` e executa um único `SaveChangesAsync`. Em provedores relacionais, o EF Core envolve as alterações dessa chamada em uma transação: ou o conjunto é salvo, ou a operação falha sem deixar metade do pedido persistida.

Uma transação manual passa a ser necessária quando um único caso de uso exige várias chamadas de gravação que precisam ser atômicas. Não adicionamos uma transação manual artificial apenas para aumentar o código.

## Limites intencionais desta etapa

Os índices e FKs garantem a consistência final do banco. Em uma disputa rara entre duas requisições simultâneas, porém, uma violação de unicidade ainda pode chegar como `DbUpdateException`. Tradução global de erros, `rowversion` e estratégias de concorrência pertencem à etapa de robustez posterior, não ao núcleo da Semana 7.

As datas são criadas com `DateTime.UtcNow` e persistidas como `datetime2`. Se um projeto futuro exigir preservação explícita de fuso/offset, a evolução natural será `DateTimeOffset`.
