# ECONOMY V4 — Market, vending, warehouse e NPC service sinks

## Objetivo

A Economy V4 fecha a camada econômica que não passa por `Shop.dat`: stalls de jogadores,
armazém de Silver, teleporte por Conductress e serviços NPC com preço fixo. A regra é
preservar sinks clássicos documentados e não inventar taxas novas sem fonte histórica.

## Fontes históricas

- Market / player vending:
  https://co.99.com/guide/guides/market.shtml
- Warehouse:
  https://co.99.com/guide/guides/warehouse.shtml
- Conductress / 100 Silver:
  https://co.99.com/guide/faq/npcs.shtml
- Exemplo contemporâneo de 2007 confirmando 100 Silver inclusive para o Market:
  https://co.99.com/content/2007-11-06/20071106213305713.shtml
- Barber / Beautician / character customization:
  https://co.99.com/guide/guides/customize.shtml
- Guild creation / 1,000,000 Silver:
  https://co.99.com/guide/guides/guild1%2C1.shtml
- Patch 5072, que introduziu remote warehouse como privilégio VIP:
  https://co.99.com/content/2008-11-10/20081110181059016.shtml

## Market e vending

O Market clássico permanece em `map 1036` e aceita stalls de jogadores com preço em
Silver ou CP. Não foi criada taxa artificial sobre a transação: `PlayerVendingTaxBasisPoints = 0`.

### Remoção dos static booths 5695/custom

`Database5700/Booths.txt` contém uma camada de lojas estáticas de oferta infinita, incluindo
mounts, acessórios, Souls e outros conteúdos posteriores. Essa camada não corresponde ao
vending clássico entre jogadores e cria uma superfície de MINT/arbitragem.

Na Era 1:

- `Booth.Load()` fica atrás de `EnablePost5017StaticBooths = false`;
- os arquivos permanecem no repositório para compatibilidade/auditoria;
- stalls reais de jogadores continuam ativos;
- o jogador só pode criar booth no Market clássico;
- itens bloqueados pela boundary Era 1 não podem ser anunciados;
- preço zero é rejeitado.

### Atomicidade da venda

O fluxo antigo transferia Gold/CP antes de remover o anúncio do stall. Dois compradores
concorrentes podiam passar na validação e pagar pelo mesmo item.

A V4 muda a ordem:

1. valida moeda, saldo e overflow do vendedor;
2. remove o anúncio com `TryRemove`;
3. somente o comprador que removeu o anúncio transfere a moeda;
4. o item é movido para o inventário do comprador;
5. o item é removido do inventário do vendedor.

Gold e CP permanecem `TRANSFER`, não MINT/BURN.

## Warehouse

A documentação clássica permite depósito/saque de Silver e itens e não descreve uma taxa
de armazenamento. Portanto:

- `WarehouseFeeSilver = 0`;
- não existe BURN artificial por depósito/saque;
- Silver continua compartilhado entre warehouses, como no comportamento clássico;
- o jogador precisa estar fisicamente junto a um Warehouse NPC;
- `ShowWarehouseMoney`, `DepositWarehouse` e `WarehouseWithdraw` aplicam a mesma boundary;
- o pacote de itens `MsgWarehouse` aplica a mesma regra para `Show`, `DepositItem` e `WithdrawItem`;
- variantes posteriores `House`, `Sash` e Poker-style são rejeitadas antes de acessar o storage;
- o pacote de Silver aceita tanto o NPC ecoado no `id` quanto `ActiveNpc`, mas a proximidade real
  continua obrigatória;
- o pacote de itens exige o `NpcID` clássico explícito e em tela;
- depositar todo o Silver carregado é permitido (`>=`, não o antigo `>`);
- valores zero, casts acima de `uint` e overflow no saque são rejeitados.

O acesso remoto de warehouse fica desativado. O patch 5072 é uma evidência direta de que
remote warehouse era um privilégio VIP posterior ao alvo da Era 1.

## Teleport e demais serviços NPC

Valores clássicos preservados na política:

| Serviço | Custo |
| --- | ---: |
| Conductress entre cidades / Market | 100 Silver |
| Arena Guard | 50 Silver |
| Barber - hairstyle clássico | 500 Silver |
| Barber - New Dynasty | 10,000 Silver |
| Beautician / avatar | 500 Silver |
| Criação de guilda | 1,000,000 Silver |

As cinco Conductresses principais já debitavam corretamente 100 Silver no código legado e
permanecem assim. O Guild Creator também já queimava 1,000,000 Silver e o Barber já usava
500/10,000 Silver.

O `ArenaGuard` atual não executa a rota clássica de entrada; por isso a V4 não inventa um
mapa de destino. O valor de 50 Silver fica registrado na policy para quando a rota clássica
for restaurada com fonte/coordinates confiáveis.

## Arbitragem CP <-> Gold

A Era 1 não define uma exchange rate server-side entre CP e Silver.

É permitido que jogadores formem uma taxa implícita pelo preço dos seus próprios itens no
Market. Isso é mercado P2P e deve permanecer.

São proibidos na camada de servidor:

- NPC/static booth que ofereça supply infinito capaz de fechar um loop CP -> item -> Gold;
- serviço que debite CP e credite Gold diretamente;
- serviço que debite Gold e credite CP diretamente;
- venda duplicada por race condition;
- overflow de saldo durante vending.

A remoção de `Booths.txt` da runtime elimina a principal oferta fixa custom encontrada nesta
auditoria.

### CP Admin clássico

O `MarketCpAdmin` permanece com a conversão histórica DragonBall -> CP:

- 1 DragonBall -> 215 CP;
- 1 DragonBall Scroll -> 2.150 CP;
- o Mall da Era 1 vende 1 DragonBall por 215 CP.

Assim, DB <-> CP fica em paridade nominal e não cria lucro circular server-side. A existência
do CPAdmin no Market para converter Dragon Balls em CP é documentada oficialmente em 2007.

No mesmo handler existia uma opção 3 custom escondida no diálogo, porém acionável por pacote
forjado, que tentava consumir 999.999.999 Silver para criar o item 3400146. A V4 bloqueia
essa opção na fronteira dos dois packets NPC, antes de o handler ser executado.

## Telemetria

`EconomyTelemetry.json` classifica:

- warehouse como `TRANSFER`;
- booth como `TRANSFER`;
- Conductress como `Teleport` / BURN;
- Barber como `Appearance` / BURN;
- Guild Creator como `GuildCreation` / BURN;
- NPC shop/repair permanecem nas categorias da Economy V3.

Com isso, o resumo MINT/BURN passa a separar melhor os sinks de Silver fora de `Shop.dat`.

## Auditor automático

Execute:

```bash
python tools/era1_economy_v4_audit.py --check
```

O script valida os gates — incluindo storage de Silver e itens, atomicidade do vending e bloqueio das variantes pós-5017 — e gera:

- `docs/catalogo/era1_v4_service_audit.csv`
- `docs/catalogo/era1_v4_static_booths.csv`
- `docs/catalogo/era1_v4_arbitrage_findings.csv`
- `docs/catalogo/era1_v4_npc_currency_sites.csv`

O CI executa o auditor antes do build. Qualquer regressão estrutural da policy faz o pipeline
falhar antes de promover a build.
