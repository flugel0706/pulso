# Uso

Guia completo do Pulso. Para instalação rápida, veja o [README](../README.md).

## Instalação

O Pulso é portátil: basta executar `Pulso.exe`. Não há instalador nem serviço.

- **Configuração:** `Pulso.config.json` ao lado do executável. Se a pasta não for
  gravável (ex.: `Program Files`), o Pulso usa `%APPDATA%\Pulso`.
- **Log:** `Pulso.log` no mesmo diretório de dados.
- **Macros:** subpasta `macros\`, criada automaticamente.

A edição portátil exige o [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0).
A edição autocontida não exige nada além do Windows x64.

## Primeira macro

1. Abra o Pulso.
2. Aperte o atalho global de gravação — padrão **`Ctrl+Alt+R`**.
3. Execute a sequência desejada em qualquer aplicação.
4. Aperte **`Ctrl+Alt+R`** novamente para parar.
5. Edite se necessário na linha do tempo.
6. Aperte **`F5`** ou **`Ctrl+Alt+P`** para reproduzir.

Durante a gravação, o Pulso fecha o próprio loop de eventos quando `IgnoreOwnWindow`
está ativo: cliques na janela do estúdio não entram na macro.

> O atalho de gravação é suprimido do teclado enquanto a captura está ativa,
> então ele não contamina a macro.

## Reprodução

Na barra inferior:

| Controle | Efeito |
|---|---|
| **Velocidade** | 5% a 2000%. `200%` reproduz duas vezes mais rápido; `50%`, duas vezes mais devagar |
| **Repetições** | 1 a 100.000 ciclos |
| **Contínuo** | repete indefinidamente (configurável em Configurações) |
| **Atraso entre ciclos** | espera em ms antes de cada nova repetição |
| **Atraso inicial** | espera em ms antes do primeiro evento |
| **Instantâneo** | ignora os intervalos gravados e dispara o mais rápido possível |

Comandos:

- **Reproduzir:** `F5` ou atalho global.
- **Pausar/continuar:** botão ou atalho global (padrão `Ctrl+Alt+Espaço`).
- **Parar:** botão, atalho global de parar (padrão `Ctrl+Alt+S`) ou **`F12`**.
- **`F12` é a parada de emergência:** funciona sempre, inclusive durante gravação,
  reprodução e com a janela minimizada.

### Reproduzir na janela gravada

Em **Configurações**:

- **Coordenadas relativas à janela gravada** — os cliques são recalculados a partir
  da posição da janela no momento da reprodução. Útil quando a janela muda de lugar.
- **Enviar teclas direto para a janela gravada (PostMessage)** — as teclas vão para
  o handle registrado, sem depender do foco. Funciona apenas com aplicativos que
  processam `WM_KEYDOWN`/`WM_KEYUP`; alguns apps ignoram mensagens sintéticas.
- **Injetar por scan code** — recomendado para jogos e aplicações que leem o layout
  físico do teclado.

## Edição da linha do tempo

Cada linha mostra `#`, tempo acumulado, tipo de evento e detalhe.

- **Duplo clique** em um evento abre o editor (tempo, coordenadas, tecla, botão).
- **Delete** remove os selecionados; **Ctrl+Delete** apaga a macro inteira
  (com confirmação, se ativada).
- **Editar → Inserir pausa a partir daqui…** adiciona uma espera a partir do ponto escolhido.
- **Editar → Ajustar escala de tempo…** multiplica todos os intervalos por um fator.
- **Editar → Remover espera inicial** zera o tempo até o primeiro evento.
- Arraste e solte `.gmt`, `.pulso`, `.rec` ou `.json` na janela para abrir.

## Exportar e compilar

### Macro pronta (`.pulso`)

**Arquivo → Exportar macro pronta…** (`Ctrl+E`) gera um pacote com a macro e o
preset de reprodução atual (velocidade, repetições, atrasos, coordenadas relativas,
scan codes). É o formato ideal para compartilhar uma automação configurada.

### Executável autônomo (`.exe`)

A compilação é feita pela linha de comando:

```powershell
Pulso.exe --compile minha-macro.gmt minha-macro.exe
```

O executável gerado:

