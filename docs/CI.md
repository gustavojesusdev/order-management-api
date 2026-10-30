# Testes automaticos no GitHub

## Como funciona

O arquivo [.github/workflows/ci.yml](../.github/workflows/ci.yml) define a integracao continua (CI). O GitHub prepara uma maquina temporaria, baixa o codigo e verifica se o projeto continua compilando e passando nos testes.

O fluxo e acionado por envio de commits para main ou feat/week-8-portfolio, por pull requests direcionados a main e manualmente pela aba Actions quando o workflow estiver disponivel na branch padrao.

## Etapas

1. Obter o codigo e instalar o SDK .NET 10.
2. Localizar e iniciar a instancia MSSQLLocalDB do usuario do runner; criar a instancia se ainda nao existir.
3. Restaurar, compilar em Release e executar os dez testes unitarios.
4. Restaurar, compilar em Release e executar os dez testes de integracao HTTP/SQL.
5. Publicar a API em uma pasta temporaria para conferir a geracao dos arquivos de execucao.
6. Anexar os relatorios TRX que tiverem sido gerados, inclusive em caso de falha, por sete dias.

Foi escolhido windows-2025 porque a factory atual usa SQL Server LocalDB com autenticacao integrada do Windows. O inventario oficial da imagem inclui o componente LocalDB; o workflow verifica o executavel antes dos testes. Cada teste com banco continua usando seu proprio catalogo OrderManagementApiTests_<GUID> e o remove ao encerrar.

Os testes usam TestServer: nao precisam iniciar a API na porta 5211. O workflow nao usa o banco de estudo nem o volume do Docker Desktop. Nao exige cadastrar uma senha do SQL Server nos secrets.

A construcao Docker foi validada localmente em 30/10/2026; este primeiro workflow automatiza as suites existentes e a publicacao .NET. Ele nao executa Docker, nao publica imagens e nao faz deploy.

## Conferir resultados

Depois de enviar a branch, abra a [aba Actions do repositorio](https://github.com/gustavojesusdev/order-management-api/actions) e escolha CI. O job Build e testes mostra cada etapa. Em caso de falha, abra a etapa marcada em vermelho e leia o primeiro erro. Os arquivos TRX ficam em resultados-dos-testes ao final da execucao, se tiverem sido gerados.

## Validacao e limites

Preparado e executado em 30/10/2026. A [primeira execucao no GitHub](https://github.com/gustavojesusdev/order-management-api/actions/runs/41125787191), acionada por push do commit aef49a8 na branch feat/week-8-portfolio, terminou com status Success em 1min56s. O job Build e testes (.NET 10 / LocalDB) foi aprovado e gerou o artifact resultados-dos-testes (10,7 KB). O resultado foi confirmado pela captura da pagina de execucao enviada pelo usuario.

Verificacoes locais da preparacao: actionlint v1.7.12 aprovou o workflow sem diagnosticos; os dez testes unitarios passaram em Release e geraram o relatorio TRX; a publicacao Release da API tambem concluiu com sucesso. A publicacao usa uma pasta temporaria fora da arvore de codigo.

As duas suites passaram localmente em 29/10 e novamente em 30/10, em Release: dez unitarios e dez integracoes, sem falhas ou ignorados. A reexecucao das integracoes pelo executor do chat em 30/10 encontrou erro de criacao de instancia LocalDB; o usuario executou a suite completa com sucesso no seu PowerShell. O workflow tambem executa a suite completa, sem filtros para omitir testes com banco.

## Referencias

- [GitHub: compilar e testar .NET](https://docs.github.com/en/actions/tutorials/build-and-test-code/net)
- [Software da imagem Windows Server 2025](https://github.com/actions/runner-images/blob/main/images/windows/Windows2025-Readme.md)
- [Preparacao do SDK .NET](https://github.com/actions/setup-dotnet)
- [Relatorios como artifacts](https://github.com/actions/upload-artifact)
