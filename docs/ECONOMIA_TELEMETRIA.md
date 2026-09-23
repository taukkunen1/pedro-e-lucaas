# Telemetria de economia (mint / burn) — moedas e recursos Era 1

## Como funciona
- **Moedas:** os setters `Player.Money`, `Player.ConquerPoints` e `Player.BoundConquerPoints` chamam `Economy.Record(...)`.
- **Recursos escassos:** a fronteira do inventário chama `Economy.RecordResource(...)` para Meteor/MeteorTear/Scroll, DragonBall/Scroll, gems Normal/Refined/Super, Tough/Star Drill, +Stones, equipamentos Refined/Unique/Elite/Super e equipamentos +N. Scrolls valem 10 unidades do recurso-base. Isso permite medir o MINT/BURN real dos principais recursos da Era 1.
- **Motivo (reason):** vem do escopo aberto no ponto de entrada — `Npc:<NpcID>#<opção>`, `ItemUsage:<ação>`, `ItemUse:<id> <nome>`, `Compose:<ação>` e `EmbedSocket:<ação>#<slot>` — ou, sem escopo, do stack de chamadas (`Code:Classe.Método`).
- **Classificação:** `Database5700/EconomyTelemetry.json` (criado no primeiro start) tem regras regex `reason -> system` e `flow` (`auto` = sinal do delta, `transfer`, `ignore`). Primeira regra que casa vence; sem regra => `Other`.
- **Mint** = moeda criada (delta > 0) · **Burn** = destruída (delta < 0) · **Transfer** = troca/armazém/barraca/poker/inter-server: a soma deve dar ~0; o resíduo aponta vazamento (imposto ou exploit).

## Saídas (`Database5700/Telemetry/economy/`, ignorada no git)
- `yyyy-MM-dd.ndjson`: um evento por linha (hora, uid, nome, mapa, moeda, antes, depois, delta, flow, system, reason, alert).
- `summary-yyyy-MM-dd.json`: a cada 5 min e à meia-noite — mint/burn/líquido por moeda e por recurso, razão MINT/BURN, cobertura de burn, sistemas, top motivos e top jogadores.
- Alertas no console e `"alert":true` para eventos acima de `alertGold` (500M) / `alertCps` (5.000).

## Ferramentas
- `python3 tools/economy_report.py [--from D --to D] [--supply]` — relatório mint/burn/resíduo por sistema; `--supply` soma a oferta em `Users/*.ini`.
- `python3 tools/economy_sites.py` — inventário estático (`docs/catalogo/economia_pontos.csv`): cada linha do código que ganha/gasta/atribui Gold/CP, com arquivo, linha, NPC, método, valor e texto próximo.

## Pontos de atenção
- Volume: por padrão todo evento é gravado (`logMinDeltaGold/Cps = 1`). Com muitos jogadores suba `logMinDeltaGold`; os agregados continuam exatos.
- Fora do alcance (não passam pelo setter): ouro que expira no chão, fundos de guilda, ouro no armazém (WHMoney) além do overflow de 2 bi (contado como saldo do jogador), e mudanças feitas direto nos arquivos `.ini`.
- Drop de ouro do jogador (morte/`DropGold`) aparece como burn `FloorDrop`; quem pega vê mint `FloorPickup`. O par se anula no líquido.
- O `Other` deve ficar pequeno: olhe "maiores motivos" no relatório e adicione regras.
- A regra de `Code:*` depende do nome da classe/método do código; se renomear, ajuste o JSON.


## Recursos escassos por mapa
O resumo diário de `resources` também contém `byMap`, com MINT, BURN, líquido, transferências e número de eventos por mapa. Isso permite localizar faucets excessivas de Meteor, Dragon Ball, gems, equipamentos de qualidade e +N usando dados reais de produção.

Para a auditoria estática por monstro/mapa, use `python tools/era1_drop_audit.py`. O relatório cruza `Monsters/*.ini` com `MobSpawns/**/*` e grava os CSVs em `docs/catalogo/`.


## MINT de recursos: criação física x oferta efetiva
Para Meteor, Dragon Ball, gems e equipamentos raros, a telemetria operacional contabiliza o MINT no limite do inventário. Portanto, um item criado no chão e abandonado/desaparecido não entra como oferta econômica efetiva. Quando o item é coletado, o evento carrega o mapa atual e entra nos agregados `resources.byMap`.

A auditoria estática em `tools/era1_drop_audit.py` mede a faucet teórica/configurada por monstro e mapa; a telemetria mede o que de fato chegou à economia dos jogadores. A diferença entre as duas é útil para enxergar perdas no chão, comportamento de farming e efeitos de densidade/velocidade de kill.


## Transformações de equipamento
Além da remoção física de materiais, a Economy V2 registra mutações do próprio equipamento. Se um alvo passa de Refined para Unique ou de +N para +(N+1), o estado anterior é registrado como BURN e o novo como MINT. A abertura de socket registra `Equipment.Socket1`/`Equipment.Socket2`.

Isso evita o falso cenário em que Dragon Balls, Meteors ou +Stones aparecem como consumidos, mas a saída econômica produzida pela forja fica invisível no resumo.


### Gems inseridas em sockets
A Economy V2 separa a gem física da gem incorporada ao equipamento. Ao inserir uma gem, o item `Gem.*` físico sofre BURN e nasce `EmbeddedGem.*`; ao remover a gem do socket, `EmbeddedGem.*` sofre BURN e nenhuma gem física é devolvida. Isso permite medir inserção e remoção sem tratar a mesma unidade como se ainda estivesse disponível no inventário.


### Rotas de upgrade
As ações legadas `UpgradeMeteor` e `UpgradeDragonball` do pacote de uso de item também passam pela política Era 1. A rota alternativa de `UpgradeMeteor` não pode mais criar sockets aleatórios durante upgrade de nível; sockets só nascem pelos sinks explícitos de socket. Alterações de nível/qualidade feitas por essa rota também entram em `RecordEquipmentTransformation`.


## Integridade do BURN na Economy V2
A remoção de recursos é contabilizada somente quando o inventário realmente perde unidades. Chamadas repetidas sobre um UID já removido não geram novo BURN. Os helpers de Meteor/Dragon Ball preservam a equivalência de scrolls em 10 unidades e o troco volta como MINT físico, de forma que o líquido corresponda ao custo efetivamente gasto.

Gems inseridas em equipamento deixam o estoque físico `Gem.*` e passam para `EmbeddedGem.*`; a retirada/destruição da gem que estava no socket queima esse estado embutido. A abertura do slot em si continua separada como `Equipment.Socket1` ou `Equipment.Socket2`.
