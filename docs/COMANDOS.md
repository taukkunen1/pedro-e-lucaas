# Comandos de chat do GameServer

Gerado a partir de `COServer/DFGameServer/Game/MsgServer/MsgMessage.cs` (método `ChatCommands`). O cliente 5695 não tem comandos próprios: tudo que começa com `@` (ou `!` para PM) é tratado pelo servidor.

## Quem pode usar

- **Jogador comum**: bloco de comandos de jogador (prefixo `@`).
- **VIP 6**: mais quatro comandos (prefixo `@`). Com o `economy.vip` removido (patch Era 1), ninguém tem VIP 6.
- **PM** (`ProjectManager`): todos os comandos da seção PM, com `@` ou `!`.
- **GM sem PM**: nenhum comando. A conta GM pula o bloco de jogador e é recusada no bloco de PM.

Como ler a tabela: `<argN:tipo>` é a posição da palavra no comando (`@item 480339 12 7` → arg1=480339, arg2=12, arg3=7). "texto" costuma ser nome de jogador; em `@give`, `@trace`, `@bring`, `@info`, `@kick` e similares, `me` significa você mesmo. Comandos que leem mais argumentos do que você passou caem no `catch` e respondem "Sorry cannot access to this command or not exists.".

A coluna "Linha" aponta para o `case` no arquivo, para conferir detalhes.

## Jogador comum (13)

| Comando | Argumentos | Observações | Linha |
|---|---|---|---|
| `@autohunt` / `@ah` | `<arg1:texto>` | mensagem: "Usage: @autohunt [settings|start|stop]" | 327 |
| `@leave` | | teleporta para mapa 1002 (428,380) | 339 |
| `@stuck` | | só funciona dentro da cadeia (mapa 6000): reposiciona em (30,74) | 345 |
| `@allieschat` | | mensagem: "Allies Chat mode: {client.Player.SendAllies}" | 351 |
| `@resetscores` / `@dc` | |  | 358 |
| `@visualeffects` | | mensagem: "Visual effects status: " | 361 |
| `@clearinventory` / `@agi` | `<arg1:número>` |  | 374 |
| `@str` | `<arg1:número>` |  | 396 |
| `@vit` | `<arg1:número>` |  | 418 |
| `@spi` | `<arg1:número>` |  | 440 |

## VIP 6 (4)

| Comando | Argumentos | Observações | Linha |
|---|---|---|---|
| `@spook` | | teleporta para mapa 2090 (32,30); mensagem: "You can`t use it in " | 468 |
| `@snow` | | teleporta para mapa 2054 (407,425); mensagem: "You can`t use it in " | 491 |
| `@vipinfo` | | mensagem: "Your VIP " | 515 |
| `@summonguild` | | mensagem: "You can`t use it in " | 537 |

## PM (205)

Comandos repetidos (`level`, `money`, `cps`, `item`, `spell`, `donationpoints`, `clearinventory`...) aparecem mais de uma vez no `switch`; o C# executa o primeiro `case` com aquele nome, e alguns destes estão aninhados como subcomandos de `@give`.

