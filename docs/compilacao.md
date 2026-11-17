# CompilaÃ§Ã£o

## Requisitos

- Windows 10/11 x64
- [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0) (o `global.json` fixa a famÃ­lia 8.0)
- Git (opcional, para clonar)

```powershell
git clone https://github.com/<seu-usuario>/pulso.git
cd pulso
```

## Build de desenvolvimento

```powershell
dotnet build src/Pulso/Pulso.csproj -c Release
```

SaÃ­da em `src/Pulso/bin/x64/Release/net8.0-windows/win-x64/`.
Esse build Ã© *framework-dependent*: o `Pulso.exe` precisa do `Pulso.dll` ao lado.

## PublicaÃ§Ã£o

### EdiÃ§Ã£o portÃ¡til (single-file, ~430 KB)

```powershell
dotnet publish src/Pulso/Pulso.csproj -c Release -r win-x64 `
  --self-contained false `
  -p:PublishSingleFile=true `
  -o dist
```

Requer o .NET 8 Desktop Runtime na mÃ¡quina de destino. Ã‰ a mesma estratÃ©gia da
distribuiÃ§Ã£o original: apphost + assembly gerenciado em um Ãºnico arquivo.

### EdiÃ§Ã£o autocontida (~70 MB)

```powershell
dotnet publish src/Pulso/Pulso.csproj -c Release -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:EnableCompressionInSingleFile=true `
  -o dist/self-contained
```

NÃ£o requer runtime instalado. A compressÃ£o de single-file sÃ³ Ã© permitida em
publicaÃ§Ãµes autocontidas (`NETSDK1176` se usada com `--self-contained false`).

### Script pronto

```powershell
.\scripts\build.ps1                  # portÃ¡til + selftest
.\scripts\build.ps1 -SelfContained   # tambÃ©m gera a ediÃ§Ã£o autocontida
.\scripts\build.ps1 -SkipTests       # sÃ³ publica
```

O script grava `dist/SHA256SUMS.txt` com os hashes dos executÃ¡veis.

## Testes

O Pulso tem testes embutidos, sem framework externo:

```powershell
.\dist\Pulso.exe --selftest
Get-Content .\dist\selftest.txt
```

Cobertura dos 12 testes:

1. Parse de atalho com acento (`Ctrl+Alt+EspaÃ§o`)
2. Round-trip de texto de atalho
3. Atalho `Shift+F5`
4. Atalho invÃ¡lido permanece invÃ¡lido
5. Nome de tecla `F12`
6. Round-trip JSON de macro
7. Exportar/importar `.rec`
8. Escrever/ler trailer `PULSOTSK`
9. Zerar checksum PE
10. Tamanho de `INPUT` para `SendInput` (40 bytes em x64)
11. Round-trip de configuraÃ§Ã£o
12. Round-trip de pacote `.pulso`

Smoke test de UI (cria as janelas sem interaÃ§Ã£o):

```powershell
.\dist\Pulso.exe --smoke
Get-Content .\dist\smoke.txt
```

## IntegraÃ§Ã£o contÃ­nua

### `build.yml`

Executa em todo push/PR:

1. `dotnet restore` e `dotnet build -c Release`.
2. Publica a ediÃ§Ã£o portÃ¡til single-file.
3. Roda `--selftest` e falha o job se algum teste falhar.
4. Sobe `Pulso.exe` como artefato do workflow.

### `release.yml`

Disparado por tags `v*`:

1. Publica as ediÃ§Ãµes portÃ¡til e autocontida.
2. Roda o selftest.
3. Gera `SHA256SUMS.txt`.
4. Cria a release no GitHub com os dois executÃ¡veis anexados.

Para lanÃ§ar uma versÃ£o:

```powershell
git tag v3.0.0
git push origin v3.0.0
```

Atualize `Version`, `AssemblyVersion` e `FileVersion` em
`src/Pulso/Pulso.csproj` e registre a mudanÃ§a no `CHANGELOG.md` antes da tag.

## PublicaÃ§Ã£o manual no GitHub

```powershell
.\scripts\publish-github.ps1 -RepoName pulso
```

O script:

1. Confere `gh auth status`.
2. Cria o repositÃ³rio (se necessÃ¡rio) e configura descriÃ§Ã£o e tÃ³picos.
3. Faz commit e push da branch `main`.
4. Cria a tag `v3.0.0` e a release com `dist/Pulso.exe`,
   `dist/self-contained/Pulso.exe` e `SHA256SUMS.txt`.

Requisitos: [GitHub CLI](https://cli.github.com/) autenticado (`gh auth login`) e
Git configurado com `user.name`/`user.email`.

## Assinatura de cÃ³digo (opcional)

ExecutÃ¡veis que usam hooks e injeÃ§Ã£o de entrada podem ser sinalizados por
heurÃ­stica de antivÃ­rus. Para distribuiÃ§Ã£o pÃºblica, assine o binÃ¡rio:

```powershell
signtool sign /fd SHA256 /tr http://timestamp.digicert.com /td SHA256 `
  /a dist/Pulso.exe
```

Assinatura evita alertas de "editor desconhecido", mas nÃ£o altera o comportamento
do programa.

