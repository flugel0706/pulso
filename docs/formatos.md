# Formatos de arquivo

Todos os formatos textuais usam UTF-8 e JSON indentado com escape relaxado
(acentos e símbolos preservados). Campos desconhecidos são ignorados na leitura.

## `.gmt` — macro nativa

```json
{
  "App": "Pulso",
  "Version": 1,
  "Created": "2026-11-17 14:01:07",
  "ScreenW": 1920,
  "ScreenH": 1080,
  "Hwnd": 984340,
  "WinLeft": 210,
  "WinTop": 207,
  "Events": [
    {
      "T": 223,
      "Kind": 2,
      "X": 0,
      "Y": 0,
      "Button": 0,
      "Vk": 162,
      "Scan": 29,
      "Extended": false,
      "Wheel": 0,
      "Hwnd": 0
    },
    {
      "T": 928,
      "Kind": 1,
      "X": 0,
      "Y": 0,
      "Button": 0,
      "Vk": 67,
      "Scan": 46,
      "Extended": false,
      "Wheel": 0,
      "Hwnd": 0
    }
  ]
}
```

| Campo | Tipo | Significado |
|---|---|---|
| `App` | string | sempre `"Pulso"` |
| `Version` | int | versão do formato (atual `1`) |
| `Created` | string | data de criação `yyyy-MM-dd HH:mm:ss` |
| `ScreenW`, `ScreenH` | int | resolução no momento da gravação |
| `Hwnd` | int64 | janela em foco ao iniciar a gravação (0 se desconhecida) |
| `WinLeft`, `WinTop` | int | posição da janela gravada |
| `Events` | array | eventos ordenados por `T` |

### Evento (`MacroEvent`)

| Campo | Tipo | Significado |
|---|---|---|
| `T` | int64 | tempo acumulado em ms desde o início da gravação |
| `Kind` | int | tipo do evento (tabela abaixo) |
| `X`, `Y` | int | coordenadas da tela (ou relativas, no preset) |
| `Button` | int | `0` esquerdo, `1` direito, `2` meio, `3` X1, `4` X2 |
| `Vk` | int | virtual key do Windows (`0x41` = A) |
| `Scan` | int | scan code físico |
| `Extended` | bool | tecla estendida (setas, Insert, etc.) |
| `Wheel` | int | delta da roda (+120 / -120 por entalhe) |
| `Hwnd` | int64 | janela sob o cursor / destino, quando disponível |

### Tipos de evento (`Kind`)

| Valor | Nome | Descrição |
|---|---|---|
| 1 | `KeyDown` | tecla pressionada |
| 2 | `KeyUp` | tecla solta |
| 3 | `MouseMove` | movimento do mouse |
| 4 | `MouseDown` | botão pressionado |
| 5 | `MouseUp` | botão solto |
| 6 | `Wheel` | roda do mouse |
| 7 | `XDown` | botão lateral pressionado |
| 8 | `XUp` | botão lateral solto |

> O índice é o valor numérico serializado. O campo `KindName` mostrado na interface
> é calculado e não existe no JSON.

## `.pulso` — macro pronta

Pacote de distribuição com a macro e o preset de reprodução:

```json
{
  "App": "Pulso",
  "Version": 1,
  "Name": "farm jogo",
  "CreatedAt": "2026-11-14T20:57:23.1784831-03:00",
  "Events": 17,
  "DurationMs": 71206,
  "Macro": { "...": "objeto Macro completo, igual ao .gmt" },
  "Play": {
    "SpeedPercent": 150,
    "Instant": false,
    "Loops": 4,
    "Continuous": false,
    "LoopDelayMs": 2250,
    "StartDelayMs": 0,
    "RelativeCoords": false,
    "PlayToRecordedWindow": true,
    "UseScanCodes": true
  }
}
```

| Campo | Significado |
|---|---|
| `Name` | nome amigável da automação |
| `CreatedAt` | data/hora ISO 8601 da exportação |
| `Events` | contagem de eventos (informativo) |
| `DurationMs` | duração em ms (informativo) |
| `Macro` | objeto `Macro` completo |
| `Play` | preset aplicado ao reproduzir |

Ao abrir um `.pulso`, o Pulso carrega a macro e aplica o preset apenas para a
sessão de reprodução; ele não sobrescreve suas configurações globais.

## `.rec` — TinyTask (compatibilidade)

