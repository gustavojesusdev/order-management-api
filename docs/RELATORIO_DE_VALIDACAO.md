# Relatório de validação da entrega

> Este documento preserva a preparação inicial de 18/08/2026, a validação manual de 28/10/2026 e a validação automatizada de 29/10/2026, em seções datadas. Para executar a versão atual, siga o README; os comandos da preparação inicial são um registro histórico.

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

## Limites registrados na preparação inicial

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

Esta seção registra a validação manual de 28/10/2026. Os testes automatizados preparados e executados na etapa seguinte estão descritos abaixo.

## Revisão da documentação — 29/10/2026

README e SETUP_LOCAL foram ajustados para conferir e aplicar as migrations versionadas, sem recriar AddOrderDomain, e iniciar a aplicação com `dotnet run --launch-profile http`.

## Validação automatizada — 29/10/2026

A Semana 8 acrescentou dois projetos de testes. As execuções locais no Windows foram conferidas pelas saídas do terminal, e os arquivos foram inspecionados no repositório.

| Projeto | Aprovados | Falhas | Ignorados |
|---|---:|---:|---:|
| OrderManagementApi.Tests | 10 | 0 | 0 |
| OrderManagementApi.IntegrationTests | 10 | 0 | 0 |

Comandos executados na raiz:

```powershell
dotnet test tests/OrderManagementApi.Tests/OrderManagementApi.Tests.csproj
dotnet test tests/OrderManagementApi.IntegrationTests/OrderManagementApi.IntegrationTests.csproj
```

### Testes unitários

Os dez casos verificam o PedidoService real com repositórios simulados: taxas e totais de Pix/cartão, recusa de segundo pagamento, três transições proibidas, três permitidas e uso do preço histórico no pagamento. A preparação repetida foi extraída para métodos auxiliares, mantendo os dez casos aprovados.

A referência explícita a System.Security.Cryptography.Pkcs 9.0.21 corrigiu os avisos da versão indireta 9.0.4 observados durante a restauração. A entrega unitária foi registrada no commit local `1ea3889`.

### Testes de integração

Foi exposta a classe Program com uma declaração public partial para permitir o uso de WebApplicationFactory. O projeto de integração referencia Microsoft.AspNetCore.Mvc.Testing 10.0.14.

O teste de status verifica apenas HTTP e JSON, sem consultar o banco. Os demais cenários usam a aplicação com serviços e repositórios reais, EF Core e SQL Server LocalDB. A factory gera um banco OrderManagementApiTests_<GUID> por instância, valida o catálogo antes de aplicar as migrations e remove esse banco no descarte. Os dados são preparados por requisições HTTP, sem depender do seed ou dos IDs do banco de estudo.

| Cenário | Casos |
|---|---:|
| Status da API, JSON e data UTC | 1 |
| Cadastro de cliente com Location e consulta independente | 1 |
| E-mail duplicado, com 409 e cadastro original preservado | 1 |
| Pedido criado a preço 100; catálogo alterado a 199,90; consultas preservam preço 100 e total 200 no pedido | 1 |
| Cliente ou produto inativo, com 409 e ausência de pedido gravado | 2 |
| Produto repetido, com 400 e ausência de pedido gravado | 1 |
| Pedido inexistente, com 404 e ProblemDetails | 1 |
| Fluxos Pix e cartão até a entrega, com duas recusas 409 e leituras independentes por fluxo | 2 |

Nos fluxos de pagamento, o subtotal é 200. Pix mantém taxa 0 e total 200; cartão registra taxa 6 e total 206. O segundo pagamento e a entrega antes de enviar são recusados. Novas consultas confirmam que as recusas preservam o pagamento e que o pedido termina em Entregue com os valores originais.

A execução das migrations em bancos novos foi exercitada por esses testes. Isso não equivale a executar o arquivo SQL idempotente exportado: a execução desse script em banco novo continua sem evidência nesta condução.

