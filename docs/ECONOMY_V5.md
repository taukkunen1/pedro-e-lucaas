# ECONOMY V5 — Trade, Guild, PK redemption e faucets globais

## Objetivo

A V5 fecha os principais caminhos de moeda que ficam fora de hunting, shops e Market. O foco é impedir MINT implícito, replay de recompensas, overflow e perda/duplicação de escrow, além de tornar todos os pontos diretos de mutação de Gold/CP inventariáveis.

A regra contábil usada é:

- **MINT**: aumenta a oferta total de moeda.
- **BURN**: reduz permanentemente a oferta total.
- **TRANSFER**: muda o proprietário/ledger da mesma moeda sem criar oferta.
- **GATED**: código legado permanece no source, mas a feature é removida pelo registro 5017.

## Trade

Trade é transferência, não faucet.

A V5:

- impede overflow ao acumular Gold/CP no escrow;
- impede settlement se algum receptor ultrapassaria o limite de uint;
- não descarta o escrow quando a validação do trade falha;
- zera/consome os dois ledgers de escrow antes de creditar os jogadores;
- impede que disconnect/replay resulte em refund de moeda já liquidada;
- preserva Code:(MsgTrade|Trade) como flow=transfer na telemetria.

## Guild economy

Doações de Gold e CP são **player -> guild treasury**.

Antes da V5, o setter do jogador enxergava apenas a perna negativa e a telemetria podia interpretar a operação como BURN. Agora cada depósito grava também a perna positiva off-player via Economy.RecordManual, com:

- GuildTreasury / Gold / transfer;
- GuildTreasury / CP / transfer.

A taxa de criação da guild continua sendo sink separado (GuildCreation), não treasury transfer.

## PK item redemption

RedeemGear e ClaimGear formam uma transferência diferida de CP:

1. o dono paga o ransom;
2. o item retorna ao dono;
3. o captor recebe um claim;
4. o claim credita exatamente o CP pago.

A V5 endurece esse ciclo:

- RedeemContainer.TryRemove(...) acontece antes do débito/retorno, tornando o redemption one-shot;
- ClaimContainer.TryRemove(...) acontece antes do crédito de CP;
- replay de ClaimGear não pode creditar o mesmo ransom duas vezes;
- há proteção de overflow antes do claim;
- claims de item expirado também usam remove-first;
- RedeemGear|ClaimGear são classificados como PkRedemption / transfer.

## Lottery

A lottery 5017 é mantida, mas o faucet é de **item**, não de Gold/CP.

A auditoria exige:

- 1 Small Lottery Ticket (711504) para adicionar jade;
- 3 Small Lottery Tickets para novo roll;
- ausência de mutação direta de Player.Money ou Player.ConquerPoints no MsgLottery.

Itens raros gerados continuam observáveis pela telemetria de recursos já existente.

## Quests e events

O principal vazamento encontrado era o fallback de EventsRewards:

- uma chave inexistente criava **350 CP** automaticamente.

Na V5:

- chave de reward ausente retorna **0**;
- o erro fica explícito no log;
- nenhum nome novo de evento pode virar faucet apenas por ser chamado no código.

Também foram removidos faucets proporcionais à população online em caminhos ativos:

- WeeklyPKWar: usa recompensa fixa de **2.500 CP**, coerente com a constante e a mensagem do próprio evento;
- CouplesTournament: deixa de depender de chave inexistente e usa a constante explícita de **2.500 CP**;
- ClanWar: Silver deixa de ser multiplicado por Pool.GamePoll.Count; base fixa de **3.500.000 Silver** para líder e metade para membro.

A V5 não declara que todos os valores de reward existentes são historicamente exatos para 5017. Ela garante que os faucets sejam **explícitos, finitos, rastreáveis e sem fallback/population multiplier oculto**. Ajuste fino de rates pode ser feito depois a partir da telemetria.

## Auditoria global CP/Gold

Execute:

~~~powershell
python tools/era1_economy_v5_audit.py --check
~~~

O auditor percorre todos os .cs do GameServer e gera:

- docs/catalogo/era1_v5_audit.csv
- docs/catalogo/era1_v5_currency_mutations.csv
- docs/catalogo/era1_v5_event_reward_keys.csv
- docs/catalogo/era1_v5_population_scaled_faucets.csv

Cada mutação direta de Money, ConquerPoints e BoundConquerPoints recebe path, linha, operação, expressão, classificação e feature/decision/origin do Features5017.json.

O --check falha quando uma invariável crítica da V5 reabre, incluindo:

- fallback implícito de reward;
- reward de evento ativo escalado por população;
- PK claim sem remove-first;
- redemption sem remove-first;
- trade sem overflow/conservation guards;
- Guild/PK classificados incorretamente na telemetria;
- lottery deixando de ser ticket-backed.

## Self-test

era1-selftest agora inclui Era1Faucets.RunSelfTest(), cobrindo constantes, overflow e classificação dos dois lados do PK redemption.
