# Relatório de validação da entrega

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

## Verificação local ainda obrigatória

Este ambiente não possui o SDK do .NET nem SQL Server. Por isso, não foi possível executar build, gerar a migration, atualizar o banco ou disparar os requests reais.

Na sua máquina, execute:

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

