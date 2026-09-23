# ECONOMY V3 — Shopping Mall, NPC shops e preços 5017

## Objetivo

A Economy V3 fecha a camada de comércio da Era 1 sem cometer o erro de remover Conquer Points.
O Shopping Mall já existia na janela clássica: a política correta é manter CP como moeda e
limitar o catálogo/serviços ao que pertence ao período.

Fonte histórica principal:

- Conquer Online oficial, "Shopping Mall", 21/10/2008:
  https://co.99.com/content/2008-10-21/20081021013424194.shtml
- Patch 5002, 26/11/2007, documentando Tough Drill/Star Drill e o sistema clássico de socket:
  https://co.99.com/content/2007-11-26/20071126010629018.shtml

## Shopping Mall ativo

A fronteira server-side está em `Game/Era1/Era1Shops.cs`.
Os arquivos 5695 continuam no repositório para compatibilidade, mas o catálogo carregado e
as compras são filtrados novamente no servidor.

Preços históricos fixados:

| Item | CP |
| --- | ---: |
| DragonBall | 215 |
| Meteor | 13 |
| Refined Moon Gem | 45 |
| Refined Rainbow Gem | 65 |
| Refined Dragon Gem | 65 |
| Refined Phoenix Gem | 65 |
| Refined Violet Gem | 45 |
| Refined Fury Gem | 45 |
| Refined Kylin Gem | 35 |
| Black Tulip | 215 |
| Miraculous Gourd | 4050 |
| Magical Bottle | 1870 |
| Exp Potion | 27 |
| Celestial Stone | 150 |
| Ninja Amulet | 215 |
| Lucky Amulet | 75 |
| Exemption Token | 1890 |
| Super Tortoise Gem | 780 |
| Exp Ball | 27 |
| Tough Drill | 1890 |
| +3 Stone | 108 |
| +4 Stone | 324 |
| +5 Stone | 972 |
| +6 Stone | 2916 |

Garments clássicos do catálogo legado que também aparecem na lista oficial são mantidos:
RoyalDignity, AngelicalDress, ColorfulDress, PrairieWind, SongofTianshan, SouthofCloud,
BonfireNight, WeddingGown, GoodLuck, Phoenix, Elegance, Celestial e DarkWizard por 675 CP;
MoonOrchid, DreaminFlowers e BlueDream por 980 CP.

### Itens deliberadamente fora da venda direta

- +1 Stone e +2 Stone não aparecem no catálogo oficial de 21/10/2008 e não são vendidos.
- Star Drill não é vendido diretamente. O Patch 5002 documenta que ele nasce quando o
  Tough Drill falha; 7 Star Drills garantem o segundo socket.
- Itens/serviços 5695 como Steed, Stabilization, Monk, Sash, packs modernos e recursos de
  Bound CP permanecem no dataset legado, porém não entram na economia Era 1.

O preço é validado por uma tabela server-side e também normalizado em `ItemType.DBItem.Parse`.
Isto corrige divergências do dataset 5695, principalmente Tough Drill 1990 -> 1890 e
garments clássicos de 980 CP armazenados com preço zero.

## NPC shops / Gold

Compras em Gold continuam usando `Shop.dat`, porém a fronteira rejeita equipamentos e itens
bloqueados pela política Era 1.

A venda para NPC foi endurecida: o pacote `SellItem` só é aceito quando o UID resolve para
um shop real de `Shop.dat` com `MoneyType=Gold`. Antes, qualquer NPC próximo podia receber
o pacote de venda, criando uma superfície de exploit e um MINT de Gold fora do shop real.

O valor de venda continua baseado em `GoldWorth` e proporcional à durabilidade restante.
Não foi inventado um multiplicador novo sem fonte histórica confiável.

## Reparos e Gold sinks

O reparo normal continua ativo e é um BURN de Silver. A fórmula existente escala o custo
por valor-base, perda de durabilidade e qualidade do item.

Durabilidade zero continua exigindo 5 Meteors, portanto entra no BURN de Meteor da Economy V2.

`RepairItemVIP` foi bloqueado na Era 1. A documentação oficial descreve reparo normal em
Pharmacist/Storekeeper/Armorer/Blacksmith por Silver e reparo de equipamento gasto pelo
Equipment Blacksmith usando Meteors; não há motivo para manter o atalho VIP da base 5695.

## Serviços pós-5017 neutralizados

- Honor Shop / Race Point Shop / Champion Shop: handlers preservados, interceptação desativada.
- Bound CP mall: desativado.
- `MsgOsShop` (Bound CP / fallback para CP normal, item 725065): bloqueado na Era 1.
- Garment exchange por pontos (`GarmentShop` packet): desativado; garments clássicos são
  compras CP normais do Shopping Mall.
- Forging-shop purchase: reaproveita a mesma whitelist/preço histórico do Mall.
- Talisman socket por CP/item, refinery/purification/stabilization e degrade por 54 CP
  permanecem bloqueados pelas fases anteriores.

## MINT / BURN

As compras CP e Gold continuam passando pelos setters de `Player.ConquerPoints` e
`Player.Money`, portanto entram na telemetria existente como BURN com o escopo de
`ItemUsage`.

A leitura operacional para V3 deve separar:

- CP BURN: Shopping Mall clássico.
- Gold BURN: compras em NPC, reparos e demais serviços clássicos.
- Gold MINT: venda para NPC.
- Resource BURN: Meteors/DBs/+Stones/gems/drills consumidos nos fluxos da Economy V2.

A Economy V3 não altera faucets de hunting/mineração; ela fecha a camada comercial sobre a
economia já auditada nas V1/V2.