### Alcance da validação

As duas suítes passaram sem erros ou avisos nas saídas finais observadas. Os pagamentos são simulados. Esta validação cobre os cenários enumerados; concorrência entre requisições simultâneas, carga, autenticação, estoque e integrações financeiras não foram avaliados. A suíte com banco, nesta versão, requer Windows e LocalDB; infraestrutura para outros ambientes e CI permanece para as próximas etapas.

O histórico de preparação inicial e de testes manuais acima foi preservado. A afirmação inicial de que testes automatizados seriam futuros descreve o estado daquela preparação; os vinte cenários desta seção correspondem ao estado atual.

## Preparação do ambiente Docker — 30/10/2026

Foram adicionados Dockerfile com construção em estágios e execução como usuário app, compose.yaml com API e SQL Server 2022 Developer, volume persistente, espera pela saúde do banco e publicação local da API em 127.0.0.1.

O script scripts/Start-Docker.ps1 prepara uma senha aleatória em .env apenas se o arquivo ainda não existir, verifica o motor Linux, valida o Compose e inicia os serviços. A senha não é exibida; .env é ignorado pelo Git e pelo contexto de construção. O roteiro de uso está em docs/DOCKER.md.

A API recebeu duas opções: Database:ApplyMigrationsOnStartup, desabilitada por padrão, e HttpsRedirection:Enabled, habilitada por padrão. O Compose habilita migrations/seed no seu banco próprio e configura HTTP para a demonstração local.

Validações executadas neste executor após as alterações:

- dez testes unitários aprovados;
- teste HTTP de status aprovado;
- publicação Release concluída;
- script PowerShell analisado sem erros de sintaxe;
- regras do Git confirmam que .env e .env.local são ignorados.

A tentativa de reexecutar toda a integração encontrou erro de LocalDB: "Cannot create an automatic instance". A execução foi interrompida para evitar repetição da mesma falha de ambiente; não constitui uma revalidação completa dos cenários com banco. Os dez casos aprovados em 29/10 continuam registrados como resultado anterior.

Atualização de 30/10/2026: o usuário instalou WSL e Docker Desktop. As capturas confirmam Desktop 4.97.0, Engine 29.9.2 com servidor Linux/amd64 e Compose v5.6.1. O executor do chat recebeu acesso negado à configuração e à conexão do Docker; os comandos Docker foram executados pelo usuário no PowerShell.

### Execução Docker confirmada

- scripts/Start-Docker.ps1 concluiu a validação do Compose, a construção da imagem da API e a inicialização dos serviços.
- SQL Server 2022 ficou saudável; rede e volume foram criados. A inicialização da API aplicou migrations/seed no banco novo.
- A API foi publicada em 127.0.0.1:5211, respondendo online em /api/status e retornando quatro produtos iniciais em /api/produtos. As respostas HTTP foram também consultadas diretamente pelo assistente.
- Foi cadastrado e consultado via HTTP o cliente id 3, Teste Persistencia Docker.
- O usuário executou docker compose stop e novamente scripts/Start-Docker.ps1. A consulta a /api/clientes/3 retornou o mesmo ID, nome, e-mail e ativo true. Portanto os dados persistiram entre a parada e a retomada com o volume existente.

A execução dos containers e a persistência nesse cenário estão validadas. A suíte de integração continua usando LocalDB no Windows; não foi executada contra o banco do Docker. Não foram testadas remoção do volume, restauração de backup ou recuperação após perda de dados.

## Preparação de CI — 30/10/2026

Foi adicionado .github/workflows/ci.yml: push para main/feat/week-8-portfolio e pull requests para main acionam um runner Windows Server 2025, instalação do SDK .NET 10, inicialização do LocalDB, dez testes unitários e dez testes de integração em Release, publicação da API e armazenamento de relatórios TRX. Também permite acionamento manual quando disponível na branch padrão.

