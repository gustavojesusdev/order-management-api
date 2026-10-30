[CmdletBinding()]
param(
    [Uri]$BaseUrl = 'http://localhost:5211'
)

$ErrorActionPreference = 'Stop'
if (-not $BaseUrl.IsAbsoluteUri -or $BaseUrl.Scheme -notin @('http', 'https')) {
    throw 'Informe uma URL HTTP ou HTTPS absoluta para a API.'
}

Add-Type -AssemblyName System.Net.Http
$demoClient = [System.Net.Http.HttpClient]::new()
$demoClient.BaseAddress = [Uri]($BaseUrl.AbsoluteUri.TrimEnd('/') + '/')
$demoClient.Timeout = [TimeSpan]::FromSeconds(15)

function Invoke-DemoRequest {
    param(
        [string]$Method,
        [string]$Path,
        [int]$ExpectedStatus,
        [object]$Body = $null
    )

    $request = [System.Net.Http.HttpRequestMessage]::new(
        [System.Net.Http.HttpMethod]::new($Method),
        $Path.TrimStart('/'))
    $response = $null
    try {
        if ($null -ne $Body) {
            $json = $Body | ConvertTo-Json -Depth 10
            $request.Content = [System.Net.Http.StringContent]::new(
                $json, [System.Text.Encoding]::UTF8, 'application/json')
        }

        $response = $demoClient.SendAsync($request).GetAwaiter().GetResult()
        $text = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        $status = [int]$response.StatusCode
        if ($status -ne $ExpectedStatus) {
            throw "$Method $Path retornou HTTP $status; esperado $ExpectedStatus. $text"
        }
        if (-not [string]::IsNullOrWhiteSpace($text)) {
            return ($text | ConvertFrom-Json)
        }
    }
    finally {
        if ($null -ne $response) { $response.Dispose() }
        $request.Dispose()
    }
}

function Assert-Demo {
    param([bool]$Condition, [string]$Message)
    if (-not $Condition) { throw "Demonstracao interrompida: $Message" }
}

try {
    $status = Invoke-DemoRequest GET '/api/status' 200
    Assert-Demo ($status.aplicacao -eq 'OrderManagementApi' -and $status.status -eq 'online') 'API esperada nao esta online.'
    Write-Host '1/6 API online.'

    # Cada execucao cria seus proprios registros; nao depende dos IDs do seed.
    $suffix = [Guid]::NewGuid().ToString('N')
    $customer = Invoke-DemoRequest POST '/api/clientes' 201 @{
        nome = 'Cliente Demo'
        email = "demo.$suffix@example.com"
        ativo = $true
    }
    $product = Invoke-DemoRequest POST '/api/produtos' 201 @{
        nome = "Produto Demo $suffix"
        preco = 100.00
        ativo = $true
    }
    Write-Host "2/6 Cliente $($customer.id) e produto $($product.id) cadastrados."

    $order = Invoke-DemoRequest POST '/api/pedidos' 201 @{
        clienteId = $customer.id
        itens = @(@{ produtoId = $product.id; quantidade = 2 })
    }
    $orderPath = "/api/pedidos/$($order.id)"
    Assert-Demo ($order.status -eq 'AguardandoPagamento' -and [decimal]$order.subtotal -eq 200) 'Pedido deveria iniciar aguardando pagamento, com subtotal 200.'
    Write-Host "3/6 Pedido $($order.id) criado: 2 unidades de 100, subtotal 200."

    $null = Invoke-DemoRequest PUT "/api/produtos/$($product.id)" 204 @{
        nome = $product.nome
        preco = 199.90
        ativo = $true
    }
    $catalog = Invoke-DemoRequest GET "/api/produtos/$($product.id)" 200
    $snapshot = Invoke-DemoRequest GET $orderPath 200
    Assert-Demo ([decimal]$catalog.preco -eq 199.90) 'Preco do catalogo nao foi atualizado.'
    Assert-Demo (@($snapshot.itens).Count -eq 1 -and [decimal]$snapshot.itens[0].precoUnitario -eq 100 -and [decimal]$snapshot.subtotal -eq 200) 'Pedido antigo deveria preservar o preco 100.'
    Write-Host '4/6 Catalogo atualizado para 199,90; pedido preservou preco 100 e subtotal 200.'

    $paid = Invoke-DemoRequest POST "$orderPath/pagamento" 200 @{ tipo = 'Cartao' }
    Assert-Demo ($paid.status -eq 'Pago' -and $paid.pagamento.tipo -eq 'Cartao' -and [decimal]$paid.pagamento.valorBase -eq 200 -and [decimal]$paid.pagamento.taxa -eq 6 -and [decimal]$paid.pagamento.valorFinal -eq 206) 'Pagamento deveria registrar base 200, taxa 6 e total 206.'
    $null = Invoke-DemoRequest POST "$orderPath/pagamento" 409 @{ tipo = 'Pix' }
    $null = Invoke-DemoRequest PATCH "$orderPath/status" 409 @{ status = 'Entregue' }
    $unchanged = Invoke-DemoRequest GET $orderPath 200
    Assert-Demo ($unchanged.status -eq 'Pago' -and $unchanged.pagamento.tipo -eq 'Cartao' -and [decimal]$unchanged.pagamento.valorFinal -eq 206) 'Recusas deveriam preservar status e pagamento.'
    Write-Host '5/6 Cartao: taxa 6, total 206. Segundo pagamento e entrega antes do envio recusados com HTTP 409.'

    $sent = Invoke-DemoRequest PATCH "$orderPath/status" 200 @{ status = 'Enviado' }
    Assert-Demo ($sent.status -eq 'Enviado') 'Pedido deveria estar enviado.'
    $null = Invoke-DemoRequest PATCH "$orderPath/status" 200 @{ status = 'Entregue' }
    $delivered = Invoke-DemoRequest GET $orderPath 200
    Assert-Demo ($delivered.status -eq 'Entregue' -and [decimal]$delivered.subtotal -eq 200 -and [decimal]$delivered.pagamento.valorFinal -eq 206) 'Consulta final deveria confirmar entrega e valores originais.'
    Write-Host '6/6 Envio e entrega confirmados por nova consulta.'
    Write-Host 'Demonstracao concluida. Os registros criados permanecem no banco de demonstracao.'

    [pscustomobject]@{
        ClienteId = $customer.id
        ProdutoId = $product.id
        PedidoId = $order.id
        StatusFinal = $delivered.status
        PrecoAtualCatalogo = $catalog.preco
        PrecoNaCompra = $delivered.itens[0].precoUnitario
        Subtotal = $delivered.subtotal
        Taxa = $delivered.pagamento.taxa
        ValorFinal = $delivered.pagamento.valorFinal
    }
}
finally {
    $demoClient.Dispose()
}
