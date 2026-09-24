#!/usr/bin/env python3
"""Gera COServer/Database5700/Features5017.json (registro de sistemas x versao 5017).
Origem: Original5017 | Posterior | Custom (invencao do autor do servidor) | Verify (nao sei) | Base (infra, nao se aplica)
Confianca: high | medium | low  -> baseada no conhecimento da linha do tempo do jogo, NAO verificada num cliente 5017 real.
Se o JSON ja existir, preserva 'decision' e 'note' editados a mao.
"""
import json, os
ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'COServer')
OUT = os.path.join(ROOT, 'Database5700', 'Features5017.json')
M = 'DFGameServer/Game/MsgServer/'
def F(id, name, origin, conf, paths, db=None, note=''):
    return dict(id=id, name=name, origin=origin, confidence=conf, decision=('keep' if origin in ('Original5017','Base') else 'review'),
                paths=paths, dbFiles=db or [], note=note)
S = 'DFGameServer/'
FEATURES = [
 F('base.infra','Infraestrutura (sockets, threads, console, config)','Base','high',[S+'Base/**',S+'Program.cs',S+'Client/**',S+'Models/**',S+'Properties/**',S+'Role/**',S+'MsgFilter.cs',S+'Game/GamePackets.cs',S+'MapGroupThread.cs',S+'MsgSchedules.cs',S+'Database/Server.cs',S+'Database/ServerDatabase.cs',S+'Database/DataCore.cs',S+'Database/GameInfo.cs',S+'Database/DbContext/**',S+'Database/DBActions/**'],['client_config.ini']),
 F('base.tools','Ferramentas do autor (managers, migrator)','Base','high',[S+'MadeByDaRkFox/**']),
 F('base.moderation','Banimentos / crime / filtro','Base','high',[S+'Database/SystemBanned*.cs',S+'Database/TheCrimeTable.cs',S+'Database/Disdain.cs'],['BanIp.txt','BanUID.txt','Crime.ini','BadMsg.txt','cq_disdain.txt']),
 F('core.login','Login / sessao / handshake','Original5017','high',[M+'MsgHandShake*',M+'MsgLoginClient*',M+'MsgServerInfo*',M+'MsgServerConfig*',M+'MsgServerTimer*',M+'MsgTick*',M+'MsgSecondaryPassword*',S+'Database/GroupServerList.cs']),
 F('core.character','Personagem (criacao, atributos, proficiencia, magias)','Original5017','high',[M+'MsgNewRole*',M+'MsgClientInfo*',M+'MsgNameChange*',M+'MsgStatus*',M+'MsgUpdatePacket*',M+'MsgAtributeSet*',M+'MsgProficiency*',M+'MsgUpdateProfExperience*',S+'Database/AtributesStatus.cs',S+'Database/DBLevExp.cs',S+'Database/ClientProficiency.cs',S+'Database/ClientSpells.cs'],['levexp.txt','Stats.ini']),
 F('core.combat','Combate e magias','Original5017','high',[M+'MsgAttackPacket*',M+'AttackHandler/**',M+'MsgSpell*',M+'MsgPkExploit*',M+'MsgBlackspot*',S+'Database/MagicType.cs'],['magictype.txt','magictypeop.txt']),
 F('core.world','Mundo: movimento, mapas, clima, itens no chao','Original5017','high',[M+'MsgMovement*',M+'MsgInterAction*',M+'MsgWeather*',M+'MsgMapStatus*',M+'MsgMapTraps*',M+'MsgFlagIcon*',S+'Game/MsgFloorItem/**'],['GameMapEx.ini','portals.ini','Traps.txt']),
 F('core.items','Itens, equipamento, armazem, sockets','Original5017','high',[M+'MsgGameItem*',M+'MsgItemUsuagePacket*',M+'MsgUpdateItem*',M+'MsgItemView*',M+'MsgShowEquipment*',M+'MsgWarehouse*',M+'MsgDetainedItem*',M+'MsgEmbedSocket*',S+'NewItems.cs',S+'Database/ItemType.cs',S+'Database/ClientItems.cs',S+'Database/ConfiscatorTable.cs',S+'Database/Shops/**'],['itemtype.txt','itemtype.dat','ItemAdd.ini','Refinery.txt','Souls*.ini']),
 F('core.itemextra','Item extra (Refinery / Purification / Stabilization)','Original5017','low',[M+'MsgItemExtra*']),
 # core.itemlock sem paths de proposito: o handler MsgItemLock precisa continuar registrado
 # para o unlock funcionar. O gate e' em codigo (MsgItemLock.LockEnabled).
 F('core.itemlock','Item lock','Verify','medium',[]),
 F('core.npcs','NPCs, dialogos, monstros e drops','Original5017','high',[S+'Game/MsgNpc/**',S+'Game/MsgMonster/**',S+'Database/NpcServer.cs'],['Npcs.txt','SobNpcs.txt','monster.txt','Spawns.txt']),
 F('core.quests','Quests','Original5017','high',[M+'MsgQuestData*',M+'MsgQuestList*',S+'Database/QuestInfo.cs',S+'Database/RoleQuests.cs'],['Questinfo.ini']),
 F('core.communication','Chat, broadcast, mensagens, GUI','Original5017','high',[M+'MsgMessage*',M+'MsgBroadcast*',M+'MsgStaticMessage*',M+'MsgPopupInfo*',M+'MsgStringPacket*',M+'MsgGameUpdate*',M+'MsgDataPacket*',M+'Gui/**',S+'Game/MsgTournaments/MsgBroadcast.cs']),
 F('social.team','Equipe / lider','Original5017','high',[M+'MsgTeam*',M+'TeamPk/**']),
 F('social.trade','Troca e barracas','Original5017','high',[M+'MsgTrade.cs',M+'MsgTrade/**']),
 F('social.tradepartner','Trade partners','Posterior','high',[M+'MsgTradePartner*']),
 F('social.arsenal','Martial Arsenal da guild','Posterior','high',[M+'MsgGuildArsenal*',M+'MsgGuildFastArsenal*']),
 F('economy.demonbox','Demon Boxes / CP Packs','Posterior','high',[]),
 F('economy.luckybag','Lucky Bag (event bag)','Custom','high',[]),
 F('social.friends','Amigos / conhecidos','Original5017','high',[M+'MsgKnowPersons*',M+'MsgKnownPersonInfo*']),
 F('social.guild','Guild','Original5017','high',[M+'MsgGuild*',S+'Database/GuildTable.cs']),
 F('social.clan','Clan / familia','Original5017','medium',[M+'MsgClan.cs',M+'MsgFamilyOccupy*',S+'Database/ClanTable.cs'],note='Clan existia na era 5017, mas conferir funcoes de FamilyOccupy'),
 F('social.mentor','Mentor / aprendiz','Original5017','high',[M+'MsgMentorApprentice*',M+'MsgApprenticeInformation*',S+'Database/TutorInfo.cs'],['cq_tutor_type.txt','cq_tutor_battle_limit_type.txt'],note='Ja existe toggle EnabledMentor no config'),
 F('social.flowers','Flores','Original5017','medium',[M+'MsgFlower*',S+'Database/FlowersTable.cs']),
 F('social.nobility','Nobreza','Original5017','medium',[S+'DaysNobility.cs',M+'MsgNobility*',S+'Database/NobilityTable.cs']),
 F('social.ranking','Rankings','Original5017','medium',[M+'MsgGenericRanking*',M+'MsgCountryFlag*',S+'Role/KOBoard.cs'],['KOBoardRanks.txt']),
 F('social.houses','Casas','Verify','low',[S+'Database/HouseTable.cs']),
 F('progression.reborn','Reencarnacao','Original5017','high',[M+'MsgReincarnation*',S+'Database/RebornInfomations.cs']),
 F('progression.enlight','Enlight','Original5017','medium',[M+'MsgEnlight*']),
 F('progression.subclass','SubClass (subprofissao)','Posterior','medium',[M+'MsgSubClass*',S+'Database/SubProfessionInfo.cs'],['SubProfessionInfo.ini'],note='Ja existe toggle EnabledSubclass no config'),
 F('progression.chi','Chi','Posterior','medium',[M+'MsgChiInfo*',S+'Database/ChiTable.cs'],note='Ja existe toggle EnabledChi no config'),
 F('progression.achievements','Conquistas','Posterior','high',[M+'MsgAchievement*',S+'Database/AchievementCollection.cs']),
 F('progression.titles','Titulos','Posterior','medium',[M+'MsgTitle*']),
 F('progression.transform','Transformacao / fairy','Verify','low',[M+'MsgTransformFairy*',S+'Database/Tranformation.cs'],['TransformInfo.txt']),
 F('economy.vip','VIP','Original5017','medium',[M+'MsgVipHandler*',M+'MsgVipStatus*',S+'Database/ShareVIP.cs'],['Share.txt']),
 F('economy.osshop','Loja OsShop','Custom','medium',[M+'MsgOsShop*']),
 F('economy.advertise','Anuncios (Advertise)','Verify','low',[M+'MsgAdvertise*']),
 F('economy.lottery','Loteria','Original5017','high',[S+'Database/Lottery.cs',S+'LotteryTable.cs',S+'MsgLottery.cs'],['lottery.ini']),
 F('economy.mining','Mineracao','Original5017','high',[S+'Base/Mining/**']),
 F('economy.poker','Poker','Verify','low',[S+'Base/Poker/**',S+'Base/PokerHandler.cs'],['PokerTables.ini','PokerTables.txt']),
 F('economy.memoryagate','Memory Agate / claim','Verify','low',[M+'MsgMemoryAgate*',M+'MsgClaim*']),
 F('economy.surprisebox','Surprise Box','Verify','low',[M+'SurpriseBox*']),
 F('events.arena','Arena (1v1)','Original5017','high',[M+'Arena/**',M+'Arena Gui/MsgArena*',S+'Game/MsgTournaments/MsgArena.cs',S+'Database/ArenaTable.cs'],['Arena.ini']),
 F('events.teamarena','Team Arena','Verify','low',[M+'Arena Gui/MsgTeamArena*',S+'Game/MsgTournaments/MsgTeamArena.cs',S+'Database/TeamArenaTable.cs'],['TeamArena.ini']),
 F('events.elitepk','Elite PK','Original5017','medium',[M+'ElitePk/**',S+'Game/MsgTournaments/OfficialTournaments/MsgElite*',S+'Game/MsgTournaments/OfficialTournaments/MsgTeamElite*'],['Elite.ini','ElitePk.ini']),
 F('events.guildwar','Guild War','Original5017','high',[S+'Game/MsgTournaments/MsgGuildWar.cs'],['GuildWarInfo.ini']),
 F('events.eliteguildwar','Elite Guild War','Posterior','medium',[S+'Game/MsgTournaments/MsgEliteGuildWar.cs']),
 F('events.citywar','City War','Custom','medium',[S+'Game/MsgTournaments/CityWars.cs'],['CityWar.ini']),
 F('events.clanwar','Clan War (classica e nova)','Original5017','medium',[S+'Game/MsgTournaments/MsgClanWar.cs',S+'Game/MsgTournaments/MsgClassicClanWar.cs']),
 F('events.classpkwar','Class PK War','Verify','low',[S+'Game/MsgTournaments/MsgClassPKWar.cs'],['ClassPkWar.ini']),
 F('events.pkwar','PK War','Original5017','medium',[S+'Game/MsgTournaments/MsgPkWar.cs']),
 F('events.poledomination','Pole Domination','Verify','low',[S+'Game/MsgTournaments/MsgPoleDomination*'],['GuildWarMap.bmp']),
 F('events.ctf','Capture the Flag','Original5017','medium',[S+'Game/MsgTournaments/MsgCaptureTheFlag.cs',M+'Arena Gui/MsgCaptureTheFlag*']),
 F('events.fortress','Fortress Wars','Posterior','low',[S+'Game/MsgTournaments/FortressWars.cs']),
 F('events.quizshow','Quiz Show','Original5017','medium',[S+'Game/MsgTournaments/MsgQuizShow.cs',S+'Database/QuizShow.cs',M+'MsgQuizShow*'],['quizquestins.ini']),
 F('events.skilltournament','Skill Tournament / Skill Team PK','Verify','low',[S+'Game/MsgTournaments/MsgSkillTournament.cs',S+'Game/MsgTournaments/OfficialTournaments/MsgSkillTeamPkTournament.cs',S+'Game/MsgTournaments/OfficialTournaments/MsgTeamPkTournament.cs'],['SkillTeamPK.ini']),
 F('events.couples','Couples PK','Verify','low',[S+'Game/MsgTournaments/MsgCouples.cs'],['CouplesPK.ini']),
 F('events.demonexterminator','Demon Exterminator','Verify','low',[S+'Database/InfoDemonExterminators.cs']),
 F('events.race','Race (corrida)','Original5017','low',[M+'MsgRacePotion*',M+'MsgRaceRecord*']),
 F('events.custom-minigames','Minigames customizados','Custom','medium',[S+'Game/MsgTournaments/MsgLastManStand.cs',S+'Game/MsgTournaments/KingOfTheHill.cs',S+'Game/MsgTournaments/PassTheBomb.cs',S+'Game/MsgTournaments/Fivenout.cs',S+'Game/MsgTournaments/FrozenSky.cs',S+'Game/MsgTournaments/KillerSystem.cs',S+'Game/MsgTournaments/MsgTopFight.cs',S+'Game/MsgTournaments/MsgTreasureThief.cs',S+'Game/MsgEvents/**']),
 F('events.knightgame','KnightGame','Custom','medium',[]),
 F('events.framework','Framework de torneios','Base','high',[S+'Game/MsgTournaments/ITournament.cs',S+'Game/MsgTournaments/OfficialTournaments.cs',S+'Game/MsgTournaments/ProcesType.cs',S+'Game/MsgTournaments/TournamentType.cs',S+'Game/MsgTournaments/MsgNone.cs',S+'Game/MsgTournaments/MsgCheckLine.cs'],['ClanWar/**']),
 F('extras.interserver','InterServer (multi-servidor)','Custom','high',[S+'Game/MsgInterServer/**',M+'MsgInterServerIdentifier*']),
 F('extras.autohunting','Auto Hunting','Posterior','high',[S+'Game/AutoHunting/**']),
 F('extras.bot','Bots / IA','Custom','high',[S+'Game/Bot/**']),
 F('extras.offlinetg','Offline Training Ground','Posterior','medium',[M+'MsgOfflineTG*']),
 F('extras.machine','Slot Machine (cassino)','Verify','low',[M+'MsgMachine*',S+'Role/Instance/SlotMachine.cs'],note='Corrigido: MsgMachine e o caca-niqueis (SlotMachine), nao anti-cheat'),
]
# NPCs (nomes do enum NpcID) cujo handler deve ser desligado junto com a feature
NPCS = {
 'economy.demonbox': ['HeavenDemonBox','ChaosDemonBox','SacredDemonBox','AuroraDemonBox','DemonBox','AncientDemonBox','FloodDemonBox'],
 'events.race': ['SteedRace','SteedRaceFinish'],
 'events.knightgame': ['KnightGame','KnightGameClaim','KnightGameClaim2','KnightGameClaim3','KnightGameClaim4'],
 'progression.subclass': ['Sage','Warlock','MartialArtist','ApothecarySubClass','Performer','SubClassManager'],
 'progression.chi': ['ChiMaster','ChiToken'],
 'events.custom-minigames': ['PassTheBomb','FiveNOut','FrozenSky','KingOfTheHill','TreasureThief','LastManStand','DragonWar','TeamDeathMatch','HideNSeek','KillTheCaptain','FreezeWar','Football','KillerOfElite','ExtremePk'],
 'events.fortress': ['FortesWar'],
 'events.eliteguildwar': ['EliteGuildOfficer'],
 'events.poledomination': ['PoleDominationAC','PoleDominationBI','PoleDominationDC','PoleDominationPC'],
 'events.classpkwar': ['ClassPkEnvoy','ClassPkWar'],
 'events.couples': ['CouplesPK'],
 'events.skilltournament': ['SkillTournament','SkillTeamPkManager'],
 'economy.poker': ['PokerMillionaireLee','PokerMillinaireLee','PokerCasinoHostess','PokerWarehouseman','PokerCpsCasino','WHPoker'],
 'social.houses': ['HouseAdmin','Class6House'],
 'extras.offlinetg': ['OflineTGNpc'],
}
import re as _re, glob as _glob
def _rx(p):
    p = _re.escape(p).replace(r'\*\*', '\x00').replace(r'\*', '[^/]*').replace('\x00', '.*')
    return _re.compile('^' + p + '$')
