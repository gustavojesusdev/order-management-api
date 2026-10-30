# Scripts do banco

- `OrderManagementApiDb-Week6.sql` preserva o script do checkpoint recebido e serve para comparação.
- `OrderManagementApiDb.sql` será gerado por você depois de criar a migration `AddOrderDomain`.

Comando:

```bash
dotnet ef migrations script --idempotent --output DatabaseScripts/OrderManagementApiDb.sql
```

Não use o script da Semana 6 para criar o banco completo da Semana 7: ele ainda não possui clientes, pedidos, itens ou pagamentos.

