# Execução local com Docker

Esta configuração prepara a API .NET 10 e o SQL Server 2022 Developer em dois containers Linux. O banco usa um volume próprio, separado do LocalDB utilizado na execução nativa do Windows.

## Situação da validação

Preparado em 30/10/2026. Os dez testes unitários, o teste HTTP de status e a publicação Release foram executados com sucesso após os ajustes. A revalidação completa da integração neste executor não pôde ser concluída: o LocalDB retornou "Cannot create an automatic instance". O resultado anterior de dez integrações aprovadas em 29/10 permanece registrado como execução anterior.

Validado localmente em 30/10/2026 com Docker Desktop 4.97.0, Engine 29.9.2 Linux/amd64 e Compose v5.6.1. O usuário executou scripts/Start-Docker.ps1 no PowerShell: a imagem da API foi construída, o SQL Server ficou saudável e a API consultou quatro produtos iniciais. As migrations e o seed foram aplicados na inicialização do banco novo.

O assistente confirmou por HTTP o status online e a listagem dos quatro produtos; cadastrou e consultou um cliente de teste. Depois de docker compose stop e nova execução do script, a consulta retornou o mesmo cliente id 3, nome, e-mail e estado ativo. Isso valida a persistência após parar e iniciar os containers, mantendo o volume. A retomada também reutilizou as camadas já construídas da imagem.

O controle do Docker pelo executor do chat recebeu acesso negado, por isso os comandos Docker foram executados pelo usuário e confirmados em capturas. Esse resultado não equivale a executar a suíte de integração LocalDB no container nem a testar restauração de backup ou recuperação após perda do volume.

## Conceitos

- **WSL 2:** recurso do Windows que executa um kernel Linux e dá suporte ao motor Linux do Docker Desktop.
- **Imagem:** pacote com a aplicação e seu ambiente de execução. O Dockerfile descreve sua construção.
- **Container:** uma instância em execução de uma imagem.
- **Compose:** configuração que conecta e inicia os serviços da API e do banco.
- **Volume:** armazenamento do banco que permanece quando os containers são parados ou recriados.

## Preparar o computador Windows

Em um PowerShell como administrador:

```powershell
wsl --install --no-distribution
```

Se o Windows pedir reinício, salve seus arquivos e reinicie. Depois:

```powershell
wsl --update
wsl --version
```

Instale o [Docker Desktop para Windows x86_64](https://docs.docker.com/desktop/setup/install/windows-install/), usando o motor WSL 2 e Linux containers. Para usar o Docker pelo terminal do Windows, não é necessário instalar Ubuntu separadamente.

Abra o Docker Desktop e aguarde o motor iniciar. Reabra o terminal e confira:

```powershell
docker version
docker compose version
```

O primeiro comando deve mostrar as informações do cliente e do servidor.

## Iniciar API e banco

Na raiz do repositório:

```powershell
.\scripts\Start-Docker.ps1
```

Na primeira execução, o script gera uma senha aleatória em .env. Esse arquivo é ignorado pelo Git e excluído do contexto de construção da imagem. O arquivo .env.example documenta as chaves disponíveis; na configuração manual, preencha uma senha antes de iniciar o Compose.

O script confere o motor Linux, valida a configuração sem exibir as variáveis, constrói a imagem, inicia os serviços e consulta status e produtos até a API responder.

Por padrão, a API fica em:

- http://localhost:5211/api/status
- http://localhost:5211/api/produtos

Para usar outra porta local, ajuste API_PORT em .env. A porta escolhida precisa estar livre; encerre uma execução nativa da API se ela estiver ocupando a porta 5211.

## Como a inicialização funciona

1. O SQL Server inicia e o healthcheck executa uma consulta com sqlcmd.
2. O Compose espera o banco ficar saudável antes de iniciar a API.
3. Database__ApplyMigrationsOnStartup=true habilita as migrations e o seed existente no banco OrderManagementApiDockerDb.
4. A API atende HTTP na porta interna 8080, publicada apenas em 127.0.0.1 na porta escolhida.

Essa opção de migrations automáticas destina-se ao ambiente local de demonstração. No modo nativo, sem habilitá-la, permanece o procedimento manual do README.

HttpsRedirection__Enabled=false configura o HTTP local desse Compose. O comportamento padrão da API continua sendo habilitar o redirecionamento HTTPS.

A API recebe a conexão por variável de ambiente. O SQL Server fica na rede interna do Compose e não publica sua porta no Windows. A edição Developer destina-se a desenvolvimento e testes.

As tags das imagens acompanham as linhas .NET 10 e SQL Server 2022 e recebem atualizações. Fixar digests é uma evolução possível quando for necessário reproduzir exatamente os mesmos binários.

## Conferir, parar e retomar

```powershell
docker compose ps
docker compose logs --tail 80 api sqlserver
docker compose stop
docker compose start
```

O volume preserva os cadastros nessas operações. O comando docker compose down remove os containers e a rede, mantendo o volume. A opção --volumes também apaga os dados; use-a apenas quando quiser descartar deliberadamente o banco do Docker.

## Relação com os testes

Os unitários continuam independentes de banco. Os testes de integração atuais continuam usando bancos temporários no LocalDB do Windows; este Compose prepara a execução da aplicação, sem alterar o destino da suíte existente.

Depois de iniciar os containers, o roteiro OrderManagementApi.http pode exercitar a API em http://localhost:5211.

## Referências

- [WSL: comandos de instalação](https://learn.microsoft.com/en-us/windows/wsl/basic-commands)
- [Docker Desktop com WSL 2](https://docs.docker.com/desktop/features/wsl/)
- [SQL Server em containers](https://learn.microsoft.com/en-us/sql/linux/install-upgrade/quickstart-install-docker?view=sql-server-ver16)
- [Ordem e saúde dos serviços no Compose](https://docs.docker.com/compose/how-tos/startup-order/)
