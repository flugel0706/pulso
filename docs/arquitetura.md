# Arquitetura

## Visão geral

```text
┌───────────────────────────── Pulso.exe ─────────────────────────────┐
│                                                                      │
│  UI thread (WinForms)              Player thread (background)        │
│  ┌──────────────────────┐          ┌──────────────────────────┐      │
│  │ MainForm             │          │ Player                   │      │
│  │  ├─ linha do tempo   │          │  ├─ Stopwatch            │      │
│  │  ├─ HUD / mini bar   │          │  ├─ speed / loops        │      │
│  │  ├─ menus / tray     │          │  └─ SendInput / PostMsg  │      │
│  │  └─ PlayerForm       │          └──────────────────────────┘      │
│  └─────────┬────────────┘                     ▲                      │
│            │                                  │ Macro + Settings      │
│  ┌─────────▼────────────┐                     │                      │
│  │ InputEngine          │   eventos           │                      │
│  │  ├─ WH_KEYBOARD_LL   │──────────────► Macro (List<MacroEvent>)    │
│  │  └─ WH_MOUSE_LL      │                                          │
│  └──────────────────────┘                                          │
│                                                                      │
│  Settings ←→ Pulso.config.json     SelfExe ←→ trailer PULSOTSK       │
└──────────────────────────────────────────────────────────────────────┘
```

## Fluxo de gravação

1. `MainForm.ToggleRecording` chama `InputEngine.StartRecording(vkDoAtalho)`.
2. O `InputEngine` instala hooks globais (`WH_KEYBOARD_LL` e `WH_MOUSE_LL`) com
   `SetWindowsHookEx` na thread da UI.
3. Cada callback converte a mensagem nativa em `MacroEvent` com timestamp do
   `Stopwatch` e adiciona à lista sob lock.
4. Eventos dentro da janela do Pulso são descartados quando `IgnoreOwnWindow` está ativo.
5. A tecla do atalho é suprimida para não entrar na macro.
6. Ao parar, o `MainForm` recebe a `Macro` pronta, atualiza a grade e o estado.

## Fluxo de reprodução

1. `Player.Start(macro, settings)` cria uma thread `IsBackground` chamada `Pulso.Player`.
2. A thread percorre os eventos ordenados, esperando a diferença de tempo escalada
   por `SpeedPercent` (ou sem espera no modo `Instant`).
3. Cada evento é injetado via `SendInput` (scan code ou virtual key) ou
   `PostMessage` quando o destino é a janela gravada.
4. `LoopProgress` e `Status` reportam à UI; `Pause` e `Stop` são flags voláteis.
5. `F12` é tratado no hook e dispara a parada de emergência em qualquer estado.

## Fluxo de compilação

1. `Program.Compile` carrega a macro (`.gmt` ou `.rec`).
2. Serializa `CompiledPayload { Macro, Play }` em JSON.
3. `SelfExe.CopySelfWithoutTrailer` copia o executável atual até `dataEnd`.
4. `SelfExe.AppendTrailer` anexa payload + tamanho + `PULSOTSK`.
5. `SelfExe.ZeroPeChecksum` zera o checksum PE.
6. Ao abrir o `.exe`, `Program.Main` lê o trailer, desserializa o payload e abre
   `PlayerForm`.

## Mapa de arquivos

| Arquivo | Responsabilidade |
|---|---|
| `Program.cs` | Entry point, CLI, selftest, demo, compilação |
| `MainForm.cs` | Estúdio: menus, sidebar, linha do tempo, HUD, tray, atalhos |
| `PlayerForm.cs` | Janela do player usada por macros compiladas e `--play` |
| `Player.cs` | Motor de reprodução em thread dedicada |
| `InputEngine.cs` | Hooks globais de teclado e mouse |
| `Macro.cs` | Modelo da macro, JSON, import/export `.rec` |
| `MacroEvent.cs` | Evento individual e textos de exibição |
| `MacroPackage.cs` | Pacote `.pulso` |
| `CompiledPayload.cs` | Payload embutido no `.exe` |
| `SelfExe.cs` | Trailer `PULSOTSK`, clonagem e checksum PE |
| `Settings.cs` | Configurações, saneamento e persistência |
| `SettingsForm.cs` | UI de configurações e captura de atalhos |
| `PlaybackPreset.cs` | Preset de reprodução do `.pulso` |
| `Hotkey.cs` | Parse/format de atalhos e nomes de VK |
| `HotkeyDialog.cs` | Captura de combinação de atalho |
| `Native.cs` | P/Invoke: hooks, `SendInput`, janelas, métricas |
| `EventEditDialog.cs` | Edição de um evento |
| `NumericPrompt.cs` | Entrada numérica (escala, pausa) |
| `Selftest.cs` | 15 testes internos |
| `AppLog.cs` | Log em arquivo |
| `Paths.cs` | Diretório portátil e caminhos |
| `Backdrop.cs`, `BackdropPanel.cs` | Foto de fundo: carga, recorte e composição por painel, sem cache de janela |
| `MiniBar.cs` | Barra compacta: arte de fundo, leituras, controles e menu de contexto |
| `HeroStrip.cs` | Faixa de operação com macro, eventos, duração e estado |
| `Theme.cs`, `HudScrollBar.cs` | Passes de transparência e barra de rolagem do HUD |
| `AboutForm.cs` | Diálogo Sobre no idioma visual do estúdio |
| `DarkUi.cs`, `DarkRenderer.cs`, `DarkColorTable.cs` | Tema escuro |
| `Palette.cs`, `UiFont.cs`, `AppIcon.cs`, `BrandMark.cs` | Cores, fontes, ícones |
| `Hud.cs`, `HudButton.cs`, `HudChrome.cs`, `HudGrid.cs`, `HudKind.cs` | Componentes do HUD |
| `PulseCheck.cs`, `StatusDot.cs`, `VectorIcon.cs`, `ButtonIcon.cs`, `CardPanel.cs` | Controles customizados |
| `ModernWindow.cs` | Chrome de janela sem borda |
| `PulseCheck.cs` | Checkbox desenhado à mão |

## Decisões de projeto

- **Sem dependências externas:** apenas BCL + WinForms. Não há NuGet em runtime.
- **Portátil por padrão:** configuração ao lado do executável, com fallback para `%APPDATA%`.
- **JSON legível:** macros podem ser editadas à mão e versionadas em Git.
- **Compilação por trailer:** evita gerar um novo assembly; o executável é o próprio Pulso.
- **Threads separadas:** a reprodução nunca bloqueia a UI; os hooks rodam na thread
  com message loop, garantindo entrega dos eventos.
- **Atalho de emergência independente:** `F12` é verificado no hook, antes de qualquer
  processamento, e sinaliza parada global.

## Limitações conhecidas

- Hooks de baixo nível não recebem eventos de janelas elevadas quando o Pulso não está elevado.
- `SendInput` pode ser bloqueado por anti-cheat.
- `.rec` não representa roda do mouse nem botões laterais.
- Coordenadas absolutas dependem da mesma configuração de monitores/DPI.
