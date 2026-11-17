# Contribuindo

Obrigado pelo interesse em melhorar o Pulso.

## Preparando o ambiente

1. Windows 10/11 x64.
2. [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0).
3. Clone o repositÃ³rio:

   ```powershell
   git clone https://github.com/<seu-usuario>/pulso.git
   cd pulso
   ```

4. Build:

   ```powershell
   dotnet build src/Pulso/Pulso.csproj -c Release
   ```

5. Testes:

   ```powershell
   .\scripts\build.ps1
   ```

## PadrÃµes do cÃ³digo

- C# 11, .NET 8, WinForms, **sem dependÃªncias NuGet em runtime**.
- Namespace Ãºnico `Pulso`, arquivos com uma responsabilidade clara.
- UI, mensagens e comentÃ¡rios voltados ao usuÃ¡rio em portuguÃªs.
- NÃ£o introduza telemetria, rede ou coleta de dados.
- MudanÃ§as em P/Invoke devem manter compatibilidade x64 e tratar erros.
- Preserve o comportamento portÃ¡til (configuraÃ§Ã£o ao lado do executÃ¡vel).

## O que testar antes de abrir um PR

- [ ] `dotnet build -c Release` sem erros.
- [ ] `.\scripts\build.ps1` (publica e roda o selftest).
- [ ] `--selftest` com 12/12.
- [ ] `--smoke` com `resultado=ok`.
- [ ] GravaÃ§Ã£o e reproduÃ§Ã£o manual de uma macro curta.
- [ ] ExportaÃ§Ã£o `.pulso` e compilaÃ§Ã£o `.exe` funcionando.

## Commits e PRs

- Commits em portuguÃªs ou inglÃªs, no imperativo e com escopo:
  `player: corrige pausa durante repetiÃ§Ã£o contÃ­nua`.
- Um PR por assunto. Descreva o problema, a soluÃ§Ã£o e como testou.
- Inclua captura de tela para mudanÃ§as visuais.
- Atualize `CHANGELOG.md` quando a mudanÃ§a for visÃ­vel ao usuÃ¡rio.

## Ãreas que aceitam contribuiÃ§Ã£o

- Compatibilidade com layouts de teclado (ABNT2, internacionais).
- Melhor suporte a mÃºltiplos monitores e DPI.
- Novos formatos de importaÃ§Ã£o (ex.: macros de outras ferramentas).
- Acessibilidade e navegaÃ§Ã£o por teclado.
- Testes adicionais no selftest.
- DocumentaÃ§Ã£o e traduÃ§Ãµes.

## Reportando bugs

Use o template de bug e inclua:

- versÃ£o do Pulso (`--version`);
- versÃ£o do Windows;
- passos para reproduzir;
- comportamento esperado e observado;
- trecho relevante de `Pulso.log`;
- se possÃ­vel, a macro `.gmt` que reproduz o problema.

