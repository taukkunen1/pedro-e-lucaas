# Mapa de sistemas — TrinityConquer

Gerado a partir da leitura da estrutura do código (pastas/arquivos). Serve de base para marcar o que existia na versão 5017 e o que não existia.
Números: ~465k linhas de C# no total (GameServer ~252k).

## 1. Projetos (COServer/)

| Projeto | Tipo | Papel | Tamanho |
|---|---|---|---|
| DFGameServer | Exe net10.0 | Servidor de jogo (toda a lógica) | 460 arquivos / ~252k linhas |
| DFAccountServer | Exe net10.0 | Autenticação/login (MongoDB `cq_auth`) | 36 / ~3,4k |
| DFAPI | Web API net10.0 | Entrega os configs e dados (ex.: Characters) ao GameServer/Site | 124 / ~43k |
| DFCore | Biblioteca | Config compartilhada, INI, REST, `DatabasePaths` | 119 / ~7,4k |
| ConquerSite | ASP.NET | Site (Home, Account) | 16 |
| ConquerLoader | WinExe .NET 4.8 | Loader do cliente (vai para Placebo) | 6 |
| OpenSourceLoader5693 | C++ / C# | Hook + Injector do cliente 5693 | — |

Fluxo: Cliente → AccountServer (login) → GameServer. GameServer e Site pegam config/dados via API. Build vai para `Placebo\<projeto>\`.

## 2. GameServer — sistemas por pasta

**Base (infra):** sockets/criptografia, pool de threads, console, config (`ServerConfig`), aleatório, janela WinAPI, Poker, Mining, Booth (barracas), cache de atributos/pacotes, MapDictionary, arena ilimitada, chat items.

**Role (entidades):** Player, RoleStatus, RoleView, GameMap, SobNpc, Statue, StaticRole, FloorSpell, KOBoard, Flags, ClientTransform, Instance.

**Database (carga/persistência):** ItemType, MagicType, RebornInfomations, NpcServer, GuildTable, ClanTable, HouseTable, NobilityTable, FlowersTable, ArenaTable, TeamArenaTable, ChiTable, ConfiscatorTable, TheCrimeTable, Lottery, QuizShow, QuestInfo, RoleQuests, TutorInfo, ShareVIP, SubProfessionInfo, Tranformation, Disdain, DBLevExp, InfoDemonExterminators, AchievementCollection, SystemBanned/BannedAccount, GroupServerList, Shops, DBActions, DbContext, ServerDatabase (salvar/carregar personagem).

**Game/MsgServer (pacotes, 197 arquivos) — agrupado por sistema:**
- Login/sessão: HandShake, LoginClient, ServerInfo, ServerConfig, ServerTimer, Tick, SecondaryPassword, Machine/MachineResponse
- Personagem: NewRole, ClientInfo, NameChange, Status, UpdatePacket, Proficiency, Reincarnation, SubClass, Title, Nobility, Achievement, Chi (ChiInfo), Enlight
- Combate: AttackPacket/AttackHandler, Spell, SpellAnimation, PkExploit, Blackspot, Movement, InterAction, Weather, MapStatus, MapTraps
- Itens: GameItem, ItemExtra, ItemLock, ItemUsuagePacket, UpdateItem, EmbedSocket, MemoryAgate, DetainedItem, Warehouse, SurpriseBox, ShowEquipment, RacePotion
- Social: Team (+Leadership, MemberInfo, TeamPk), Trade (+Partner), Clan, Guild (Information, Members, Profile, Ranks, Arsenal, MinDonations), MentorApprentice, KnownPersons, Flower, FamilyOccupy, CountryFlag, GenericRanking
- Economia/loja: OsShop, Advertise (+Gui), VipHandler/VipStatus, Claim
- Eventos/torneios (pacotes): Arena, ElitePk, QuizShow, RaceRecord, OfflineTG (+Stats)
- Comunicação: Message, Broadcast/BroadcastList, StaticMessage, PopupInfo, StringPacket, GameUpdate, QuestData/QuestList, Gui

**Game/MsgTournaments (torneios/eventos, 37):** CityWars, ClanWar/ClassicClanWar, GuildWar/EliteGuildWar, ClassPKWar, PkWar, FortressWars, CaptureTheFlag, LastManStand, KingOfTheHill, PoleDomination (BI/DC/PC), TeamArena, TopFight, SkillTournament, QuizShow, TreasureThief, PassTheBomb, FrozenSky, Fivenout, KillerSystem, Couples, Arena, CheckLine.

**Game/MsgNpc:** Npc, NpcHandler (~60k linhas: todos os diálogos/quests), Dialogs, NpcReply, StaticGUI. **MsgMonster:** mobs, drops, família, geração de item, settings. **MsgFloorItem:** itens no chão. **MsgEvents:** DragonWar.

**Extras (marcar contra 5017):** MsgInterServer (multi-servidor via pipes), AutoHunting/Catching, Bot (AI/Dynamic/Equipment), Poker, Mining, Lottery, Chi, SubClass, Achievement, Title, Machine, OfflineTG, OsShop.

**MadeByDaRkFox (ferramentas do autor):** BulletinManager, DataManager, DropConfiguration, EventsRewards, ItemsByTime, MapManager, SmartNPCManager, SquamaManager, SqlMigrator, FixMonstersFromMonsterDat, CharacterCreationDefaultSet.

## 3. Database (COServer/Database5700 — 406 MB)

**Pastas:** map (1004 arquivos), maps (250), cq_generator (1888), Quests (530), Monsters (439), MobSpawns (201), ClanWar (5), shops (5), ini (3); vazias/dados de execução: Users, PlayersItems, PlayersProfs, PlayersSpells, Guilds, Clans, Houses, Backup.

**Arquivos por sistema:**
- Itens/magias: itemtype.txt/.dat, magictype.txt, magictypeop.txt, ItemAdd.ini, Refinery.txt, databaserefineryboxes.txt, Souls1-7.ini, Stats.ini, furnitures.txt
- Mobs/NPC/mapa: monster.txt/.dat, Spawns.txt, Npcs.txt, SobNpcs.txt, Traps.txt, portals.ini, GameMapEx.ini, StaticStatue.txt, Booths.txt
- Torneios/guerras: Arena.ini, TeamArena.ini, Elite.ini, ElitePk.ini, ClassPkWar.ini, CityWar.ini, GuildWarInfo.ini (+GuildWarMap.bmp), SkillTeamPK.ini, CouplesPK.ini, PokerTables.ini/.txt
- Progressão/quests: levexp.txt, Questinfo.ini, SubProfessionInfo.ini, TransformInfo.txt, Associate.txt, Share.txt, cq_tutor_*.txt, quizquestins.ini, lottery.ini
- Regras/segurança: BadMsg.txt (movido para cá), BanIp.txt, BanUID.txt, Crime.ini, cq_disdain.txt
- Cliente/ranking: client_config.ini, KOBoardRanks.dat/.txt, RedeemContainer.bin
- Ferramentas: DatCryptor.exe, Magic.exe

## 4. Fluxo da Database

- Todo acesso do GameServer passa por `ServerConfig.DbLocation` (ItemType, MagicType, Npcs, Users, GameMapEx, levexp, BadMsg...). O valor vem da API e é resolvido por `Core.DatabasePaths` para `COServer\Database5700` (caminho embutido no build; `TRINITY_DATABASE` tem prioridade).
- A API lê da Database só o endpoint de personagens (`Users\<id>.ini`).
- Armazenamento: arquivos (INI/TXT/.dat) na Database; auth (contas, servidores, votos, online, configurações) em MongoDB via `AuthMongo` na API; contas com hash PBKDF2-SHA256 (`Core.Security.PasswordHasher`, conta antiga em texto puro é convertida no primeiro login). O modo `DbFromFiles=false` (dados do jogo pela API) também usa MongoDB (`GameDbContext` com o provider oficial `MongoDB.EntityFrameworkCore`); MySQL não é mais usado. Importação: `tools/migrate_mysql_to_mongo.py`.
- Fora da Database (não é dado do jogo): 3 gravações de debug em `C:\PacketSniffing\` (MyConsole.cs, MsgMessage.cs) e os logs de exceção, que vão para a pasta do executável (Placebo\GameServer).

## 5. Classificação 5017 (sistema de identificação)

- Fonte única: `COServer/Database5700/Features5017.json` (61 features; cada uma com `origin`, `confidence`, `decision`, `paths`, `dbFiles`, `note`).
- Origens: `Original5017`, `Posterior`, `Custom` (invenção do autor), `Verify` (sem certeza), `Base` (infra).
- `decision` (`keep` / `remove` / `review`) é editável à mão; o padrão é `keep` para Original/Base e `review` para o resto.
- Regerar/atualizar: `python3 tools/build_features.py` (preserva decision/note editados). Cobertura: `python3 tools/feature_scan.py --report` (hoje 460/460 arquivos do GameServer classificados, 0 conflitos).
- No código: `Core.Features.FeatureRegistry.Get("progression.chi")`, `IsOriginal5017(id)`, `IsKept(id)` e o atributo `[Feature("id")]` para marcar classes/métodos. O GameServer imprime o resumo ao iniciar.
- Atenção: a classificação inicial vem do conhecimento da linha do tempo do jogo, não de um cliente/servidor 5017 real; itens com `confidence: low` ou `origin: Verify` precisam ser conferidos.

### Gates (implementação)
Com `python3 tools/features_set.py <id> remove` (e reiniciar o GameServer), a feature é desligada por:
1. **Pacotes e NPCs:** `CachedAttributeInvocation` não registra handlers de tipos da feature (`types`) nem de NPCs listados (`npcs`) — o pacote/NPC passa a ser ignorado.
2. **Torneios/eventos:** `Open()`/`Start()` das classes de `Game/MsgTournaments` começam com `IsKept("<id>")` (marcados `// [feature-gate]`). Os torneios sem `Open()` único têm o gate no ponto de entrada: `Start()` de GuildWar, EliteGuildWar, PoleDomination, CityWars e FortressWars, e `CheckGroups/CreateMatches/VerifyMatches` da TeamArena.
3. **Toggles do config:** `EnabledChi`, `EnabledSubclass`, `EnabledMentor` e `IsInterServer` só ficam ligados se a feature não for `remove`.
4. **Itens e quests que só existem em dados:** cada feature pode listar `items` e `quests` (ids) no `Features5017.json`. Com `remove`, o item não entra no inventário (`Add`, `AddItemWitchStack`, `AddMinute`, `AddAccess`, `AddReturnedItem`), não pode ser usado (`UseItem`) e sai das lojas (EShop, ShopFile, HonorShop, RacePointShop, podado ao iniciar em `Server.cs`); a quest não pode ser aceita (`Quests.Accept` e `AcceptKingDomMission`). Hoje: `progression.subclass` bloqueia os itens 723342, 723094, 720774 e 720775 (study points); `progression.chi` bloqueia 729304, 729476-729479, 729572, 729659 e 729660. A lista de quests está vazia (nenhuma quest exclusiva de sistema removido foi achada). O `tools/build_features.py` preserva essas listas.

Ainda sem gate: itens fora das listas acima (por exemplo o ChiToken 3003747, usado por um NPC por outro caminho). O comando de chat `bot`/`abot` já tem gate (`extras.bot`). NPCs continuam aparecendo no mapa (vêm do `Npcs.txt`), mas não respondem.
`tools/features_set.py --list [origem]` lista as decisões. Decisões `remove` hoje: progression.chi, progression.subclass, progression.achievements, progression.titles, extras.offlinetg, economy.osshop, events.custom-minigames, extras.bot, extras.interserver. Os seis torneios acima seguem `keep`/`review`, então nada muda até você decidir.


## 6. Plataforma

Alvo `net10.0` (API, AccountServer, Core, GameServer, ConquerSite); loaders em .NET Framework 4.8. EF Core 10 com `MongoDB.EntityFrameworkCore` 10.0.4 e `MongoDB.Driver` 3.12 (sem Pomelo/MySQL). AutoMapper segue em 14.0.0: a 16 exige mudar o código e licença. O CI usa `dotnet-version: 10.0.x`; a máquina de build precisa do SDK do .NET 10.
