[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$envPath = Join-Path $repoRoot '.env'

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    throw 'Docker nao encontrado. Instale e inicie o Docker Desktop e reabra o terminal.'
}

Push-Location $repoRoot
try {
    $dockerOs = & docker info --format '{{.OSType}}'
    if ($LASTEXITCODE -ne 0) {
        throw 'O motor do Docker nao respondeu. Abra o Docker Desktop e aguarde a inicializacao.'
    }
    if (($dockerOs -join '').Trim() -ne 'linux') {
        throw 'Este projeto usa Linux containers. Selecione esse modo no Docker Desktop.'
    }

    if (-not (Test-Path -LiteralPath $envPath)) {
        $randomBytes = New-Object byte[] 32
        $generator = [System.Security.Cryptography.RandomNumberGenerator]::Create()
        try {
            $generator.GetBytes($randomBytes)
        }
        finally {
            $generator.Dispose()
        }
        $password = 'LocalDb!' + [Convert]::ToBase64String($randomBytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')
        [System.IO.File]::WriteAllLines(
            $envPath,
            @("SQLSERVER_SA_PASSWORD=$password", 'API_PORT=5211'),
            [System.Text.UTF8Encoding]::new($false))
        Write-Host 'Configuracao local criada em .env; a senha nao sera exibida.'
    }

    & docker compose config --quiet
    if ($LASTEXITCODE -ne 0) {
        throw 'Compose invalido. Confira o arquivo .env e a mensagem anterior.'
    }

    & docker compose up --build --detach
    if ($LASTEXITCODE -ne 0) {
        throw 'Os containers nao iniciaram. Confira os logs e se a porta escolhida esta livre.'
    }

    $apiPort = '5211'
    foreach ($line in (Get-Content -LiteralPath $envPath)) {
        if ($line -match '^\s*API_PORT\s*=\s*(\d+)\s*$') {
            $apiPort = $Matches[1]
        }
    }
    if ($env:API_PORT) {
        $apiPort = $env:API_PORT
    }
    $apiUrl = "http://localhost:$apiPort"
    $ready = $false
    for ($attempt = 0; $attempt -lt 60; $attempt++) {
        try {
            $null = Invoke-RestMethod "$apiUrl/api/status" -TimeoutSec 3
            $products = Invoke-RestMethod "$apiUrl/api/produtos" -TimeoutSec 3
            $ready = $true
            break
        }
        catch {
            Start-Sleep -Seconds 2
        }
    }
    if (-not $ready) {
        throw 'A API ainda nao respondeu. Consulte: docker compose logs api sqlserver'
    }

    & docker compose ps
    $productCount = @($products).Count
    Write-Host "API disponivel em $apiUrl; consulta ao banco retornou $productCount produtos."
}
finally {
    Pop-Location
}