| Comando | Argumentos | Observações | Linha |
|---|---|---|---|
| `@autohunt` / `@ah` | `<arg1:texto>` | mensagem: "Usage: @autohunt [settings|start|stop]" | 605 |
| `@spawnmob` | `<arg1:texto>` | subcomandos: snowbanshee, teratodragon, thrillingspook, darkmoondemon, corndevil, darkspearman, mummyskeleton, ganoderma | 618 |
| `@resetscores` / `@counter` | |  | 650 |
| `@break` / `@steedu` | `<arg1:número> <arg2:número> <arg3:número> <arg4:número>` |  | 660 |
| `@addguiitem` | `<arg1:número> <arg2:número>` |  | 669 |
| `@addgui` | `<arg1:número>` |  | 679 |
| `@editnpc` / `@ali` | `<arg1:número>` |  | 704 |
| `@bc` / `@invisible` | |  | 726 |
| `@visible` / `@battlepoints` | `<arg1:número>` |  | 736 |
| `@hit` | `<arg1:número>` |  | 741 |
| `@reward` | `<arg1:número> <arg2:número> <arg3:número> <arg4:número>` |  | 748 |
| `@data` | `<arg1:número>` |  | 763 |
| `@data1` | `<arg1:número>` |  | 781 |
| `@clearspells` / `@incquest` | `<arg1:número> <arg2:número> <arg3:número> <arg4:número>` |  | 809 |
| `@accquest` | `<arg1:número>` |  | 818 |
| `@remquest` | `<arg1:número>` |  | 824 |
| `@finishquest` | `<arg1:número>` |  | 830 |
| `@rr` / `@cards` | `<arg1:número>` |  | 840 |
| `@trans` | `<arg1:número>` |  | 865 |
| `@pick` / `@ag` | |  | 890 |
| `@addnpc` | `<arg1:número> <arg2:número> <arg3:número> <arg4:número> <arg5:número>` |  | 899 |
| `@itemstack` | `<arg1:número> <arg2:número> <arg3:número>` |  | 911 |
| `@itemeffect` | `<arg1:número>` |  | 920 |
| `@dura` | `<arg1:número> <arg2:número>` |  | 935 |
| `@testct` | `<arg1:número>` |  | 951 |
| `@realbp` | | mensagem: "You real BatterPower is = " | 969 |
| `@bp` | | mensagem: "You BatterPower is = " | 974 |
| `@inventoryinfo` / `@newsuper` | |  | 989 |
| `@vitalitypoints` / `@removebuggedgarments` | |  | 1013 |
| `@removeitemandcompensate` | `<arg1:número> <arg2:número> <arg3:texto>` | mensagem: "[Usage: removeitemandcompensate itemid playerCompensationCPs player(optional)]" | 1040 |
| `@superman` / `@resetstats` | |  | 1104 |
| `@addmem` | `<arg1:número>` |  | 1122 |
| `@resetnobility` / `@give` | `<arg1:texto> <arg2:texto> <arg3:número> <arg4:número> <arg5:número> <arg6:número> <arg7:número> <arg8:número> <arg9:número>` | subcomandos: givenobil, donationpoints, spell, level, money, cps, reborns, item | 1144 |
| `@unbanstr` | `<arg1:texto>` |  | 1309 |
| `@unbanuid` | `<arg1:número>` |  | 1314 |
| `@ban` | `<arg1:texto> <arg2:número> <arg3:texto>` | mensagem: "You Account was Banned by [PM]/[GM]." | 1319 |
| `@banip` | `<arg1:texto> <arg2:número>` | mensagem: "You Ip Address was Banned by [PM]/[GM]." | 1339 |
| `@kick` | `<arg1:texto>` |  | 1354 |
| `@rev` / `@revive` | |  | 1367 |
| `@estats` / `@online` | | mensagem: "Online Players : " | 1404 |
| `@addtitle` | `<arg1:número>` |  | 1411 |
| `@vip` | `<arg1:texto> <arg2:número>` |  | 1418 |
| `@vipnewplayer` | `<arg1:texto> <arg2:número>` |  | 1444 |
| `@info` | `<arg1:texto>` |  | 1471 |
| `@scroll` | `<arg1:texto>` | subcomandos: lv, tc, pc, ac, am, dc, bi, pka, ma, ja; teleporta para mapa 1354 (5,290) | 1498 |
| `@trace` | `<arg1:texto>` |  | 1515 |
| `@bring` | `<arg1:texto>` |  | 1528 |
| `@arenapoints` | `<arg1:número>` |  | 1547 |
| `@life` / `@donationpoints` | `<arg1:número>` |  | 1559 |
| `@onlinepoints` | `<arg1:número>` |  | 1569 |
| `@staticrole` / `@facke` | `<arg1:número>` |  | 1583 |
| `@onlineminutes` | `<arg1:número>` |  | 1636 |
| `@addsouls` | `<arg1:número>` |  | 1641 |
| `@addrefinary` | `<arg1:número>` |  | 1662 |
| `@statue` / `@sofoke` | `<arg1:número>` |  | 1689 |
| `@haire` | `<arg1:número>` |  | 1696 |
| `@mapstat` | `<arg1:número>` |  | 1705 |
| `@pkp` | `<arg1:número>` |  | 1714 |
| `@ctf` / `@couplespk` | |  | 1738 |
| `@epk` / `@sktp` | |  | 1750 |
| `@tp` / `@startelitegw` | |  | 1760 |
| `@finishelitegw` / `@ctfon` | |  | 1772 |
| `@searchguard` | | mensagem: "Location Spawn --> " | 1777 |
| `@gui` | `<arg1:número>` |  | 1790 |
| `@sound` / `@sound2` | |  | 1805 |
| `@attackspell` | `<arg1:número> <arg2:número> <arg3:número>` |  | 1815 |
| `@attacknormal` | `<arg1:número>` |  | 1866 |
| `@ef` | `<arg1:número>` |  | 1894 |
| `@teleback` / `@map` | | mensagem: "MapID = " | 1904 |
| `@studypoints` | `<arg1:número>` |  | 1909 |
| `@expball` | `<arg1:número>` |  | 1915 |
| `@string_effect` | `<arg1:texto>` |  | 1920 |
| `@string_effect3` | `<arg1:número> <arg2:número> <arg3:número>` |  | 1929 |
| `@string_effect2` | `<arg1:número> <arg2:número> <arg3:texto>` |  | 1946 |
| `@gh` / `@xp` | |  | 2023 |
| `@addflag` | `<arg1:número>` |  | 2028 |
| `@remflag` | `<arg1:número>` |  | 2033 |
| `@level` | `<arg1:número>` |  | 2038 |
| `@incexp` | `<arg1:número>` |  | 2052 |
| `@money` | `<arg1:número>` |  | 2065 |
| `@do` | `<arg1:número>` |  | 2079 |
| `@soulp` | `<arg1:número>` |  | 2087 |
| `@refp` | `<arg1:número>` |  | 2105 |
| `@lotteryreset` / `@sendtick` | |  | 2137 |
| `@aura` / `@crit` | |  | 2153 |
| `@testmoob` | `<arg1:número> <arg2:número> <arg3:número> <arg4:número>` |  | 2163 |
| `@santaitems` / `@bot` | `<arg1:número>` |  | 2184 |
| `@abot` | `<arg1:número>` |  | 2203 |
| `@citywar` / `@bomba` | |  | 2229 |
| `@dragonwar` / `@new3` | |  | 2256 |
| `@new4` / `@new5` | |  | 2286 |
| `@new2` / `@testspawnmob` | `<arg1:número> <arg2:número>` |  | 2315 |
| `@topnobility` / `@frozensky` | |  | 2347 |
| `@frozenskyoff` / `@treasure` | |  | 2359 |
| `@lastman` / `@trinitypoints` | `<arg1:número>` |  | 2378 |
| `@teraton` / `@ann` | |  | 2403 |
| `@cps` | `<arg1:número>` |  | 2416 |
| `@a7a` / `@sofokedatos` | | mensagem: "Cps hunted since restart : " | 2432 |
| `@boundcps` | `<arg1:número>` |  | 2442 |
| `@presentflag` / `@remspell` | `<arg1:número>` |  | 2458 |
| `@spell` | `<arg1:número> <arg2:número> <arg3:número>` |  | 2470 |
| `@prof` | `<arg1:número> <arg2:número> <arg3:número>` |  | 2497 |
| `@clear` / `@clearinventory` | |  | 2525 |
| `@hhhj` / `@dancastle` | | teleporta para mapa 1601 (200,200) | 2536 |
| `@tele` | `<arg1:número> <arg2:número> <arg3:número> <arg4:número>` | mensagem: "You can`t go there, its nova`s office." | 2542 |
| `@effectfloor` | `<arg1:número>` |  | 2590 |
| `@tele2` | `<arg1:número> <arg2:número> <arg3:número>` |  | 2609 |
| `@itemid` | `<arg1:número> <arg2:número>` |  | 2651 |
| `@randomp1` / `@randomp2` | |  | 2695 |
| `@randomp3` / `@randomp4` | |  | 2717 |
| `@randomp5` / `@itemminute` | `<arg1:número> <arg2:número> <arg3:número>` |  | 2739 |
| `@item` | `<arg1:número> <arg2:número> <arg3:número> <arg4:número> <arg5:número> <arg6:número> <arg7:número> <arg8:número>` |  | 2759 |
| `@bcps` | `<arg1:número>` |  | 2821 |
| `@pkwar` / `@poledominationac` | |  | 2831 |
| `@poledominationbi` / `@poledominationpc` | |  | 2845 |
| `@poledominationdc` / `@additemstack` | `<arg1:número> <arg2:número>` |  | 2859 |
| `@remitemstack` | `<arg1:número> <arg2:número>` |  | 2868 |
| `@fftest` | `<arg1:número> <arg2:número> <arg3:número> <arg4:número>` |  | 2877 |
| `@atest` / `@ftest` | |  | 2977 |
| `@floor` | `<arg1:número>` |  | 3089 |
| `@eat` | `<arg1:número>` |  | 3108 |
| `@activetrap` | `<arg1:número> <arg2:número>` |  | 3127 |
| `@bodynpcs` | `<arg1:número>` |  | 3152 |
| `@addd` / `@aelite` | `<arg1:número>` |  | 3168 |
| `@teelite` | `<arg1:número> <arg2:texto>` |  | 3179 |
| `@transfer` | `<arg1:número>` |  | 3189 |
| `@inter` / `@hp` | `<arg1:número>` |  | 3200 |
| `@championpoints` | `<arg1:número>` |  | 3206 |
| `@tgui` | `<arg1:número>` |  | 3211 |
| `@hair` | `<arg1:número>` |  | 3236 |
| `@interip` | `<arg1:texto> <arg2:número>` |  | 3247 |
| `@dcinter` / `@floor2` | `<arg1:número> <arg2:número>` |  | 3262 |
| `@wea` | `<arg1:número> <arg2:número> <arg3:número> <arg4:número> <arg5:número>` |  | 3285 |
| `@activefairi` | `<arg1:número>` |  | 3295 |
| `@gift` / `@quiz` | `<arg1:número>` |  | 3335 |
| `@learnspells` / `@testupd` | |  | 3369 |
| `@asd` / `@exp` | `<arg1:número>` |  | 3403 |
| `@testsinglepacket` | `<arg1:número> <arg2:número>` |  | 3414 |
| `@testpacket` / `@test` | |  | 3482 |
| `@ada` | `<arg1:número> <arg2:número>` |  | 3509 |
| `@tttt` / `@reborn` | `<arg1:número>` |  | 3540 |
| `@class` | `<arg1:número>` |  | 3550 |
| `@agi` | `<arg1:número>` |  | 3555 |
| `@str` | `<arg1:número>` |  | 3577 |
| `@vit` | `<arg1:número>` |  | 3599 |
| `@testflag` | `<arg1:número>` |  | 3621 |
| `@testeffect` | `<arg1:texto>` |  | 3630 |
| `@reallot` / `@spi` | `<arg1:número>` |  | 3699 |
| `@reload` | `<arg1:texto>` | mensagem: "Sorry cannot access to this command or not exists." | 3721 |

