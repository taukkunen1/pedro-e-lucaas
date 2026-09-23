# Auditoria da economia Era 1 / 5017

Este documento registra o estado da economia de hunting/mineração depois da limpeza dos sistemas pós-5017.

## Hunting: faucets normais

As chances-base ficam centralizadas em `Game/Era1/Era1Economy.cs` e são aplicadas pelo `ConfigurableDropSystem`:

- Gold: 15% por kill, mas somente quando o monstro possui `drop_money > 0`.
- Equipamento: 1,2% por kill.
- Meteor: 0,20% por kill.
- Dragon Ball: 0,002% por kill (aprox. 1 em 50.000 kills).
- +1: somente em equipamento de qualidade Normal, 1 em 7.500.
- +2 ou superior: não nasce diretamente do hunting normal; permanece progressão por composição/BURN.

O Gold normal usa o `drop_money` do arquivo real de cada monstro em `Database5700/Monsters/*.ini`. O antigo intervalo global de 1.000–2.000 foi removido porque inflava artificialmente monstros de baixo nível.

## Distribuição dos equipamentos

Dentro de um evento de equipamento válido, a seleção de família é:

- Boots: 1,67%.
- Necklace: 2,50%.
- Ring: 4,17%.
- Headgear: 25,00%.
- Armor: 25,00%.
- Weapons: 41,67%.

Dentro da faixa de armas:

- Backsword: 20% dos weapon drops (8,33% do total dos equipment-family rolls).
- One-handers: 60% dos weapon drops (25,00% do total).
- Two-handers/shield: 20% dos weapon drops (8,33% do total).

Foi removida uma faixa morta de RNG que antes podia selecionar "equipment drop" sem gerar item e foi corrigido o nível do Backsword, que anteriormente podia ficar em zero.

## Qualidade

A rolagem agora usa um denominador comum e entrega exatamente as probabilidades declaradas, mutuamente exclusivas:

- Refined: 1/650.
- Unique: 1/1.500.
- Elite: 1/7.500.
- Super: 1/30.000.
- Demais: Normal.

Bless e sockets aleatórios vindos do caminho moderno de drop permanecem desativados.

## Mineração

A mineração continua sendo uma faucet clássica de ore/gem/Meteor/DB. As probabilidades configuradas são sequenciais, portanto o valor econômico relevante é a chance efetiva por tick de mineração:

- Dragon Ball: 0,025% por tick (aprox. 1/4.000).
- Meteor: 0,3748125% por tick (aprox. 1/266,8).
- Super Gem: 0,00099200375% por tick (aprox. 1/100.806).
- Refined Gem: 0,039660309925% por tick (aprox. 1/2.521).
- Normal Gem: 1,943355186325% por tick.
- Ore: 47,61618% por tick.
- Nada: 50%.

O limite diário moderno de mineração e recompensas +Stone continuam fora da Era 1.

## Auditoria por monstro e mapa

Execute:

```bash
python tools/era1_drop_audit.py
```

O script cruza os dados reais de `Monsters/*.ini` e `MobSpawns/**/*` com a política de economia e gera:

- `docs/catalogo/era1_drops_por_mapa_monstro.csv`
- `docs/catalogo/era1_mining_rates.csv`
- `docs/catalogo/era1_drop_anomalias.csv`

O CSV principal mostra, para cada par mapa/monstro encontrado nos spawns, o nível, número de geradores/spawns, `drop_money`, níveis de drop por slot e todas as faucets Era 1 aplicáveis.

## MINT / BURN

A telemetria cobre Gold, CP, Bound CP e recursos escassos:

- Meteor / MeteorTear
- Dragon Ball
- Gems Normal / Refined / Super
- Equipamentos Refined / Unique / Elite / Super
- Equipamentos +N

O resumo diário inclui `minted`, `burned`, `net`, razão MINT/BURN, cobertura de BURN, origem por sistema e agora também agregação por mapa para recursos escassos. O NDJSON inclui o nome do recurso em eventos de resource telemetry.

A regra operacional é simples: a economia só deve ser rebalanceada depois de observar MINT/BURN real em produção. A taxa no código é a faucet teórica; a taxa efetiva depende da população de cada mapa, velocidade de kill, composição dos spawns, perdas no chão e consumo em composição/forja.


## Fechamento de faucets paralelas

A auditoria do fluxo real de morte de monstros encontrou recompensas que não passavam pelo `DropMob` normal. Na Era 1 elas ficam explicitamente separadas:

- `EnablePost5017MonsterRewards = false` bloqueia os reward scripts 5695/custom que injetavam Fruits/City rewards, Study Points, Souls, +Stones altos, bundles de Dragon Ball e recompensas de bosses posteriores.
- Nemesis (incluindo o bundle direto de 7 Dragon Balls), Chaos Guard, NightmareCaptain e os caminhos PurpleBanshee posteriores ficam atrás desse gate.
- `MonsterRole.DropItem` e `MonsterRole.DropItemID` também aplicam `Era1Items.IsAllowedGeneratedDrop`, evitando que um script legado contorne a fronteira de itens apenas por não utilizar o `DropMob`.
- O WaterDevil ID 8419 permanece como exceção direta declarada por `IsClassicDirectDragonBallMonster`. O auditor não mistura essa exceção com a taxa global: registra `dragonball_global_pct`, `dragonball_direct_units_per_kill` e `dragonball_expected_units_per_kill`.

