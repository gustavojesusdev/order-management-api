# Como apresentar o projeto

## Explicação curta

Estou desenvolvendo uma API de gerenciamento de pedidos em C# e ASP.NET Core. Ela cadastra clientes e produtos, cria pedidos, preserva o preço da compra e controla um fluxo de pagamento simulado, envio e entrega. Os dados ficam no SQL Server. O projeto tem vinte casos automatizados, execução com Docker e verificação de build e testes no GitHub Actions.

É um projeto de estudo e portfólio. Não há cobrança real ou interface de loja.

## O problema e um exemplo

Uma compra precisa continuar consistente mesmo quando o catálogo muda e quando alguém tenta uma operação fora de ordem.

No exemplo da demonstração, duas unidades de R$100 geram subtotal de R$200. O produto depois passa a custar R$199,90, mas aquele pedido continua usando R$100. O pagamento simulado por cartão acrescenta R$6. Uma segunda tentativa de pagar ou uma tentativa de entregar antes de enviar recebe HTTP 409.

O pedido então percorre Pago, Enviado e Entregue. Consultas independentes verificam que os dados foram gravados e mantidos.

## Decisões que você deve conseguir explicar

| Conceito | Como aparece no projeto |
|---|---|
| API REST | Recebe requisições HTTP e devolve JSON e códigos de resposta |
| Controller | Recebe os dados e transforma resultados e erros esperados em respostas HTTP |
| Service | Aplica regras, calcula valores e valida transições |
| Repository | Consulta e grava dados por meio do EF Core |
| Interface | Define o contrato que uma implementação deve cumprir; permite substituir o acesso ao banco nos unitários |
| DTO | Define os campos aceitos e devolvidos pela API, separadamente das entidades persistidas |
| EF Core | Mapeia os objetos para tabelas e executa consultas e gravações |
| Migration | Versiona mudanças na estrutura do banco |
| Snapshot do preço | Copia Produto.Preco para ItemPedido.PrecoUnitario ao criar o pedido |
| Container e volume | Executam os serviços e mantêm os dados do banco entre paradas |
| CI | Executa as verificações automaticamente em uma máquina do GitHub |

## Async e await, sem decorar

Uma consulta ao banco pode levar algum tempo. Com APIs assíncronas, o fluxo pode aguardar essa operação sem manter uma thread bloqueada durante toda a espera de entrada/saída.

Em um método que retorna Task<Pedido>, o resultado esperado é um Pedido quando a operação terminar. O await permite continuar o fluxo usando esse resultado. Async não significa que todo método ganha uma thread nova nem que toda consulta fica mais rápida.

No projeto, o token de cancelamento é repassado até as operações do EF Core. Ele permite solicitar que trabalho ainda pendente seja cancelado, por exemplo quando a requisição é encerrada.

## O que os testes comprovam

Os dez testes unitários chamam o PedidoService real. Repositórios simulados fornecem os dados, então esses casos verificam regras e chamadas de gravação sem SQL Server.

Os dez de integração iniciam a aplicação com TestServer. As requisições passam pelos controllers, serviços e repositórios reais. Nos casos com banco, uma factory aplica migrations em um catálogo temporário LocalDB e remove apenas esse catálogo no final. O teste de status dispensa banco.

O script de demonstração chama a API que já está rodando pela rede local. Isso permite mostrar o ambiente Docker além da suíte com LocalDB.

Vinte casos não representam 100% de cobertura. Não foram medidos carga ou comportamento sob pagamentos simultâneos. O [relatório](RELATORIO_DE_VALIDACAO.md) registra os resultados e o alcance.

## O que mostrar ao apresentar

1. Abra o README para explicar o objetivo.
2. Execute [a demonstração](DEMONSTRACAO.md).
3. Abra o trecho de criação do pedido em PedidoService para mostrar a cópia do preço.
4. Abra um teste unitário para identificar preparação, execução e conferência.
5. Mostre a execução aprovada do CI e explique o que ela verifica.
6. Cite uma evolução possível e seu motivo: autenticação para distinguir usuários, ou controle de concorrência para tratar disputas simultâneas.

## Perguntas para treinar

- Se três unidades custam R$100 cada, qual é o total com a regra de cartão?
- Por que alterar o catálogo não deveria alterar uma compra antiga?
- Por que o teste unitário consegue executar sem banco?
- Qual diferença prática existe entre a suíte com LocalDB e o script que chama o Docker?
- Por que um pedido Pago não pode ir direto para Entregue?
- O que falta para aceitar pagamentos reais?

Confira as respostas no código e nos resultados da demonstração. Para a primeira pergunta, a base é R$300, a taxa R$9 e o total R$309.

## Texto para a seção de projetos do currículo

**Order Management API — projeto de estudo em backend C#/.NET**

Desenvolvimento de API REST com ASP.NET Core, EF Core e SQL Server para clientes, produtos e pedidos, com preservação de preço histórico e regras de pagamento simulado e status. Implementação de vinte casos automatizados com xUnit, ambiente Docker com volume persistente e CI no GitHub Actions.

Adapte esse texto ao currículo real e ao que você consegue demonstrar. Não há currículo anexado para revisão nesta etapa.

## Limites para comunicar com clareza

Esta entrega demonstra backend, persistência, regras de negócio e verificações automatizadas. Autenticação, estoque, gateway financeiro, deploy público e observabilidade são evoluções futuras. O cancelamento de pedido pago conserva o registro; não processa estorno.

O uso de ferramentas de IA no desenvolvimento não substitui entender, revisar e validar o código. Na apresentação, use os exemplos concretos para demonstrar o que você aprendeu.
