# TrinityConquer Agent Guide

Objetivo: gastar menos contexto/tokens mantendo seguranca nas alteracoes.

## Primeiro leia pouco

Antes de abrir arquivos grandes, use `rg` para achar o ponto exato.

Arquivos de mapa:

- `README.md`: setup atual e decisoes principais.
- `docs/MAPA_SISTEMAS.md`: mapa dos sistemas e features.
- `COServer/Database5700/Features5017.json`: origem/decisao de cada sistema.

Evite ler diretorios inteiros como `COServer/Database5700`, `COServer/DFGameServer/Game/MsgNpc` e `COServer/DFGameServer/Game/MsgServer` sem filtro.

## Estado atual

- Sem MongoDB e sem MySQL no fluxo ativo.
- Autenticacao/configuracao da API usa JSON local em `API/AuthJson/*.json`.
- GameServer usa arquivos com `DbFromFiles=true`.
- Build publica para `C:\Users\hecto\OneDrive\Desktop\Placebo`.
- Solucao principal: `COServer/TrinityConquerServer.sln`.
- Target: `.NET 10`.

## Fluxo barato

1. Use `git status --short`.
2. Use `rg` para localizar simbolos, rotas, ids de itens/NPCs ou feature ids.
3. Abra somente os trechos necessarios.
4. Edite com patch pequeno.
5. Rode `dotnet build .\COServer\TrinityConquerServer.sln --nologo` quando mudar C# ou csproj.
6. Nao rode scripts Python nesta maquina sem checar runtime; Python pode nao estar instalado.

## Sistemas 5017

Resumo atual (gerado de `Features5017.json`; use `tools/features_set.py --list` para conferir):

- `keep`: 25 sistemas.
- `remove`: 42 sistemas.
- `review`: 0 sistemas.

Sistemas em `remove`:

- `social.tradepartner`
- `core.itemextra`
- `core.itemlock`
- `economy.demonbox`
- `social.arsenal`
- `economy.luckybag`
- `social.clan`
- `social.mentor`
- `social.flowers`
- `social.houses`
- `progression.enlight`
- `progression.subclass`
- `progression.chi`
- `progression.achievements`
- `progression.titles`
- `progression.transform`
- `economy.vip`
- `economy.osshop`
- `economy.advertise`
- `economy.poker`
- `economy.memoryagate`
- `economy.surprisebox`
- `events.teamarena`
- `events.elitepk`
- `events.eliteguildwar`
- `events.citywar`
- `events.clanwar`
- `events.classpkwar`
- `events.poledomination`
- `events.ctf`
- `events.fortress`
- `events.quizshow`
- `events.skilltournament`
- `events.couples`
- `events.demonexterminator`
- `events.race`
- `events.custom-minigames`
- `events.knightgame`
- `extras.interserver`
- `extras.bot`
- `extras.offlinetg`
- `extras.machine`

Sistemas em `review`: nenhum.

Nota: `core.itemlock` e' `remove` mas sem paths/types de proposito. O gate e' em codigo (`MsgItemLock.LockEnabled`) para o unlock de itens ja travados continuar funcionando.

Linha do tempo usada para as decisoes: `docs/ERA1_LINHA_DO_TEMPO.md`.

## Cuidados

- Nao reverta mudancas existentes sem pedido explicito.
- Se o usuario disser que tudo entra no Git, ainda preserve o repositorio interno `TrinityConquer-main/.git`; ele e o repo real com remote.
- Nao reintroduza MongoDB.
- Nao altere gameplay e infraestrutura no mesmo patch quando puder separar.
- Quando revisar sistema, entregue: decisao sugerida (`keep/remove/review`), arquivos tocados, risco e teste.