A exceção direta precisa ser lida como uma faucet especial do monstro, e não como alteração da taxa global de Dragon Ball.

## Taxa efetiva por monstro

A chance global de tentativa de equipamento é 1,2%, porém a chance de item realmente produzido depende dos slots habilitados no arquivo do monstro. O relatório agora distingue:

- `equipment_attempt_pct`: chance de entrar no evento global de equipamento.
- `equipment_family_success_pct`: probabilidade de a família sorteada possuir nível de drop válido para aquele monstro.
- `equipment_effective_pct`: produto das duas probabilidades.
- `runtime_spawn_capacity_est`: estimativa construída conforme os dois loaders reais de spawn.
- `special_drops`: entradas de `[SpecialDrop]` que exigem revisão explícita.

Isso impede usar 1,2% como se fosse a taxa efetiva para qualquer monstro/mapa.


## Economy V2: BURN, composição e sockets

O fechamento do lado de consumo da economia cobre agora os principais caminhos de forja da Era 1.

### Composição 5017

A auditoria histórica mostrou que `Composition Points` não pertence ao 5017: esse modelo foi introduzido no patch 5066. Portanto, `MsgUpdateItem.Compose` não usa `PlusProgress`/`ComposePlusPoints` na Era 1.

- `Quick Compose`/`ChanceUpgrade` fica bloqueado.
- +0 até +9 usa a composição antiga: 1 item principal + exatamente 2 materiais compatíveis para subir um nível.
- O material precisa ser +N válido, não pode repetir UID, não pode ser o próprio alvo, não pode estar locked e não pode ter qualidade superior à do item principal.
- Armas são separadas em Bow, 1-handed, 2-handed e Backsword; demais equipamentos precisam pertencer à mesma categoria/tipo.
- Equipamento +0 não pode ser material; +Stones +1..+8 substituem materiais de equipamento.
- Para produzir +6 ou superior pela composição antiga, são exigidas as gems adicionais da regra clássica: 2 gems para arma e 1 para equipamento não-arma.
- +9 -> +10 custa 12 Dragon Balls; +10 -> +11 custa 25; +11 -> +12 custa 40, com requisito de personagem level 130+.
- Dragon Ball Scroll vale 10 DBs; troco de um scroll parcialmente utilizado volta como Dragon Balls físicas, mantendo o BURN líquido correto.
- O estado do alvo é registrado como transformação: por exemplo, +1 -> +2 gera BURN de `Equipment.Plus1` e MINT de `Equipment.Plus2`, além do BURN dos dois materiais.

### Upgrade de nível e qualidade

- Upgrade de qualidade só aceita Dragon Balls em **todos** os UIDs enviados; UIDs repetidos são rejeitados.
- O alvo precisa ser equipamento clássico válido e qualidade Super não pode ser incrementada novamente.
- Refined -> Unique -> Elite -> Super registra BURN do estado anterior e MINT do novo estado.
- Meteor/Meteor Tear/Meteor Scroll são consolidados em `Meteor`; scroll vale 10 unidades.
- Reparo de equipamento com durabilidade zero passa pelo mesmo pool de Meteor/Tear/Scroll e consome 5 Meteors líquidos.

### Gems e recursos auxiliares

- Gem Compose mantém 15 Normal + 10.000 Silver -> Refined e 15 Refined + 800.000 Silver -> Super; Refined Tortoise -> Super custa 1.000.000 Silver no handler existente.
- O ramo que permitia parâmetros especiais de `GemCompose` adicionarem um item sem débito correspondente foi removido.
- `ToristSuper` não desconta mais 100.000 Silver antes de confirmar que o conjunto completo de gems existe.
- A recompensa histórica aleatória de 7 Refined Gems -> 2/3 Refined Tortoise ou Super Tortoise ainda precisa de probabilidade histórica confiável antes de substituir o resultado legado do código.
- Tough Drill e Star Drill entram como `Drill.Tough` e `Drill.Star`; +Stones entram como `PlusStone.PlusN`.

### Socket policy

- Armas clássicas: 1 Dragon Ball para o primeiro socket e 5 Dragon Balls para o segundo.
- Equipamentos clássicos não-arma: 12 Dragon Balls para o primeiro socket; segundo socket por Tough Drill (chance do código) ou 7 Star Drills.
- Segundo socket exige que o primeiro já exista; pacotes não podem abrir diretamente o slot 2.
- Cada socket criado gera `Equipment.Socket1` ou `Equipment.Socket2` na telemetria.
- `MsgEmbedSocket` possui escopo próprio (`EmbedSocket:<acao>#<slot>`), então o BURN de gems inseridas é classificado em `Forging`.

### Rotas pós-5017 bloqueadas

- Talisman socket por CP/item é rejeitado na fronteira de `ItemUsage`.
- Refinery/Purification/Stabilization em `MsgItemExtra` ficam atrás de `EnablePost5017ItemExtra = false`.
- Steed composition e reward de mentor ligados à composição permanecem fora da Era 1.
- Stabilization Stones foram retiradas do handler `BuyItemFromForging`.

A leitura operacional passa a ser **faucet -> inventário -> transformação/sink**. Para cada recurso, compare `minted`, `burned`, `net`, origem por sistema e por mapa. Para progressão, acompanhe também as transições `Equipment.PlusN`, qualidade e sockets.
