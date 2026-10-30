# Demonstração da API

## Executar o fluxo completo

Na raiz do repositório, com Docker Desktop em execução:

```powershell
.\scripts\Start-Docker.ps1
.\scripts\Invoke-Demo.ps1
```

O primeiro comando inicia a API e o SQL Server. O segundo envia requisições HTTP reais para a aplicação em http://localhost:5211 e confere as respostas. Ele não inicia os testes xUnit.

Para outra porta:

```powershell
.\scripts\Invoke-Demo.ps1 -BaseUrl http://localhost:5220
```

Use o endereço do seu ambiente de demonstração: o script cria um cliente, um produto, um pedido e um pagamento simulado a cada execução. Esses registros permanecem no banco. Os nomes e e-mails são exclusivos; não há dependência dos IDs do seed e nenhum cadastro anterior é editado.

## O que mostrar

| Momento | Ação | Resultado esperado |
|---|---|---|
| Cadastro | Criar cliente e produto de R$100 | HTTP 201 e IDs gerados |
| Compra | Criar pedido de duas unidades | AguardandoPagamento, subtotal R$200 |
| Preço histórico | Alterar catálogo para R$199,90 | Produto muda; pedido mantém R$100 por unidade |
| Pagamento simulado | Registrar cartão | Pago, taxa R$6 e total R$206 |
| Regra de negócio | Pagar novamente | HTTP 409; pagamento anterior preservado |
| Ordem das etapas | Entregar antes de enviar | HTTP 409; pedido permanece Pago |
| Finalização | Enviar e depois entregar | Nova consulta confirma Entregue e total R$206 |

O pagamento é uma simulação interna; não há cobrança em cartão ou Pix real.

## Roteiro para apresentar

1. Mostre o README e explique o problema: manter pedidos e valores consistentes durante cadastro, compra, pagamento e entrega.
2. Execute os dois comandos e acompanhe as seis etapas impressas pelo script.
3. Aponte a diferença entre o preço atual do catálogo e o preço armazenado no item do pedido.
4. Explique os dois HTTP 409: a API recebeu a solicitação e recusou uma operação incompatível com o estado do pedido.
5. Mostre a [execução do CI na main](https://github.com/gustavojesusdev/order-management-api/actions/runs/41130330057) e as etapas de testes unitários, integração e publicação.
6. Feche com os limites atuais e as próximas evoluções, sem apresentar pagamento simulado como integração financeira.

Para executar cada requisição manualmente, use [OrderManagementApi.http](../OrderManagementApi.http). Esse roteiro mais longo inclui Pix, cartão, cancelamento, filtros e validações adicionais. Ele usa dois IDs do seed; ajuste-os se o banco tiver outros cadastros.

## Em caso de erro

O script interrompe na primeira resposta inesperada. Leia a etapa e a mensagem retornada. Confirme a porta e consulte os containers:

```powershell
docker compose ps
docker compose logs --tail 60 api sqlserver
```

Se o ambiente estiver parado, execute Start-Docker.ps1 novamente. O volume mantém os registros existentes.

## Evidência

A execução do ambiente Docker, a persistência após parar e iniciar e as suítes xUnit estão registradas no [relatório de validação](RELATORIO_DE_VALIDACAO.md). A demonstração deste arquivo possui sua própria verificação de execução; consulte a seção datada de apresentação no relatório.
