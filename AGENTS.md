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

Resumo atual:

- `keep`: 36 sistemas.
- `remove`: 9 sistemas.
- `review`: 16 sistemas.

Sistemas em `remove`:

- `progression.chi`
- `progression.subclass`
- `progression.achievements`
- `progression.titles`
- `extras.offlinetg`
- `economy.osshop`
- `events.custom-minigames`
- `extras.bot`
- `extras.interserver`

Sistemas em `review`:

- `events.eliteguildwar`
- `events.fortress`
- `extras.autohunting`
- `economy.advertise`
- `economy.memoryagate`
- `economy.poker`
- `economy.surprisebox`
- `events.classpkwar`
- `events.couples`
- `events.demonexterminator`
- `events.poledomination`
- `events.skilltournament`
- `events.teamarena`
- `extras.machine`
- `progression.transform`
- `social.houses`

## Cuidados

- Nao reverta mudancas existentes sem pedido explicito.
- Se o usuario disser que tudo entra no Git, ainda preserve o repositorio interno `TrinityConquer-main/.git`; ele e o repo real com remote.
- Nao reintroduza MongoDB.
- Nao altere gameplay e infraestrutura no mesmo patch quando puder separar.
- Quando revisar sistema, entregue: decisao sugerida (`keep/remove/review`), arquivos tocados, risco e teste.