- é uma cópia do próprio Pulso com a macro embutida em um trailer `PULSOTSK`;
- abre direto no player, sem mostrar o estúdio;
- reproduz com o preset salvo no momento da compilação;
- encerra com `F12`;
- mantém a mesma dependência da edição usada como base (portátil ou autocontida).

### TinyTask (`.rec`)

- **Arquivo → Importar do TinyTask (.rec)…**
- **Arquivo → Exportar para TinyTask (.rec)…**

O formato `.rec` não possui representação para roda do mouse e botões X1/X2;
esses eventos são omitidos na exportação (o Pulso informa quantos ficaram de fora).

## Configurações

**Ferramentas → Configurações** (`Ctrl+,`):

| Opção | Descrição |
|---|---|
| Atalhos | Gravar, reproduzir, pausar, parar e modo compacto (globais) |
| Velocidade | Percentual de reprodução (5–2000%) |
| Repetições | Número de ciclos (1–100.000) |
| Contínuo | Repetição infinita |
| Atraso entre ciclos | Espera entre repetições (0–3.600.000 ms) |
| Atraso inicial | Espera antes do primeiro evento (0–60.000 ms) |
| Instantâneo | Sem intervalos entre eventos |
| Gravar mouse | Movimento, cliques, roda e botões X |
| Gravar teclado | Teclas |
| Ignorar janela do Pulso | Não captura eventos dentro do estúdio |
| Coordenadas relativas | Relativas à janela gravada |
| PostMessage | Envia teclas direto para a janela gravada |
| Scan codes | Injeta teclas pelo código físico |
| Sempre no topo | Mantém a janela acima das demais |
| Minimizar para bandeja | Ícone na área de notificação |
| Minimizar para barra | Ao minimizar, vira a barra compacta |
| Confirmar limpeza | Pede confirmação ao apagar tudo |
| Balão na bandeja | Exibe notificações do sistema |

## Modo compacto e bandeja

- **`Ctrl+Alt+M`** alterna o modo compacto (barra mini com gravar, reproduzir, parar e estado).
- Se **Minimizar para bandeja** estiver ativo, fechar a janela mantém o Pulso ativo na bandeja.
- O menu da bandeja oferece gravar, reproduzir, pausar, parar e abrir o estúdio.

## CLI

```text
Pulso.exe                          abre o estúdio
Pulso.exe --version                versão
Pulso.exe --help                   ajuda
Pulso.exe --selftest               testes internos → selftest.txt
Pulso.exe --smoke                  teste de criação das janelas → smoke.txt
Pulso.exe --demo <arquivo.gmt>     macro de demonstração
Pulso.exe --compile <in> <out.exe> compila .gmt/.rec em executável
Pulso.exe --play <arquivo>         reproduz sem abrir o estúdio
Pulso.exe <arquivo>                abre a macro no editor
```

Os relatórios são gravados ao lado do executável e também impressos no console
quando o processo é iniciado a partir de um terminal.

`PULSO_PROBE=1` faz qualquer instância (inclusive um `.exe` compilado) gravar
`probe.txt` com versão, trailer, número de eventos, duração e preset, e sair sem UI.

## Solução de problemas

**O atalho global não funciona**
Outro programa pode estar usando a mesma combinação (gravadores, overlays, OBS,
drivers). Troque em Configurações ou feche o conflitante.

**Não captura cliques em janelas elevadas**
O Windows bloqueia hooks de um processo comum em janelas administradoras.
Execute o Pulso como administrador se precisar automatizar esses aplicativos.

**A macro erra a posição**
Use coordenadas relativas à janela gravada, mantenha a janela no mesmo tamanho
e evite monitores com escalas DPI diferentes entre gravação e reprodução.

**O jogo ignora a automação**
Alguns anti-cheats bloqueiam `SendInput`/`PostMessage` por design. Teste o modo
scan code e o envio direto à janela; se ainda assim falhar, o jogo não aceita
entrada sintética.

**Antivírus sinaliza o executável compilado**
Hooks e injeção de entrada são comportamentos usados por malware, então a
heurística pode disparar. O código é aberto; compile você mesmo e confira o hash.

**O executável portátil não abre**
Instale o .NET 8 Desktop Runtime ou use a edição autocontida.
