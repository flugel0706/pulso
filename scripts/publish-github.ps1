#Requires -Version 5.1
<#
.SYNOPSIS
    Publica o repositório do Pulso no GitHub e cria a release.

.DESCRIPTION
    Requer GitHub CLI autenticado (gh auth login). O script:
      1. valida a autenticação;
      2. inicializa o repositório git, se necessário;
      3. cria o repositório remoto (ou usa o origin existente);
      4. configura descrição e tópicos;
      5. faz push da branch main;
      6. cria a tag e a release com os executáveis e SHA256SUMS.txt.

.PARAMETER RepoName
    Nome do repositório. Padrão: pulso.

.PARAMETER Visibility
    public (padrão) ou private.

.PARAMETER Tag
    Tag da release. Padrão: v3.1.0.

.PARAMETER Description
    Descrição do repositório no GitHub.

.PARAMETER SkipRelease
    Não cria tag/release; apenas publica o código.

.EXAMPLE
    .\scripts\publish-github.ps1

.EXAMPLE
    .\scripts\publish-github.ps1 -RepoName meu-pulso -Visibility private
#>
[CmdletBinding()]
param(
    [string]$RepoName = 'pulso',

    [ValidateSet('public', 'private')]
    [string]$Visibility = 'public',

    [string]$Tag = 'v3.1.0',

    [string]$Description = 'Estúdio de automação para Windows: grave, edite, reproduza e compile macros. Alternativa aprimorada ao TinyTask.',

    [switch]$SkipRelease
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$script:ghLastExit = 0

function Invoke-Gh {
    <#
        Executa o gh capturando stdout/stderr sem transformar stderr em exceção
        (PowerShell 5.1 converte NativeCommandError quando ErrorActionPreference = Stop).
    #>
    param([string[]]$Arguments)

    $previous = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    try {
        $output = & $script:gh @Arguments 2>&1
        $script:ghLastExit = $LASTEXITCODE
        return $output
    }
    finally {
        $ErrorActionPreference = $previous
    }
}

Push-Location $root
try {
    # ---------------------------------------------------------------- gh
    $ghCommand = Get-Command gh -ErrorAction SilentlyContinue
    $script:gh = if ($ghCommand) { $ghCommand.Source } else { $null }
    if (-not $script:gh) {
        $candidate = Join-Path $env:ProgramFiles 'GitHub CLI/gh.exe'
        if (Test-Path $candidate) { $script:gh = $candidate }
    }
    if (-not $script:gh) {
        throw 'GitHub CLI não encontrado. Instale com: winget install GitHub.cli'
    }

    $null = Invoke-Gh @('auth', 'status')
    if ($script:ghLastExit -ne 0) {
        throw "GitHub CLI sem autenticação. Rode: & '$script:gh' auth login"
    }

    $login = ((Invoke-Gh @('api', 'user', '--jq', '.login')) | Out-String).Trim()
    if ($script:ghLastExit -ne 0 -or -not $login) {
        throw 'Não foi possível obter o usuário do GitHub.'
    }
    Write-Host "==> Autenticado como $login" -ForegroundColor Cyan

    # ---------------------------------------------------------------- git
    if (-not (Test-Path (Join-Path $root '.git'))) {
        Write-Host "==> Inicializando repositório git" -ForegroundColor Cyan
        git init -b main | Out-Null
    }

    $userName = (git config user.name) 2>$null
    $userEmail = (git config user.email) 2>$null
    if (-not $userName) {
        git config user.name $login
        Write-Host "    user.name definido como $login"
    }
    if (-not $userEmail) {
        git config user.email "$login@users.noreply.github.com"
        Write-Host "    user.email definido como $login@users.noreply.github.com"
    }

    # Commit de tudo que ainda não está versionado
    git add -A
    git diff --cached --quiet
    if ($LASTEXITCODE -ne 0) {
        $message = if ($SkipRelease) { 'Publica o projeto Pulso' } else { "Pulso $Tag" }
        git commit -m $message | Out-Null
        Write-Host "==> Commit criado: $message" -ForegroundColor Cyan
    }

    # ---------------------------------------------------------------- remote
    $remotes = @(git remote)
    if ($remotes -notcontains 'origin') {
        Write-Host "==> Criando repositório $login/$RepoName ($Visibility)" -ForegroundColor Cyan
        $result = Invoke-Gh @('repo', 'create', "$login/$RepoName", "--$Visibility", '--source', '.', '--remote', 'origin', '--push', '--description', $Description)
        $result | ForEach-Object { Write-Host $_ }
        if ($script:ghLastExit -ne 0) { throw 'Falha ao criar o repositório remoto.' }
    }
    else {
        Write-Host "==> Enviando para origin" -ForegroundColor Cyan
        git push -u origin main
        if ($LASTEXITCODE -ne 0) { throw 'Falha no git push.' }
    }

    # ---------------------------------------------------------------- metadados
    Write-Host "==> Configurando descrição e tópicos" -ForegroundColor Cyan
    $topics = @(
        'macro', 'macro-recorder', 'automation', 'windows', 'winforms',
        'csharp', 'dotnet', 'tinytask', 'gui-automation', 'input-automation'
    )
    $editArgs = @('repo', 'edit', "$login/$RepoName", '--description', $Description)
    foreach ($topic in $topics) { $editArgs += @('--add-topic', $topic) }
    $null = Invoke-Gh $editArgs

    if ($SkipRelease) {
        Write-Host ""
        Write-Host "Repositório publicado: https://github.com/$login/$RepoName" -ForegroundColor Green
        return
    }

    # ---------------------------------------------------------------- release
    $dist = Join-Path $root 'dist'
    $exe = Join-Path $dist 'Pulso.exe'
    $scExe = Join-Path $dist 'self-contained/Pulso.exe'
    if (-not (Test-Path $exe) -or -not (Test-Path $scExe)) {
        Write-Host "==> Gerando executáveis (portátil + autocontido)" -ForegroundColor Cyan
        & (Join-Path $PSScriptRoot 'build.ps1') -SelfContained
    }

    $assets = @($exe)
    if (Test-Path $scExe) {
        $scCopy = Join-Path $dist 'Pulso-selfcontained.exe'
        Copy-Item $scExe $scCopy -Force
        $assets += $scCopy
    }

    $sums = Join-Path $dist 'SHA256SUMS.txt'
    $hashLines = foreach ($file in $assets) {
        $hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $file).Hash.ToLowerInvariant()
        "$hash  $([System.IO.Path]::GetFileName($file))"
    }
    $hashLines | Set-Content $sums -Encoding ASCII
    $assets += $sums

    # Tag
    $tagExists = (git tag --list $Tag) -contains $Tag
    if (-not $tagExists) {
        git tag -a $Tag -m "Pulso $Tag"
        git push origin $Tag
        if ($LASTEXITCODE -ne 0) { throw 'Falha ao enviar a tag.' }
        Write-Host "==> Tag $Tag criada" -ForegroundColor Cyan
    }

    # Release
    $null = Invoke-Gh @('release', 'view', $Tag, '--repo', "$login/$RepoName")
    if ($script:ghLastExit -eq 0) {
        Write-Host "==> Release $Tag já existe; atualizando assets" -ForegroundColor Cyan
        $result = Invoke-Gh (@('release', 'upload', $Tag, '--repo', "$login/$RepoName", '--clobber') + $assets)
    }
    else {
        Write-Host "==> Criando release $Tag" -ForegroundColor Cyan
        $result = Invoke-Gh (@('release', 'create', $Tag, '--repo', "$login/$RepoName",
                '--title', "Pulso $Tag", '--generate-notes', '--latest') + $assets)
    }
    $result | ForEach-Object { Write-Host $_ }
    if ($script:ghLastExit -ne 0) { throw 'Falha ao criar a release.' }

    Write-Host ""
    Write-Host "Repositório: https://github.com/$login/$RepoName" -ForegroundColor Green
    Write-Host "Release:     https://github.com/$login/$RepoName/releases/tag/$Tag" -ForegroundColor Green
}
finally {
    Pop-Location
}
