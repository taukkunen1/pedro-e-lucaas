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
