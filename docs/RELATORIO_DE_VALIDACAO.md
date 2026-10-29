# Relatório de validação da entrega

> O registro inicial abaixo descreve a preparação de 18/08/2026. As verificações realizadas na máquina local em 28/10/2026 estão na seção final; a documentação foi revisada em 29/10/2026.

Data da preparação: 18 de agosto de 2026.

## Preservação

- O ZIP recebido não foi alterado.
- SHA-256 do ZIP original: `cf448f28a04848056eda4a100df52a7ae264b25c0135b86b557813a358d1875f`.
- A implementação foi feita em uma cópia independente sem `bin` e `obj`.
- As duas migrations existentes e o snapshot recebido permaneceram byte a byte iguais.
- O script SQL antigo foi preservado como `DatabaseScripts/OrderManagementApiDb-Week6.sql`.

## Verificações concluídas

- 48 arquivos C# inspecionados estruturalmente;
- delimitadores, comentários e strings verificados;
- nenhum marcador de merge ou implementação pendente encontrado;
- arquivos JSON e `.csproj` analisados com sucesso;
- interfaces, implementações e registros de injeção comparados;
- namespaces e tipos públicos verificados;
- relacionamentos, índices, constraints e navegações revisados;
- links locais da documentação conferidos;
- referências dinâmicas do arquivo HTTP conferidas;
- nenhuma credencial evidente encontrada;
- três revisões independentes não encontraram bloqueador aparente de compilação.

Durante a revisão foram corrigidos:

- propagação de `Cliente.Ativo` na atualização;
- limites monetários alinhados a `decimal(10,2)`;
- arredondamento explícito de preços e taxa;
- teste HTTP com captura automática dos IDs criados;
- produto exclusivo para demonstrar o snapshot em execuções repetidas;
- resposta interna apropriada para a falha excepcional de recarga do pedido.

## Verificação local pendente na preparação inicial

Na preparação inicial, o ambiente utilizado não possuía o SDK do .NET nem SQL Server. Por isso, naquele momento não foi possível executar build, gerar a migration, atualizar o banco ou disparar os requests reais. Os resultados locais posteriores estão na seção de 28/10/2026, ao final deste documento.

O roteiro proposto na preparação inicial era:

```bash
dotnet tool restore
dotnet restore
dotnet build
dotnet ef migrations add AddOrderDomain
dotnet ef database update
dotnet run
```

Depois, execute `OrderManagementApi.http` de cima para baixo. A migration local é proposital: o EF Core deve comparar o novo modelo com o snapshot real usando a mesma versão `10.0.10` do seu projeto.

## Limites conhecidos e adiados

- concorrência extrema entre requisições duplicadas ainda pode exigir tradução de `DbUpdateException`;
- `rowversion`, tratamento global de erros e testes automatizados ficam para a etapa de robustez;
- `DateTimeOffset` pode substituir `DateTime` se a preservação explícita de offset virar requisito;
- estoque e autenticação continuam fora do escopo.

Esses pontos não bloqueiam o roteiro didático da Semana 7.

## Validação local — 28/10/2026

A aplicação foi compilada com sucesso com o SDK .NET 10.0.302 e executada no perfil HTTP em `http://localhost:5211`. O histórico do banco listou `InitialCreate`, `ConfigureProdutoSchema` e `AddOrderDomain` como aplicadas.

As verificações foram realizadas manualmente pelo navegador e pelo REST Client, com as respostas conferidas durante a execução.

| Cenário | Resultado observado |
|---|---|
| Cadastro e consulta de cliente | Sucesso; e-mail duplicado rejeitado com 409 |
| Criação e consulta de pedidos | Itens, cliente, preços e totais recuperados corretamente |
| Fluxo Pix | Criação → pagamento → envio → entrega; total 340 e taxa zero |
| Pagamento por cartão | Subtotal 360, taxa de 3% igual a 10.80 e valor final 370.80 |
| Pagamento repetido | Rejeitado com 409 |
| Entrega antes do envio | Rejeitada com 409 |
| Filtros por cliente e status | Resultados compatíveis com os critérios |
| Produto inativo | Rejeitado com 409 |
| Produto repetido no pedido | Rejeitado com 400 |
| Snapshot de preço | Produto passou de 100 para 199.90; item do pedido antigo manteve 100 e pedido manteve total 340 |
| Exclusão de produto ou cliente referenciado | Rejeitada com 409 para preservar o histórico |
| Cancelamento antes do envio | Permitido, mantendo os itens e o registro de pagamento |
| Alteração de pedido cancelado | Rejeitada com 409 por estado final |

O cancelamento não implementa estorno. Os pagamentos são simulados.

O script SQL idempotente foi regenerado e conferido: inclui Produtos, Clientes, Pedidos, ItensPedido, Pagamentos, chaves estrangeiras, índices e o histórico de AddOrderDomain. Não foi executado em um banco novo nesta etapa.

Esta é uma validação manual dos cenários descritos, sem constituir uma suíte automatizada. A preparação dos testes automatizados integra a próxima etapa da Semana 8.

## Revisão da documentação — 29/10/2026

README e SETUP_LOCAL foram ajustados para conferir e aplicar as migrations versionadas, sem recriar AddOrderDomain, e iniciar a aplicação com `dotnet run --launch-profile http`.

