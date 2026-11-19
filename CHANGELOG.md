# Changelog

Todas as mudanças relevantes do Pulso são registradas aqui.
O formato segue [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/)
e o projeto adere ao [Versionamento Semântico](https://semver.org/lang/pt-BR/).

## [3.1.0] - 2026-11-19

### Corrigido
- **Fundo sem distorção ao minimizar, maximizar e redimensionar:** a imagem
  deixou de ser composta em um bitmap único do tamanho da janela, cache que podia
  ficar defasado durante as transições. Cada painel agora desenha o recorte da foto
  a partir do tamanho atual do cliente, com escala uniforme, mantendo o
  enquadramento e as sobreposições alinhadas.

### Adicionado
- **Menu de contexto da barra compacta** (clique direito): gravar/reproduzir,
  pausar, parar, abrir macro, salvar, salvar como, repetição contínua, sempre no
  topo, configurações, expandir e fechar.
- Dicas contextuais no rodapé da barra compacta ao passar o mouse sobre cada
  controle.
- Linha de progresso da reprodução por ciclo, inclusive no modo contínuo.

### Alterado
- **Barra compacta reconstruída:** recorte cinematográfico da foto de operação,
  painel de leitura dedicado e nome da macro, estado, eventos, duração,
  velocidade e repetições sempre visíveis.
- **Controles diretos na barra:** gravar, reproduzir, pausar e parar em destaque,
  além de velocidade −/+, repetição contínua, configurações, expandir e fechar.
- Modo compacto com **tamanho fixo** e sem alças de redimensionamento.
- **Estado da janela preservado:** sair do modo compacto devolve o tamanho
  anterior, inclusive quando a janela estava maximizada.
- Versão 3.1.0.

## [3.0.0] - 2026-11-18

### Alterado
- **Novo visual tático**, inspirado em interfaces de operações militares:
  paleta oliva e areia, tipografia condensada (Bahnschrift), cantos retos com
  colchetes de HUD e barras translúcidas.
- **Fundo de operação noturna** embutido no executável, com brilho, dessaturação
  e vinheta aplicados em tempo real. Um `backdrop.jpg` ao lado do `Pulso.exe`
  substitui a imagem padrão.
- Janela padrão maior (1160 x 760) e barra lateral compactada, sem rolagem na
  configuração inicial.
- Ícone da marca redesenhado como hexágono com linha de pulso.
- Logotipo, barra mini e diálogo Sobre atualizados para o novo idioma visual.

### Adicionado
- **Faixa de operação** no topo com o nome da macro em destaque, contagem de
  eventos, duração e estado ao vivo (gravação em vermelho).
- Rodapé fixo na barra lateral com o aviso de parada de emergência (`F12`).
- Teste interno da imagem de fundo embutida.

### Mantido
- **Desfazer e refazer** (`Ctrl+Z` / `Ctrl+Y` / `Ctrl+Shift+Z`) com histórico
  de até 40 passos, cobrindo remoção, edição, duplicação, pausa, escala de
  tempo, remoção de espera inicial e limpeza da macro.
- **Arquivos recentes** no menu Arquivo, com até 8 entradas persistentes.
- **Duplicar selecionados** (`Ctrl+D`): repete o bloco escolhido logo depois,
  deslocando o restante da macro e preservando os intervalos internos.
- Botão **Apagar tudo** na barra lateral e ícone de lixeira no topo, além de
  `Ctrl+Del` e **Editar → Apagar tudo**.
- Pacote **`.pulso`** (macro + preset de reprodução) com exportar (`Ctrl+E`) e
  carregar macro pronta (`Ctrl+Shift+O`).

## [2.0.0] - 2026-11-18

### Adicionado
- **Desfazer e refazer** (`Ctrl+Z` / `Ctrl+Y` / `Ctrl+Shift+Z`) com histórico
  de até 40 passos, cobrindo remoção, edição, duplicação, pausa, escala de
  tempo, remoção de espera inicial e limpeza da macro.
- **Arquivos recentes** no menu Arquivo, com até 8 entradas persistentes.
- **Duplicar selecionados** (`Ctrl+D`): repete o bloco escolhido logo depois,
  deslocando o restante da macro e preservando os intervalos internos.
- Botão **Apagar tudo** na barra lateral e ícone de lixeira no topo, além de
  `Ctrl+Del` e **Editar → Apagar tudo**.
- Pacote **`.pulso`** (macro + preset de reprodução) com exportar (`Ctrl+E`) e
  carregar macro pronta (`Ctrl+Shift+O`).

### Mantido
- Estúdio com linha do tempo editável (tempo, evento, detalhe).
- Gravação global de mouse e teclado por hooks de baixo nível.
- Reprodução com velocidade de 5% a 2000%, repetições, modo contínuo,
  atraso entre ciclos e atraso inicial.
- Parada de emergência global com `F12`.
- Atalhos globais configuráveis para gravar, reproduzir, pausar, parar e modo compacto.
- Presets de reprodução salvos no arquivo `.pulso`.
- Compilação de macro em executável autônomo com trailer `PULSOTSK`.
- Importação e exportação do formato TinyTask `.rec`.
- Envio de teclas por scan code e `PostMessage` para a janela gravada.
- Reprodução relativa à janela gravada.
- HUD compacto, barra mini, bandeja do sistema e modo sempre no topo.
- CLI com `--selftest`, `--smoke`, `--demo`, `--compile`, `--play` e `--version`.
- 12 testes internos cobrindo atalhos, JSON, `.rec`, trailer, checksum PE,
  `SendInput`, configurações e pacote `.pulso`.
- Tema escuro e ícones vetoriais próprios.

### Notas
- Requer .NET 8 Desktop Runtime na edição portátil.
- O formato `.rec` não armazena roda do mouse nem botões laterais; esses
  eventos são omitidos na exportação e reportados ao usuário.
