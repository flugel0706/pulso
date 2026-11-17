# Contribuindo

Obrigado pelo interesse em melhorar o Pulso.

## Preparando o ambiente

1. Windows 10/11 x64.
2. [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0).
3. Clone o repositório:

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

## Padrões do código

- C# 11, .NET 8, WinForms, **sem dependências NuGet em runtime**.
- Namespace único `Pulso`, arquivos com uma responsabilidade clara.
- UI, mensagens e comentários voltados ao usuário em português.
- Não introduza telemetria, rede ou coleta de dados.
- Mudanças em P/Invoke devem manter compatibilidade x64 e tratar erros.
- Preserve o comportamento portátil (configuração ao lado do executável).

## O que testar antes de abrir um PR

- [ ] `dotnet build -c Release` sem erros.
- [ ] `.\scripts\build.ps1` (publica e roda o selftest).
- [ ] `--selftest` com 12/12.
- [ ] `--smoke` com `resultado=ok`.
- [ ] Gravação e reprodução manual de uma macro curta.
- [ ] Exportação `.pulso` e compilação `.exe` funcionando.

## Commits e PRs

- Commits em português ou inglês, no imperativo e com escopo:
  `player: corrige pausa durante repetição contínua`.
- Um PR por assunto. Descreva o problema, a solução e como testou.
- Inclua captura de tela para mudanças visuais.
- Atualize `CHANGELOG.md` quando a mudança for visível ao usuário.

## Áreas que aceitam contribuição

- Compatibilidade com layouts de teclado (ABNT2, internacionais).
- Melhor suporte a múltiplos monitores e DPI.
- Novos formatos de importação (ex.: macros de outras ferramentas).
- Acessibilidade e navegação por teclado.
- Testes adicionais no selftest.
- Documentação e traduções.

## Reportando bugs

Use o template de bug e inclua:

- versão do Pulso (`--version`);
- versão do Windows;
- passos para reproduzir;
- comportamento esperado e observado;
- trecho relevante de `Pulso.log`;
- se possível, a macro `.gmt` que reproduz o problema.

