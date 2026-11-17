# Changelog

Todas as mudanças relevantes do Pulso são registradas aqui.
O formato segue [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/)
e o projeto adere ao [Versionamento Semântico](https://semver.org/lang/pt-BR/).

## [3.0.0] — 2026-11-17

### Adicionado
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