## Principais comandos de PM, com exemplo

| Uso | Exemplo |
|---|---|
| Dar item: id, plus, bless, enchant, gema 1, gema 2, [quantidade], [efeito] | `@item 480339 12 7 255 13 13` ou `@item 1088000 0 0 0 0 0 5` (5 unidades) |
| Dar algo a um jogador | `@give me level 130`, `@give Fulano cps 1000`, `@give Fulano item 1088000 0 0 0 0 0` |
| Teleportar | `@tele 1002 428 378` |
| Ir até / trazer jogador | `@trace Fulano`, `@bring Fulano` |
| Cidades | `@tc`, `@pc`, `@ac`, `@am`, `@dc`, `@bi`, `@ma`, `@ja` (cadeia) |
| Definir valores exatos | `@level 120`, `@money 1000000`, `@cps 500`, `@pkp 0` |
| Skills | `@spell 1045 4`, `@prof 410 12`, `@learnspells` |
| Moderação | `@kick Fulano`, `@ban Fulano ...`, `@unbanstr Fulano` |
| Anúncio | `@bc mensagem` |
| Recarregar dados | `@reload`, `@reload --items` |
| Auto Hunt | `@ah`, `@ah start`, `@ah stop` |

Observação: os comandos de evento (`@citywar`, `@ctf`, `@epk`, `@poledomination*`, `@frozensky`, `@lastman`, `@treasure`, `@dragonwar` etc.) iniciam o evento à mão mesmo quando ele está como `remove` no `Features5017.json`.
