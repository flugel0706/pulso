#Requires -Version 5.1
<#
.SYNOPSIS
    Publica o Pulso em dist/ e roda os testes internos.

.DESCRIPTION
    Gera a edição portátil single-file (padrão) e, opcionalmente, a edição
    autocontida. Produz SHA256SUMS.txt e executa --selftest.

.PARAMETER Configuration
    Release (padrão) ou Debug.

.PARAMETER Output
    Diretório de saída. Padrão: dist na raiz do repositório.

.PARAMETER SelfContained
    Também gera a edição autocontida em <Output>/Pulso-selfcontained.exe.

.PARAMETER SkipTests
    Não executa o selftest após a publicação.

.EXAMPLE
    .\scripts\build.ps1

.EXAMPLE
    .\scripts\build.ps1 -SelfContained
#>
[CmdletBinding()]
param(
    [ValidateSet('Release', 'Debug')]
    [string]$Configuration = 'Release',

    [string]$Output = 'dist',

    [switch]$SelfContained,

    [switch]$SkipTests
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$project = Join-Path $root 'src/Pulso/Pulso.csproj'

if (-not (Test-Path $project)) {
    throw "Projeto não encontrado: $project"
}

$outDir = if ([System.IO.Path]::IsPathRooted($Output)) { $Output } else { Join-Path $root $Output }
New-Item -ItemType Directory -Path $outDir -Force | Out-Null

Write-Host "==> Publicando edição portátil single-file" -ForegroundColor Cyan
dotnet publish $project -c $Configuration -r win-x64 --self-contained false `
    -p:PublishSingleFile=true -p:DebugType=embedded -o $outDir
if ($LASTEXITCODE -ne 0) { throw "dotnet publish falhou (código $LASTEXITCODE)." }

$assets = @(Join-Path $outDir 'Pulso.exe')

if ($SelfContained) {
    $scDir = Join-Path $outDir 'self-contained'
    New-Item -ItemType Directory -Path $scDir -Force | Out-Null
    Write-Host "==> Publicando edição autocontida" -ForegroundColor Cyan
    dotnet publish $project -c $Configuration -r win-x64 --self-contained true `
        -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true `
        -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=embedded -o $scDir
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish autocontido falhou (código $LASTEXITCODE)." }
    $scExe = Join-Path $outDir 'Pulso-selfcontained.exe'
    Move-Item (Join-Path $scDir 'Pulso.exe') $scExe -Force
    Remove-Item $scDir -Recurse -Force
    $assets += $scExe
}

Write-Host "==> Gerando SHA256SUMS.txt" -ForegroundColor Cyan
$lines = foreach ($file in $assets) {
    $hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $file).Hash.ToLowerInvariant()
    $name = [System.IO.Path]::GetFileName($file)
    if ($name -eq 'Pulso.exe' -and $file -ne $assets[0]) { $name = 'Pulso-selfcontained.exe' }
    "$hash  $name"
}
$lines | Set-Content -Path (Join-Path $outDir 'SHA256SUMS.txt') -Encoding ASCII

if (-not $SkipTests) {
    Write-Host "==> Executando selftest" -ForegroundColor Cyan
    $exe = Join-Path $outDir 'Pulso.exe'
    $report = Join-Path $outDir 'selftest.txt'
    Remove-Item $report -ErrorAction SilentlyContinue

    $process = Start-Process -FilePath $exe -ArgumentList '--selftest' -PassThru -Wait
    if (Test-Path $report) {
        Get-Content $report | ForEach-Object { Write-Host $_ }
        $resultLine = Select-String -Path $report -Pattern 'Resultado:' | Select-Object -First 1
        if (-not $resultLine -or $resultLine.Line -notmatch '0 falharam') {
            throw "Selftest falhou. Veja $report"
        }
    }
    elseif ($process.ExitCode -ne 0) {
        throw "Selftest retornou código $($process.ExitCode) e não gerou relatório."
    }
}

Write-Host ""
Write-Host "Build concluído:" -ForegroundColor Green
Get-ChildItem $outDir -File | Select-Object Name, Length | Format-Table -AutoSize