A escolha de Windows preserva a suíte existente com SQL Server LocalDB e bancos temporários isolados. O workflow não usa o banco de estudo, não precisa da senha local .env e não faz deploy. O roteiro está em docs/CI.md. A primeira execução remota continua pendente de envio dos arquivos ao GitHub; a preparação local não é apresentada como resultado aprovado no servidor.

Verificações locais da preparação: actionlint v1.7.12 aprovou o workflow sem diagnósticos, os dez testes unitários passaram em Release (0 falhas e 0 ignorados) com geração de TRX e a publicação Release da API concluiu com sucesso em pasta temporária fora do repositório. Uma nova consulta HTTP após a retomada do Docker conferiu ID, nome, e-mail e ativo do cliente de persistência.

## Validação final local e primeira execução de CI — 30/10/2026

Esta seção atualiza as pendências registradas na preparação acima.

O usuário executou a suíte completa de integração em Release no seu PowerShell: dez aprovados, zero falhas e zero ignorados, em 8s. Esse resultado completa a revalidação local após os ajustes Docker/CI, junto dos dez unitários aprovados em Release pelo assistente. O erro anterior de LocalDB ocorreu no executor do chat; não se reproduziu nessa execução do usuário.

A entrega foi registrada no commit aef49a8 (build: add Docker environment and Windows CI), com doze arquivos. O usuário confirmou árvore limpa e enviou feat/week-8-portfolio para origin, configurando seu rastreamento.

A [execução CI #1](https://github.com/gustavojesusdev/order-management-api/actions/runs/41125787191), acionada por esse push, concluiu com status Success em 1min56s. A captura enviada pelo usuário confirma o job Build e testes (.NET 10 / LocalDB) aprovado e o artifact resultados-dos-testes de 10,7 KB. O job configurado executa as suítes unitária e de integração, verifica a publicação e guarda os relatórios TRX. Essa evidência valida a primeira execução remota no commit citado; não equivale a deploy da API nem à construção Docker no GitHub.

## Merge, solução e apresentação — 30/10/2026

O PR #1 foi incorporado à main no commit 2b0a68c. A consulta direta à API pública do GitHub confirmou o merge e a [execução CI da main](https://github.com/gustavojesusdev/order-management-api/actions/runs/41130330057) concluída com sucesso nesse commit. As execuções de push e de pull request do commit fcfb183 também haviam sido aprovadas.

O aviso MSB4121 observado no VS Code veio da solução temporária do C# Dev Kit: o projeto de integração estava sem o mapeamento Debug/Any CPU. Foi adicionado OrderManagementApi.slnx com a API e os dois projetos de testes. A compilação explícita dessa solução em Debug concluiu com zero erros e zero avisos. O usuário depois informou que o indicador de problemas do editor desapareceu.

A apresentação inclui README reorganizado, detalhes de testes em docs/TESTES.md, guia de explicação em docs/APRESENTACAO.md e roteiro em docs/DEMONSTRACAO.md. O script scripts/Invoke-Demo.ps1 usa registros exclusivos, envia HTTP para a API em execução e verifica criação, preço histórico, pagamento por cartão, duas recusas HTTP 409 e envio/entrega.

O script foi executado com sucesso em PowerShell 7 e Windows PowerShell 5.1 contra a API disponível em localhost:5211. As duas execuções criaram registros distintos e concluíram o fluxo: catálogo 199,90, preço histórico 100, subtotal 200, taxa 6, total 206 e status final Entregue. Os pedidos gerados foram 1 e 2; os dados de demonstração permaneceram no banco. Esses fluxos adicionais são demonstrações e não alteram a contagem de vinte casos xUnit.

Esta seção registra a preparação local da apresentação. O CI citado valida o commit 2b0a68c; os arquivos de apresentação e a solução ainda precisam ser versionados e enviados em uma nova entrega. Nenhuma publicação no LinkedIn, revisão de currículo anexado ou deploy foi realizado nesta etapa.