Formato binário simples: uma sequência de registros de **20 bytes**, little-endian,
sem cabeçalho.

| Offset | Tamanho | Campo | Uso no Pulso |
|---|---|---|---|
| 0 | 4 | `dwMsg` | tipo da mensagem do Windows |
| 4 | 4 | `wParam` | tecla (low byte = VK, high byte = scan) ou X |
| 8 | 4 | `lParam` | Y para eventos de mouse |
| 12 | 4 | `dwTime` | timestamp em ms |
| 16 | 4 | `hwndExtra` | janela de destino |

Mensagens reconhecidas:

| `dwMsg` | Evento | Conversão |
|---|---|---|
| 256 (`WM_KEYDOWN`) | `KeyDown` | `Vk = wParam & 0xFF`, `Scan = (wParam >> 8) & 0xFF` |
| 257 (`WM_KEYUP`) | `KeyUp` | idem |
| 512 (`WM_MOUSEMOVE`) | `MouseMove` | `X = wParam`, `Y = lParam` |
| 513 (`WM_LBUTTONDOWN`) | `MouseDown` botão 0 | coordenadas |
| 514 (`WM_LBUTTONUP`) | `MouseUp` botão 0 | coordenadas |
| 515 (`WM_LBUTTONDBLCLK`) | `MouseDown` + `MouseUp` | expandido em dois eventos |
| 516 (`WM_RBUTTONDOWN`) | `MouseDown` botão 1 | coordenadas |
| 517 (`WM_RBUTTONUP`) | `MouseUp` botão 1 | coordenadas |

Tempo na importação: o primeiro registro vira `T = 0`; os seguintes acumulam
`dwTime - dwTimeAnterior`. Outras mensagens são ignoradas e contadas no log.
Limitações do formato: **sem roda do mouse, sem botões X1/X2, sem botão do meio**.

Na exportação, o Pulso grava `dwTime = 1000 + T` e omite eventos sem equivalente.

## `.exe` — macro compilada

O Pulso não gera um executável do zero: ele copia a si mesmo (sem trailer antigo)
e anexa um trailer no fim do arquivo.

```text
+--------------------------------------------------+
| executável PE (Pulso, cópia sem trailer antigo)  |
+--------------------------------------------------+
| payload JSON (UTF-8)                             |
+--------------------------------------------------+
| uint32 payloadLength (little-endian)             |
+--------------------------------------------------+
| "PULSOTSK" (8 bytes ASCII)                       |
+--------------------------------------------------+
```

Regras de leitura:

1. Ler os últimos 12 bytes.
2. Conferir a assinatura `PULSOTSK` e o `payloadLength` (não nulo e menor que o arquivo).
3. O payload começa em `fileLength - 12 - payloadLength`.
4. O fim dos dados originais é exatamente esse offset (`dataEnd`).

Payload:

```json
{
  "App": "Pulso",
  "V": 2,
  "Macro": { "...": "objeto Macro" },
  "Play": { "...": "objeto Settings completo" }
}
```

Após anexar, o checksum PE é zerado (campo em `e_lfanew + 88`) para evitar
inconsistência de assinatura. O executável gerado lê o próprio trailer ao iniciar,
abre o player e reproduz a macro. `PULSO_PROBE=1` grava um relatório de diagnóstico
sem abrir a interface.

## Configuração — `Pulso.config.json`

```json
{
  "HotkeyRecord": "Ctrl+Alt+R",
  "HotkeyPlay": "Ctrl+Alt+P",
  "HotkeyPause": "Ctrl+Alt+Espaço",
  "HotkeyStop": "Ctrl+Alt+S",
  "HotkeyCompact": "Ctrl+Alt+M",
  "SpeedPercent": 100,
  "Instant": false,
  "Loops": 1,
  "Continuous": false,
  "LoopDelayMs": 0,
  "StartDelayMs": 0,
  "RecordMouse": true,
  "RecordKeyboard": true,
  "IgnoreOwnWindow": true,
  "RelativeCoords": false,
  "PlayToRecordedWindow": false,
  "UseScanCodes": true,
  "TopMost": true,
  "MinimizeToTray": false,
  "MinimizeToBar": false,
  "ConfirmClear": true,
  "ShowTrayBubble": true,
  "LastFolder": ""
}
```

Valores são saneados na leitura: velocidade limitada a 5–2000, repetições a
1–100.000, atrasos a 0–3.600.000 ms e atalhos inválidos voltam ao padrão.
