# Política de segurança

## Versões suportadas

| Versão | Suporte |
|---|---|
| 3.x | sim |
| < 3.0 | não |

## Reportando uma vulnerabilidade

Não abra issue pública para problemas de segurança. Use
**Security → Report a vulnerability** no GitHub (private vulnerability reporting)
ou entre em contato com o mantenedor pelo perfil do repositório.

Inclua:

- descrição do problema e impacto;
- versão do Pulso e do Windows;
- passos para reproduzir ou PoC;
- arquivos/logs relevantes (sem dados pessoais).

Resposta esperada em até 7 dias. Correções de segurança são priorizadas e
divulgadas após a publicação da versão corrigida.

## Escopo

O Pulso executa automação local com privilégios do usuário. Fazem parte do escopo:

- execução de código a partir de macros ou arquivos `.pulso`/`.gmt`/`.rec`;
- gravação indevida de entrada além do esperado;
- falhas no parsing de arquivos que causem crash ou corrupção;
- problemas no trailer de executáveis compilados;
- elevação de privilégio, persistência ou acesso a dados não autorizado.

## Fora de escopo

- Alertas de antivírus por heurística (hooks + injeção de entrada são o propósito da ferramenta).
- Bloqueios impostos por anti-cheat de jogos.
- Uso da ferramenta para automatizar sistemas sem autorização.
- Engenharia social ou phishing.

## Boas práticas para usuários

- Baixe apenas das Releases oficiais e confira o `SHA256SUMS.txt`.
- Não execute macros compiladas de origem desconhecida: um `.exe` compilado é um
  programa executável completo.
- Mantenha o .NET Desktop Runtime atualizado na edição portátil.
