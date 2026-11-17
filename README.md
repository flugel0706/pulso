<div align="center">

<img src="assets/icon.png" width="96" alt="Pulso">

# Pulso

**EstÃºdio de automaÃ§Ã£o para Windows. Grave, edite, reproduza e compile macros.**

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](#requisitos)
[![Windows 10/11](https://img.shields.io/badge/Windows-10%20%7C%2011-0078D6?logo=windows&logoColor=white)](#requisitos)
[![x64](https://img.shields.io/badge/arquitetura-x64-64748b)](#requisitos)
[![LicenÃ§a MIT](https://img.shields.io/badge/licen%C3%A7a-MIT-22c55e)](LICENSE)

[Download](#download) Â· [Recursos](#recursos) Â· [Atalhos](#atalhos) Â· [Formatos](#formatos-de-arquivo) Â· [Compilar](#compilar-do-c%C3%B3digo-fonte) Â· [FAQ](docs/faq.md)

<img src="assets/screenshot.png" alt="Interface do Pulso" width="880">

</div>

---

**Pulso** Ã© um gravador e reprodutor de macros para Windows â€” uma evoluÃ§Ã£o do conceito do TinyTask, com interface moderna, linha do tempo editÃ¡vel, atalhos globais configurÃ¡veis, presets de reproduÃ§Ã£o e compilaÃ§Ã£o da macro em um `.exe` autÃ´nomo.

Grava mouse e teclado por **hooks de baixo nÃ­vel** (eventos reais, sem perder cliques), reproduz com controle total de velocidade, repetiÃ§Ãµes, atrasos e janela de destino, e ainda importa/exporta o formato `.rec` do TinyTask.

## Download

A versÃ£o pronta fica na pÃ¡gina de **[Releases](../../releases/latest)**.

| EdiÃ§Ã£o | Arquivo | Tamanho | Requisito |
|---|---|---|---|
| PortÃ¡til | `Pulso.exe` | ~430 KB | [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) |
| Autocontida | `Pulso-selfcontained.exe` | ~70 MB | Nenhum â€” sÃ³ Windows x64 |

> NÃ£o sabe qual escolher? Se o `Pulso.exe` nÃ£o abrir, use a ediÃ§Ã£o **autocontida**.
> Cada release publica tambÃ©m o `SHA256SUMS.txt` para conferÃªncia de integridade.

**Verificar o hash (PowerShell):**

```powershell
Get-FileHash .\Pulso.exe -Algorithm SHA256
```

## Requisitos

- Windows 10 ou Windows 11, x64
- .NET 8 Desktop Runtime (apenas na ediÃ§Ã£o portÃ¡til)
- Para compilar do cÃ³digo-fonte: [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0) e Windows

## Recursos

### GravaÃ§Ã£o
- Captura global de **mouse e teclado** via `SetWindowsHookEx` (eventos reais, timestamps em ms)
- GravaÃ§Ã£o de movimento, cliques, roda e botÃµes laterais (X1/X2)
- Teclas por **scan code** ou virtual key (melhor compatibilidade com jogos)
- Ignora cliques dentro da prÃ³pria janela do Pulso
- Registra a janela de destino (handle + posiÃ§Ã£o) para reproduzir no mesmo lugar
- Atalho global para iniciar/parar a gravaÃ§Ã£o

### ReproduÃ§Ã£o
- Velocidade de **5% a 2000%**
- RepetiÃ§Ãµes de **1 a 100.000** ou modo **contÃ­nuo**
- Atraso entre ciclos e atraso inicial configurÃ¡veis
- Modo **instantÃ¢neo** (sem intervalos)
- Pausa/retomada em qualquer ponto
- ReproduÃ§Ã£o **relativa Ã  janela gravada**
- Envio de teclas direto Ã  janela de destino (`PostMessage`)
- **F12** interrompe tudo, sempre â€” emergÃªncia global
- Player dedicado ao abrir uma macro compilada

### EdiÃ§Ã£o
- Linha do tempo com colunas de tempo, evento e detalhe
- Editar evento individual, remover selecionados, limpar tudo
- Inserir pausa a partir de um ponto
- Ajustar escala de tempo da macro inteira
- Remover espera inicial
- Arrastar e soltar arquivos `.gmt`, `.pulso`, `.rec` e `.json` na janela

### Interface
- Tema escuro, HUD compacto e barra mini
- Sempre no topo (opcional), bandeja do sistema, tela cheia
- PersistÃªncia portÃ¡til: configuraÃ§Ã£o e log ao lado do executÃ¡vel
  (ou em `%APPDATA%\Pulso` quando a pasta nÃ£o Ã© gravÃ¡vel)
- Testes internos (`--selftest`) e smoke test de UI (`--smoke`)

## Atalhos

Atalhos globais (funcionam com o Pulso em segundo plano) â€” todos configurÃ¡veis em **Ferramentas â†’ ConfiguraÃ§Ãµes**:

| AÃ§Ã£o | PadrÃ£o |
|---|---|
| Gravar / parar gravaÃ§Ã£o | `Ctrl+Alt+R` |
| Reproduzir | `Ctrl+Alt+P` |
| Pausar / continuar | `Ctrl+Alt+EspaÃ§o` |
| Parar tudo | `Ctrl+Alt+S` |
| Modo compacto | `Ctrl+Alt+M` |
| **Parada de emergÃªncia** | **F12** (sempre ativa) |

Atalhos locais da janela:

| AÃ§Ã£o | Tecla |
|---|---|
| Gravar / parar | `F9` |
| Reproduzir | `F5` |
| Nova macro | `Ctrl+N` |
| Abrir | `Ctrl+O` |
| Salvar | `Ctrl+S` |
| Salvar como | `Ctrl+Shift+S` |
| Exportar macro pronta (`.pulso`) | `Ctrl+E` |
| ConfiguraÃ§Ãµes | `Ctrl+,` |
| Tela cheia | `F11` |

## InÃ­cio rÃ¡pido

1. Baixe `Pulso.exe` e execute (nÃ£o precisa instalar).
2. Aperte **`Ctrl+Alt+R`** e execute a sequÃªncia que deseja automatizar.
3. Aperte **`Ctrl+Alt+R`** de novo para encerrar a gravaÃ§Ã£o.
4. Ajuste velocidade, repetiÃ§Ãµes e atraso na barra inferior.
5. Aperte **`F5`** (ou `Ctrl+Alt+P`) para reproduzir.
6. Para compartilhar a automaÃ§Ã£o configurada: **Arquivo â†’ Exportar macro prontaâ€¦** (`.pulso`).
7. Para gerar um executÃ¡vel independente, use a CLI:

   ```powershell
   Pulso.exe --compile minha-macro.gmt minha-macro.exe
   ```

## Linha de comando

```text
Pulso.exe                          abre o estÃºdio
Pulso.exe --version                mostra a versÃ£o
Pulso.exe --help                   mostra a ajuda
Pulso.exe --selftest               executa os testes internos (selftest.txt)
Pulso.exe --smoke                  teste rÃ¡pido de criaÃ§Ã£o das janelas
Pulso.exe --demo <arquivo.gmt>     gera uma automaÃ§Ã£o de demonstraÃ§Ã£o
Pulso.exe --compile <in> <out.exe> compila uma macro em executÃ¡vel
Pulso.exe --play <arquivo>         reproduz uma macro sem abrir a interface
Pulso.exe <arquivo>                abre .gmt, .rec, .pulso ou .json direto
```

CÃ³digos de saÃ­da: `0` sucesso Â· `1` falha Â· `2` uso incorreto.

VariÃ¡vel `PULSO_PROBE=1` grava um relatÃ³rio de diagnÃ³stico (`probe.txt`) â€” Ãºtil para inspecionar executÃ¡veis compilados.

## Formatos de arquivo

| ExtensÃ£o | Papel | DescriÃ§Ã£o |
|---|---|---|
| `.gmt` | Macro nativa | JSON com metadados da tela e lista de eventos |
| `.pulso` | Macro pronta | Pacote com macro + preset de reproduÃ§Ã£o + nome/duraÃ§Ã£o |
| `.rec` | Compatibilidade | Formato binÃ¡rio do TinyTask (importaÃ§Ã£o e exportaÃ§Ã£o) |
| `.exe` | Macro compilada | CÃ³pia do Pulso com o JSON embutido em um trailer `PULSOTSK` |

Detalhes completos dos schemas em **[docs/formatos.md](docs/formatos.md)**.

## Compilar do cÃ³digo-fonte

```powershell
# Build de desenvolvimento
dotnet build src/Pulso/Pulso.csproj -c Release

# Publicar executÃ¡vel Ãºnico portÃ¡til (~430 KB, requer .NET 8 Desktop Runtime)
dotnet publish src/Pulso/Pulso.csproj -c Release -r win-x64 `
  --self-contained false -p:PublishSingleFile=true -o dist

# Publicar ediÃ§Ã£o autocontida (sem dependÃªncias, ~70 MB)
dotnet publish src/Pulso/Pulso.csproj -c Release -r win-x64 `
  --self-contained true -p:PublishSingleFile=true `
  -p:EnableCompressionInSingleFile=true -o dist/self-contained
```

Ou use o script pronto:

```powershell
.\scripts\build.ps1                 # portÃ¡til + testes
.\scripts\build.ps1 -SelfContained  # tambÃ©m gera a ediÃ§Ã£o autocontida
```

ValidaÃ§Ã£o:

```powershell
.\dist\Pulso.exe --selftest   # 12 testes internos
.\dist\Pulso.exe --smoke      # criaÃ§Ã£o das janelas
```

## Estrutura do projeto

```text
pulso/
â”œâ”€ src/Pulso/            cÃ³digo-fonte C# (WinForms, .NET 8)
â”‚  â”œâ”€ Program.cs         entrada, CLI, selftest, compilaÃ§Ã£o de macro
â”‚  â”œâ”€ MainForm.cs        estÃºdio: linha do tempo, HUD, menus, tray
â”‚  â”œâ”€ InputEngine.cs     hooks globais de mouse/teclado
â”‚  â”œâ”€ Player.cs          motor de reproduÃ§Ã£o em thread dedicada
â”‚  â”œâ”€ Macro.cs           modelo, JSON, import/export .rec
â”‚  â”œâ”€ SelfExe.cs         trailer PULSOTSK e clonagem do executÃ¡vel
â”‚  â”œâ”€ Settings.cs        configuraÃ§Ãµes e atalhos
â”‚  â””â”€ ...                UI, tema, P/Invoke e utilitÃ¡rios
â”œâ”€ docs/                 uso, formatos, compilaÃ§Ã£o, arquitetura e FAQ
â”œâ”€ scripts/              build e publicaÃ§Ã£o no GitHub
â”œâ”€ examples/             macro de demonstraÃ§Ã£o
â”œâ”€ assets/               Ã­cone e screenshot
â””â”€ .github/workflows/    CI (build + release automÃ¡tica por tag)
```

Mais detalhes em **[docs/arquitetura.md](docs/arquitetura.md)**.

## Aviso de seguranÃ§a

O Pulso usa hooks globais de teclado/mouse e injeÃ§Ã£o de entrada (`SendInput`/`PostMessage`) â€” as mesmas APIs de qualquer ferramenta de automaÃ§Ã£o. Alguns antivÃ­rus podem sinalizar executÃ¡veis compilados por heurÃ­stica; o cÃ³digo Ã© aberto e auditÃ¡vel. Use apenas em sistemas e aplicaÃ§Ãµes onde vocÃª tem autorizaÃ§Ã£o. Alguns jogos com anti-cheat bloqueiam entrada sintÃ©tica por design.

## Contribuindo

Leia **[CONTRIBUTING.md](CONTRIBUTING.md)**. Para vulnerabilidades, veja **[SECURITY.md](SECURITY.md)**.

## LicenÃ§a

[MIT](LICENSE) Â© 2026 Pulso Studio