_files = [os.path.relpath(x, ROOT).replace('\\', '/') for x in _glob.glob(os.path.join(ROOT, 'DFGameServer', '**', '*.cs'), recursive=True)]
def _types(paths):
    rxs = [_rx(x) for x in paths]; out = set()
    for fl in _files:
        if not any(r.match(fl) for r in rxs): continue
        t = open(os.path.join(ROOT, fl), encoding='utf-8-sig', errors='ignore').read()
        ns = _re.search(r'^\s*namespace\s+([\w\.]+)', t, _re.M)
        if not ns: continue
        for m in _re.finditer(r'^ {4}(?:\w+\s+)*?(?:class|struct|enum|interface)\s+(\w+)', t, _re.M): out.add(ns.group(1) + '.' + m.group(1))
    return sorted(out)
# ids de itens/quests que so existem em dados e pertencem a feature (bloqueados quando decision=remove)
ITEMS = {'economy.demonbox': [720650, 720651, 720652, 720653, 720654, 720655, 720656, 720657, 720658, 720659, 720660, 720661, 720662, 720663, 720664, 720665, 720666, 720667, 720671, 720672, 720673, 720674, 720675, 720676, 720677, 720678, 720679, 720681, 720682, 720683, 720684, 720685, 720687, 720688, 720689, 720690, 720691, 720693, 720694, 720695, 720696, 720697, 3000272, 3000273, 3000274, 3000275, 3000276], 'economy.luckybag': [728352], 'events.race': [720874, 720875, 720876, 720877], 'progression.subclass': [723342, 723094, 720774, 720775], 'progression.chi': [729304, 729476, 729477, 729478, 729479, 729572, 729659, 729660]}
QUESTS = {}
old = {}
if os.path.exists(OUT):
    for f in json.load(open(OUT, encoding='utf-8'))['features']:
        old[f['id']] = f
for f in FEATURES:
    if f['id'] in old:
        f['decision'] = old[f['id']].get('decision', f['decision'])
        if old[f['id']].get('note'): f['note'] = old[f['id']]['note']
    f['paths'] = [p for p in f['paths'] if '__none__' not in p]
    f['types'] = _types(f['paths'])
    f['npcs'] = NPCS.get(f['id'], [])
    f['items'] = ITEMS.get(f['id'], [])
    f['quests'] = QUESTS.get(f['id'], [])
doc = {'_readme': 'Registro de sistemas x versao 5017. origin: Original5017|Posterior|Custom|Verify|Base. decision: keep|remove|review (edite a mao). Classificacao inicial baseada na linha do tempo do jogo, nao verificada num cliente 5017.',
       'features': FEATURES}
json.dump(doc, open(OUT, 'w', encoding='utf-8'), ensure_ascii=False, indent=2)
print('ok', len(FEATURES), 'features ->', OUT)
