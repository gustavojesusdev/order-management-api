# Testes automatizados

Os projetos de testes usam o SDK .NET 10. Execute os comandos abaixo na raiz do repositório; cada comando restaura as dependências necessárias e compila os projetos.

O arquivo [OrderManagementApi.slnx](../OrderManagementApi.slnx) reúne a API e os dois projetos de testes. Para compilar os três juntos:

```powershell
dotnet build OrderManagementApi.slnx
```

No VS Code com C# Dev Kit, use **.NET: Switch Solution** e selecione esse arquivo. Assim o editor usa a solução do repositório, com os três projetos, em vez de uma solução temporária gerada para a pasta.

## Testes unitários — 10 cenários

```bash
dotnet test tests/OrderManagementApi.Tests/OrderManagementApi.Tests.csproj
```

Usam o `PedidoService` real e repositórios simulados com NSubstitute. Não precisam iniciar a API ou configurar um banco.

| Regra verificada | Casos |
|---|---:|
| Cartão: base de R$360, taxa de R$10,80 e total de R$370,80 | 1 |
| Pix: base de R$360, taxa zero e total de R$360 | 1 |
| Segundo pagamento recusado, preservando o primeiro | 1 |
| Transições proibidas: Pago → Entregue, Cancelado → Enviado, Entregue → Cancelado | 3 |
| Transições permitidas: Pago → Enviado, Enviado → Entregue, Pago → Cancelado | 3 |
| Pagamento usa o preço histórico do item após alteração do preço do produto | 1 |
| **Total unitário** | **10** |

A preparação comum fica em métodos auxiliares; cada cenário cria seus próprios objetos e substitutos.

## Testes de integração — 10 cenários

Pré-requisitos desta versão: **Windows, SDK .NET 10 e SQL Server LocalDB com a instância `MSSQLLocalDB` disponível para o usuário que executa os testes**.

```bash
dotnet test tests/OrderManagementApi.IntegrationTests/OrderManagementApi.IntegrationTests.csproj
```

O `WebApplicationFactory` inicia a API automaticamente com `TestServer`, em ambiente `Testing`. As requisições HTTP passam pelas rotas e controllers reais, sem precisar executar `dotnet run` nem ocupar a porta 5211.

Nos cenários com banco, a `ApiWebApplicationFactory` cria uma conexão exclusiva para `OrderManagementApiTests_<GUID>`, confere o nome do banco configurado e aplica as migrations versionadas. Os testes usam serviços, repositórios e EF Core reais. Ao encerrar cada instância com `using`, a factory remove apenas o banco temporário gerado por ela. Não é necessário executar `dotnet ef database update` para preparar esses bancos.

A configuração de teste substitui as opções do DbContext e não usa o seed do banco de estudo. Os registros necessários são cadastrados pelo próprio teste, via HTTP. O teste de `/api/status` é independente do banco.

| Cenário verificado por HTTP | Casos |
|---|---:|
| Status: 200, JSON, identificação da API e data UTC | 1 |
| Cadastro de cliente: 201, Location e consulta independente com 200 | 1 |
| E-mail repetido: 409 e cadastro original preservado | 1 |
| Criação de pedido e preço histórico: produto de 100 para 199,90, item permanece 100 e total 200 | 1 |
| Pedido com cliente inativo ou produto inativo: 409 e nenhum pedido gravado | 2 |
| Produto repetido no pedido: 400 e nenhum pedido gravado | 1 |
| Pedido inexistente: 404 com ProblemDetails | 1 |
| Fluxo Pix e cartão: pagamento, recusas de duplicidade/entrega fora de ordem, envio e entrega | 2 |
| **Total de integração** | **10** |

No fluxo de pagamento, a base é R$200: Pix mantém taxa zero e total R$200; cartão aplica taxa de R$6 e total R$206. Novas consultas conferem a persistência dos estados e dos valores, inclusive após as recusas.

## Resultado local e limites

Execuções locais confirmadas em **29/10/2026** e novamente em **30/10/2026**, em Release após a preparação Docker/CI: **10 unitários e 10 de integração aprovados**, com **0 falhas e 0 ignorados** em cada projeto. Em 30/10, as integrações foram executadas no PowerShell do usuário; o executor do chat havia encontrado uma restrição de ambiente ao criar a instância LocalDB.

Os unitários não exercitam persistência. Os testes de integração incluem a cópia automática do preço na criação e sua preservação no SQL Server; os cenários de erro conferem o código HTTP e o conteúdo de ProblemDetails. Esse conjunto cobre os cenários enumerados, sem medir carga ou concorrência entre pagamentos simultâneos. Os pagamentos continuam simulados.

O roteiro manual está em [OrderManagementApi.http](../OrderManagementApi.http). As instruções de integração acima usam LocalDB no Windows. O ambiente Docker executa a aplicação em Linux; o workflow de CI usa Windows para executar as suítes atuais.

A referência explícita a `System.Security.Cryptography.Pkcs 9.0.21` na API atualiza a dependência indireta trazida pelo cliente SQL e corrige os avisos [GHSA-2fmv-rv2c-59x7](https://github.com/advisories/GHSA-2fmv-rv2c-59x7) e [GHSA-846f-f764-76mx](https://github.com/advisories/GHSA-846f-f764-76mx) observados durante a restauração.
