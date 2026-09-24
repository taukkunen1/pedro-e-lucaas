# Era 1 — linha do tempo 5017

Alvo: **Patch 5017 (12/03/2008)**. Tudo que entrou depois fica fora do gameplay.

Referência: patch notes compilados pela comunidade (CptSky, cooldown.dev). Não é o
changelog oficial completo; o cliente 5017 é a prova definitiva quando houver dúvida.

| Patch | Data | O que entrou | Situação no servidor |
|---|---|---|---|
| 5016 | 05/03/2008 | Nobreza | mantido (anterior ao alvo) |
| 5022 | 25/04/2008 | Mentor/Aprendiz, itens suspeitos, item lock, trade partners, contas de mercador, cadeia sem perder itens travados, pagamento pelo Warden Zhang, limite de 1000 PK points, nível 137 | `social.mentor`, `social.tradepartner`, `core.itemlock` removidos; `Suspicious` zerado ao carregar; sem limite de 1000 PK (só proteção contra estouro); nível máximo 130 (`Era1Progression.MaxLevel`) |
| 5035 | 23/07/2008 | Escudos 120, plumas e headbands | bloqueados em `Era1Items` (tipos 141/142/143 e escudos nível 120+) |
| 5066 | 19/09/2008 | Composição por pontos, detenção de itens de red/black | detenção removida: equipamento cai no chão; RedeemGear/ClaimGear só atendem registros antigos |
| 5072 | 12/11/2008 | Talismãs (Fan/Tower), ranking de flores | talismãs bloqueados em `Era1Items`; `social.flowers` removido |
| 5089 | 19/12/2008 | Ninja | criação só aceita Trojan, Warrior, Archer e Taoist; skills bloqueadas em `Era1Skills` |
| 5127 | 20/05/2009 | Enlightenment, interações, canal World, lista de quests | `progression.enlight` removido |
| 5155 | 16/07/2009 | Montarias, Clans, Frozen Grotto 1-2 | itens de montaria já bloqueados; skills Riding/Spook/WarCry bloqueadas; `social.clan` e `events.clanwar` removidos; mapas do Frozen Grotto bloqueados em `Era1Maps` |
| 5160 | 05/08/2009 | Remoção das Guild Branches | branches existiam no 5017, mas o cliente 5695 não tem interface para elas (ver abaixo) |
| 5171 | 23/09/2009 | Demon Boxes | `economy.demonbox` removido (caixas e CP Packs bloqueados) |
| 5180 | 04/11/2009 | Martial Arsenal, Arena | `social.arsenal` removido (BP 0, inscrições liberadas); Arena mantida por decisão do projeto |
| 5212 | 01/02/2010 | Refinery, Stabilization, Horse Racing | Refinery/Stabilization já desligados; `events.race` removido (NPCs, packs de pontos e mapa 1950) |
| 5250 | 27/04/2010 | Frozen Grotto 3-6, Sub-Class, acessórios de arma | Frozen Grotto e Sub-Class bloqueados; acessórios bloqueados em `Era1Items` |

## Regras de PK (5017)

- Kill comum: 10 PK points; inimigo pessoal: 5; guild inimiga: 3.
- Red name (30+) pode perder equipamento; black name (100+) perde equipamento e vai para a cadeia ao ser morto por outro jogador, guarda ou patrulha (`Patroller`); o black name morto por guarda/patrulha também derruba equipamento. A cadeia já existia antes do 5022; o 5022 só acrescentou a proteção de itens travados.
- Sem detenção: o equipamento cai no chão.

## Agendador

Marcar um evento como `remove` só desregistra os pacotes. O agendador (`MsgSchedules.cs`) também checa `FeatureRegistry.IsKept` para: minigames customizados e Treasure Thief (`events.custom-minigames`), Pole Domination, Team/Skill Team PK (`events.skilltournament`), Classic Clan War e Elite Guild War.

## Removidos por decisão do projeto (sem data confirmada)

VIP, Elite PK, Capture the Flag, Team Arena, Class PK War, Fortress War, Couples PK, Quiz Show, Casas, Surprise Box e Lucky Bag. Auto Hunting mantido.

## Estátuas

Todas as estátuas (Elite PK e de guild) guardam o pacote de spawn no momento em que são criadas, têm o nome do jogador e são salvas em `StaticStatue.txt` (uma por linha: pacote, UID, X, Y, mapa, HP máximo, HP, estática, guild). Quando outra guild vence a Guild War, as estátuas das outras guilds no mapa 1038 saem.

## Migração de login

`Era1Migration` roda ao carregar o personagem: equipamento posterior ao 5017 sai do corpo e vai para o inventário (ou para o armazém de Twin City, sem espaço); itens de sistemas removidos (Demon Boxes, CP Packs, Lucky Bag, packs de corrida, itens de SubClass/Chi) são apagados do inventário e dos armazéns. Tudo gera uma linha `[Era1Migration]` no log para reembolso manual.

## Conferência de sistemas mantidos (2026-09-24)

- PK War semanal/mensal: evento clássico (General Bravery, sábado 20h e primeiro dia do mês). Mantido.
- Lava Beast: posterior (notícia oficial de maio de 2010), mantida por decisão do projeto. Como o Frozen Grotto está bloqueado, passou a nascer no Labyrinth 4 (mapa 1354, `MsgSchedules.LavaBeastMap`).
- City War: nenhuma fonte oficial encontrada; o evento com esse nome aparece em servidor privado. Removido (`events.citywar`).
- Rankings: o ranking de nobreza (5016) é anterior ao 5017; os rankings de flores e chi já saíram com seus sistemas.

## Pendências conhecidas

### Testes no jogo (a fazer pela equipe)

- [ ] Red/black name morto por jogador: equipamento cai no chão (sem Warden Zhang).
- [ ] Black name morto por guarda ou `Patroller`: derruba equipamento e vai para a cadeia.
- [ ] Criação de clan recusada; clan não aparece no login; sem BP de clan e de mentor.
- [ ] Frozen Grotto (1762, 1926, 1927, 1999, 2054-2056) e mapa 1950 recusam teleporte; quem logar dentro volta para Twin City.
- [ ] Migração no login: equipamento posterior ao 5017 vai para inventário/armazém; Demon Boxes, CP Packs e Lucky Bag somem (linha `[Era1Migration]` no log).
- [ ] Estátua de guild continua no lugar depois de reiniciar; estátuas da guild anterior saem quando outra vence a Guild War.
- [ ] Eventos removidos não aparecem no horário (minigames, Pole Domination, Elite GW, CTF, Elite PK, Class PK, Fortress, Couples, Team/Skill PK, City War, KnightGame, Nobility Tournament).
- [ ] Lava Beast nasce no Labyrinth 4 e o anúncio aponta para lá.
- [ ] Auto Hunt: Jump só para VIP 3+ e Pickup só para VIP 4+, usando o nível VIP salvo (`@vip`).

### Decisões do projeto

- Guild Branches: ficam como estão (o cliente 5695 não tem a tela).
- Valores de recompensa: calibrar depois, com a telemetria da V5.
- Comandos de PM que iniciam eventos removidos continuam funcionando (uso manual de administração).

### Dependem do cliente 5017

- Animação do pulo.
- Validação do protocolo e da interface.
