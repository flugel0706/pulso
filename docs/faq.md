# FAQ

**O Pulso é um vírus?**
Não. O código é aberto (MIT) e usa as mesmas APIs do Windows que qualquer
ferramenta de automação: hooks de teclado/mouse e `SendInput`. Executáveis
compilados podem disparar heurística de antivírus; compile você mesmo e confira
o hash publicado na release.

**Funciona em jogos?**
Depende. Jogos sem anti-cheat geralmente aceitam. Jogos com anti-cheat podem
bloquear entrada sintética por design — nesse caso, nenhuma ferramenta de
automação funciona de forma confiável. Teste o modo scan code e o envio direto
à janela.

**Preciso instalar algo?**
Na edição portátil, apenas o .NET 8 Desktop Runtime. Na edição autocontida,
nada. Não há instalador, serviço ou driver.

**Onde ficam minhas macros e configurações?**
Ao lado do executável: `Pulso.config.json`, `Pulso.log` e a pasta `macros\`.
Se a pasta não for gravável, o Pulso usa `%APPDATA%\Pulso`.

**Como troco os atalhos?**
**Ferramentas → Configurações** (`Ctrl+,`) → seção de atalhos → **Alterar**.
As combinações são globais e funcionam com o Pulso em segundo plano.

**Qual a diferença entre `.gmt`, `.pulso` e `.rec`?**
`.gmt` é a macro nativa em JSON; `.pulso` é um pacote com macro + preset de
reprodução; `.rec` é o formato binário do TinyTask, suportado para importação
e exportação.

**Por que a exportação `.rec` perde a roda do mouse?**
O formato TinyTask não tem representação para roda nem botões X1/X2. O Pulso
omite esses eventos e informa quantos ficaram de fora.

**Como gero um executável da minha macro?**
Pela CLI: `Pulso.exe --compile minha-macro.gmt minha-macro.exe`.
O `.exe` abre direto no player e reproduz sozinho. `F12` encerra.
Para compartilhar a macro configurada sem gerar executável, use **Arquivo →
Exportar macro pronta…** (`.pulso`).

**O executável compilado ficou grande. É normal?**
Sim. Ele é uma cópia do Pulso com a macro anexada. Se você compilou a partir da
edição autocontida, o tamanho é o da edição autocontida.

**Funciona com múltiplos monitores?**
Sim, usando coordenadas absolutas da área de trabalho virtual. Evite escalas DPI
diferentes entre gravação e reprodução, ou use coordenadas relativas à janela.

**Posso usar comercialmente?**
Sim. A licença MIT permite uso, modificação e distribuição, mantendo o aviso de
copyright.

**Como paro tudo em emergência?**
**F12**, sempre. Funciona durante gravação, reprodução, com a janela minimizada
ou na bandeja.

**O Pulso pede administrador?**
Não por padrão. Para automatizar janelas elevadas, execute como administrador.
Sem elevação, o Windows bloqueia hooks sobre janelas de nível superior.

**Como desinstalar?**
Apague o executável e, se quiser remover tudo, `Pulso.config.json`, `Pulso.log`
e a pasta `macros\` (ou `%APPDATA%\Pulso`).

**O atalho global parou de funcionar.**
Outro programa pode ter capturado a combinação. Troque o atalho nas configurações
ou feche o conflitante (overlays, OBS, gravadores, drivers de teclado).
