# Configuração e teste local

## Pré-requisitos

- .NET SDK 10;
- SQL Server LocalDB;
- VS Code com a extensão REST Client ou outro cliente HTTP.

## 1. Confirmar o ambiente

```bash
dotnet --version
dotnet tool restore
dotnet ef --version
```

O manifesto `.config/dotnet-tools.json` fixa o `dotnet-ef` na versão `10.0.10`, igual aos pacotes do projeto.

## 2. Restaurar e compilar

Na pasta que contém `OrderManagementApi.csproj`:

```bash
dotnet restore
dotnet build
```

Corrija qualquer erro de compilação antes de tocar no banco.

## 3. Gerar a migration da Semana 7

```bash
dotnet ef migrations add AddOrderDomain
```

Confira a migration gerada. Ela deve adicionar as tabelas `Clientes`, `Pedidos`, `ItensPedido` e `Pagamentos`, além das chaves estrangeiras, índices e restrições.

Não apague nem edite as duas migrations anteriores: elas representam a evolução real do projeto.

## 4. Atualizar o banco

```bash
dotnet ef database update
```

No SSMS, atualize a árvore do banco `OrderManagementApiDb` e verifique:

- tabelas novas;
- chaves primárias e estrangeiras;
- índice único de e-mail;
- índice único de um produto dentro do mesmo pedido;
- índice único do pagamento por pedido;
- dados iniciais de clientes e produtos.

## 5. Executar

```bash
dotnet run
```

Abra `OrderManagementApi.http` e execute os requests de cima para baixo. IDs criados são capturados automaticamente dos responses; ajuste apenas os IDs dos produtos do seed se seu banco anterior usou outra numeração.

## 6. Casos obrigatórios

1. Criar e consultar cliente.
2. Criar pedido com dois produtos ativos.
3. Confirmar que produto inativo é rejeitado.
4. Pagar um pedido por Pix e outro por cartão.
5. Tentar pagar o mesmo pedido duas vezes.
6. Tentar entregar antes de enviar.
7. Fazer `Pago -> Enviado -> Entregue`.
8. Alterar o preço de um produto e confirmar que pedido antigo manteve o snapshot.
9. Tentar excluir cliente ou produto já referenciado e conferir o `409 Conflict`.
10. Filtrar pedidos por cliente e por status.

## 7. Atualizar o script SQL

Depois da migration estar correta:

```bash
dotnet ef migrations script --idempotent --output DatabaseScripts/OrderManagementApiDb.sql
```

O script gerado passa a representar o schema completo e pode ser versionado com a migration.

## Checklist antes do GitHub

```bash
dotnet build
git status
git diff
```

Não versione `bin`, `obj`, `.vs` ou configurações pessoais do editor.
