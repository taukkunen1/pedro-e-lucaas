#!/usr/bin/env python3
"""Catalogo completo do TrinityConquer: itens, NPCs, quests, monstros, magias, mapas, lojas + auditoria de nomes.
Fontes: Database5700 (servidor), codigo do GameServer e (opcional) o cliente Placebo (ini/npc.ini, ini/Questinfo.ini).
Uso: python3 tools/build_catalog.py [--client <pasta do Placebo>]
Saida: docs/Catalogo_TrinityConquer.xlsx  +  docs/catalogo/*.csv
Nao altera nenhum arquivo do jogo."""
import os, re, sys, csv, glob, collections, struct
from openpyxl import Workbook
from openpyxl.styles import Font, PatternFill
from openpyxl.utils import get_column_letter

HERE = os.path.dirname(os.path.abspath(__file__))
REPO = os.path.normpath(os.path.join(HERE, '..'))
CO = os.path.join(REPO, 'COServer'); DB = os.path.join(CO, 'Database5700'); GS = os.path.join(CO, 'DFGameServer')
CLIENT = None
if '--client' in sys.argv: CLIENT = sys.argv[sys.argv.index('--client') + 1]
else:
    for c in (os.path.join(REPO, '..', '..', 'Placebo'), os.path.expanduser('~/mnt/Placebo')):
        if os.path.isdir(os.path.join(c, 'ini')): CLIENT = c; break
OUTD = os.path.join(REPO, 'docs'); os.makedirs(os.path.join(OUTD, 'catalogo'), exist_ok=True)

def rd(p, client=False):
    b = open(p, 'rb').read()
    for e in (('utf-8-sig', 'gbk') if client else ('utf-8-sig', 'cp1252')):
        try: return b.decode(e)
        except UnicodeDecodeError: pass
    return b.decode('latin-1')

def pretty(n):
    """DanceofCherry -> Dance of Cherry ; Doctor`sJacket -> Doctor's Jacket ; Delight~of~Speed -> Delight of Speed"""
    n = n.replace('~', ' ').replace('`', "'").replace('_', ' ')
    n = re.sub(r'(?<=[a-z0-9])(?=[A-Z])', ' ', n)
    n = re.sub(r'(?<=[A-Z])(?=[A-Z][a-z])', ' ', n)
    n = re.sub(r'(?<=[a-z])(of|the|and|for|in|to)(?=[A-Z])', r' \1 ', n)
    n = re.sub(r"'s(?=[A-Z])", "'s ", n)
    return re.sub(r'\s+', ' ', n).strip()

QUALITY = {0: 'Fixed', 1: 'Fixed', 2: 'Fixed', 3: 'Normal', 4: 'Normal', 5: 'Normal', 6: 'Refined', 7: 'Unique', 8: 'Elite', 9: 'Super'}

def item_slot(i):
    t = i // 1000
    if t in (622, 624, 626, 620, 617, 614, 615, 616) or (410 <= t <= 490) or (500 <= t <= 580) or (601 <= t <= 613): return 'Weapon (right)'
    if t == 619 or t == 1050 or t == 900: return 'Shield/Arrow (left)'
    if t == 148 or 111 <= t <= 118 or t in (123, 141, 142, 143, 144, 145, 170): return 'Headgear'
    if 120 <= t <= 121: return 'Necklace'
    if 130 <= t <= 139 or t == 101: return 'Armor'
    if 150 <= t <= 152: return 'Ring'
    if t == 160: return 'Boots'
    if 181 <= t <= 194: return 'Garment'
    if t == 201: return 'Fan'
    if t == 202: return 'Tower'
    if t == 203: return 'Riding crop'
    if t == 200: return 'Steed mount'
    if t == 300: return 'Steed'
    if t == 2100: return 'Bottle'
    if 350 <= t <= 370: return 'Accessory (right)'
    if t == 380: return 'Accessory (left)'
    return 'Misc / consumable'

# ---------------- ITEMS ----------------
items = []
for ln in rd(os.path.join(DB, 'itemtype.txt')).splitlines():
    if len(ln.strip()) <= 11 or ln.startswith('//'): continue
    d = [x for x in ln.split('@@') if x != '']
    bad = len(d) <= 52
    if bad and not (d and d[0].isdigit()): continue
    try:
        iid = int(d[0])
        items.append(dict(id=iid, raw=d[1].strip(), name=pretty(d[1].strip()), quality=QUALITY.get(iid % 10, '?'), q=iid % 10,
                          slot=item_slot(iid), kind=(d[53] if len(d) > 53 else ''), level=int(d[4]), gold=int(d[12]),
                          cps=int(d[37]) if len(d) > 37 else 0, bad=bad, desc=(d[54].replace('~', ' ') if len(d) > 54 else '')))
    except Exception: pass
item_ids = {i['id'] for i in items}

# ---------------- MAPS ----------------
maps = {}
gm = os.path.join(DB, 'ini', 'GameMap.txt')
for ln in rd(gm).splitlines():
    p = ln.split()
    if len(p) >= 2 and p[0].isdigit():
        f = p[1].replace('\\', '/'); base = os.path.splitext(os.path.basename(f))[0]
        maps[int(p[0])] = dict(id=int(p[0]), file=f, name=pretty(base), exists=os.path.exists(os.path.join(DB, f)))
for m in maps.values():  # nomes canonicos vindos de outras fontes
    pass

# ---------------- NPC SOURCES ----------------
enum = {}
en = rd(os.path.join(GS, 'Game', 'MsgNpc', 'NpcID.cs'))
for m in re.finditer(r'^\s*([A-Za-z_][\w]*)\s*=\s*(\d+)\s*,', en, re.M): enum[int(m.group(2))] = m.group(1)
attr = {}
for f in glob.glob(os.path.join(GS, 'Game', 'MsgNpc', '**', '*.cs'), recursive=True):
    for m in re.finditer(r'\[NpcAttribute\(NpcID\.(\w+)([^\]]*)\)\]', rd(f)):
        attr[m.group(1)] = m.group(2)
name2uid = {v: k for k, v in enum.items()}
dyn = []  # NPCs declarados no codigo
for nm, args in attr.items():
    uid = name2uid.get(nm)
    a = [x.strip() for x in args.strip(', ').split(',')] if args.strip(', ') else []
    nmatch = re.search(r'name:\s*"([^"]*)"', args)
    dyn.append(dict(uid=uid, ident=nm, args=args.strip(), cname=(nmatch.group(1) if nmatch else '')))

client_npc = {}; client_npcx = {}; client_q = {}
if CLIENT:
    cur = None
    for l in rd(os.path.join(CLIENT, 'ini', 'npc.ini'), True).splitlines():
        m = re.match(r'\[NpcType(\d+)\]', l)
        if m: cur = int(m.group(1)); client_npc[cur] = ''; continue
        if cur is not None and l.startswith('Name='): client_npc[cur] = l[5:].strip(); cur = None
    for m in re.finditer(r'\[(\d+)\]\s*\r?\nName=([^\r\n]*)', rd(os.path.join(CLIENT, 'ini', 'NpcX.ini'), True)): client_npcx[int(m.group(1))] = m.group(2).strip()
    q = rd(os.path.join(CLIENT, 'ini', 'Questinfo.ini'), True)
    for m in re.finditer(r'^\[(\d+)\]\s*\n(.*?)(?=^\[|\Z)', q, re.M | re.S):
        body = m.group(2)
        g = lambda k: (re.search(r'^' + k + r'=(.*)$', body, re.M) or [None, ''])[1].strip()
        mid = g('MissionId')
        client_q[int(m.group(1))] = dict(section=int(m.group(1)), mission=int(mid) if mid.isdigit() else int(m.group(1)), name=g('Name'), typeid=g('TypeId'),
                                          lvmin=g('Lv_min'), lvmax=g('Lv_max'), begin=g('BeginNpcId'), finish=g('FinishNpcId'), prize=g('Prize'), desc=g('IntentionDesp'), content=g('Content'))

# nomes de NPC vindos do Questinfo (server + client): uid,map,x,y,name,mapname
quest_npc = {}
def scan_quest_npcs(text):
    for m in re.finditer(r'^(?:BeginNpcId|FinishNpcId)=(\d+),(\d+),\d+,\d+,([^,\r\n]+),([^\r\n]*)', text, re.M):
        quest_npc.setdefault(int(m.group(1)), m.group(3).strip())
        mp = m.group(4).strip()
        if mp and int(m.group(2)) in maps and maps[int(m.group(2))]['name'].lower() == pretty(os.path.splitext(os.path.basename(maps[int(m.group(2))]['file']))[0]).lower(): maps[int(m.group(2))]['alt'] = mp
scan_quest_npcs(rd(os.path.join(DB, 'Questinfo.ini')))
if CLIENT: scan_quest_npcs(rd(os.path.join(CLIENT, 'ini', 'Questinfo.ini'), True))

sob = {}
for ln in rd(os.path.join(DB, 'SobNpcs.txt')).splitlines():
    p = ln.strip().split(',')
    if len(p) >= 7 and p[0].isdigit(): sob[int(p[0])] = dict(name=p[1], type=p[2], mesh=p[3], map=p[4], x=p[5], y=p[6])

NPCTYPE = {0: 'Stun', 1: 'Shop', 2: 'Talker', 3: 'Fixed', 5: 'Beautician', 6: 'Upgrader', 7: 'Socketer', 8: 'Warehouse?', 10: 'Pole', 14: 'Booth', 16: 'Sob(structure)',
           19: 'Gambling', 21: 'Stake', 22: 'Scarecrow', 25: 'Furniture', 26: 'Gate', 29: 'Type29', 30: 'Type30', 31: 'ClanInfo', 32: 'DialogAndGui', 33: 'Type33', 36: 'Type36', 37: 'Type37', 38: 'Type38'}

def resolve_npc(uid):
    if uid in quest_npc: return pretty(quest_npc[uid]), 'Questinfo.ini (NPC citado na quest)', 'high'
    if uid in client_npcx: return pretty(client_npcx[uid]), 'client NpcX.ini', 'high'
    for d in dyn:
        if d['uid'] == uid and d['cname']: return pretty(d['cname']), 'NpcAttribute(name)', 'high'
    if uid in sob: return pretty(sob[uid]['name']), 'SobNpcs.txt', 'high'
    if uid in enum: return pretty(enum[uid]), 'NpcID enum (codigo)', 'medium'
    # npc.ini do cliente e' indexado por TIPO de NPC, nao por UID: so bate com o nome real em ~6% dos casos testados -> baixa confianca
    if uid in client_npc and client_npc[uid]: return pretty(client_npc[uid]), 'client npc.ini (tipo = UID; pode diferir do nome real)', 'low'
    return '', '', ''

CJK = re.compile(r'[\u3400-\u9fff]')
_resolve = resolve_npc
def resolve_npc(uid):
    nm, src, conf = _resolve(uid)
    if nm and CJK.search(nm): return '(CN) ' + nm, src + ' [nome em chines]', 'low'
    return nm, src, conf
npcs = []
seen_uid = set()
for ln in rd(os.path.join(DB, 'Npcs.txt')).splitlines():
    p = ln.strip().split(',')
    if len(p) < 6: continue
    uid, typ, mesh, mp, x, y = map(int, p[:6])
    nm, src, conf = resolve_npc(uid)
    mesh_hint = ''  # palpite por mesh removido: 0 de 94 acertos na validacao contra nomes de quests
    tname = NPCTYPE.get(typ, str(typ)); mname = maps.get(mp, {}).get('name', str(mp))
    if not nm:
        if mesh_hint: nm, src, conf = mesh_hint, 'client npc.ini (pelo mesh %d)' % mesh, 'low'
        else: nm, src, conf = f'{tname} {uid} - {mname}', 'sugerido (tipo+id+mapa)', 'placeholder'
    npcs.append(dict(uid=uid, name=nm, source=src, conf=conf, type=tname, mesh=mesh, map=mp, mapname=mname, x=x, y=y, origin='Npcs.txt'))
    seen_uid.add(uid)
for uid, s in sob.items():
    npcs.append(dict(uid=uid, name=pretty(s['name']), source='SobNpcs.txt', conf='high', type=NPCTYPE.get(int(s['type']), s['type']), mesh=s['mesh'], map=s['map'],
                     mapname=maps.get(int(s['map']), {}).get('name', s['map']), x=s['x'], y=s['y'], origin='SobNpcs.txt'))
    seen_uid.add(uid)
for d in dyn:  # declarados no codigo, fora dos arquivos
    if d['uid'] is not None and d['uid'] in seen_uid: continue
    nm, src, conf = resolve_npc(d['uid']) if d['uid'] is not None else ('', '', '')
    if not nm: nm, src, conf = pretty(d['ident']), 'identificador no codigo', 'medium'
    npcs.append(dict(uid=d['uid'] if d['uid'] is not None else '', name=nm, source=src, conf=conf, type='(dinamico)', mesh='', map='', mapname='', x='', y='', origin='codigo (NpcAttribute)'))
# enum-only
declared = {n['uid'] for n in npcs if n['uid'] != ''}
for uid, ident in enum.items():
    if uid in declared: continue
    nm, src, conf = resolve_npc(uid)
    npcs.append(dict(uid=uid, name=nm, source=src, conf=conf, type='(enum)', mesh='', map='', mapname='', x='', y='', origin='NpcID enum'))

# ---------------- QUESTS ----------------
code_q = collections.defaultdict(lambda: dict(refs=0, files=set()))
for f in glob.glob(os.path.join(GS, '**', '*.cs'), recursive=True):
    s = rd(f); rel = os.path.relpath(f, GS).replace('\\', '/')
    for m in re.finditer(r'QuestGUI\.(?:CheckQuest|FinishQuest|CheckObjectives|IncreaseQuestObjectives|SetQuestObjectives|RemoveQuest|IsActiveQuest)\(\s*(\d+)', s):
        c = code_q[int(m.group(1))]; c['refs'] += 1; c['files'].add(rel)
    for m in re.finditer(r'GetFinishQuest\(\s*[^,()]+,\s*[^,()]+,\s*(\d+)', s):
        c = code_q[int(m.group(1))]; c['refs'] += 1; c['files'].add(rel)
srv_q = {}
sq = rd(os.path.join(DB, 'Questinfo.ini'))
for m in re.finditer(r'^\[(\d+)\]\s*\n(.*?)(?=^\[|\Z)', sq, re.M | re.S):
    body = m.group(2)
    nm = (re.search(r'^Name=(.*)$', body, re.M) or [0, ''])[1].strip(); mid = (re.search(r'^MissionId=(\d+)', body, re.M) or [0, m.group(1)])[1]
    srv_q[int(mid)] = nm
bin_q = collections.Counter(); bins = sorted(glob.glob(os.path.join(DB, 'Quests', '*.bin')))
for b in bins:
    d = open(b, 'rb').read()
    try:
        n = struct.unpack_from('<i', d, 0)[0]; off = 4
        for _ in range(n):
            uid, st, tm, k = struct.unpack_from('<IIIi', d, off); off += 16 + 4 * k; bin_q[uid] += 1
    except Exception: pass
all_q = set(code_q) | set(srv_q) | set(bin_q)
by_mission = {v['mission']: v for v in client_q.values()}
quests = []
for qid in sorted(all_q):
    c = by_mission.get(qid) or client_q.get(qid)
    if qid in srv_q and srv_q[qid]: nm, src, conf = srv_q[qid], 'Questinfo.ini (servidor)', 'high'
    elif c and c['name']: nm, src, conf = c['name'], 'client Questinfo.ini', ('low' if CJK.search(c['name']) else 'high')
    else: nm, src, conf = f'Quest {qid}', 'sugerido (sem nome no cliente)', 'placeholder'
    quests.append(dict(id=qid, name=pretty(nm) if nm == nm.replace(' ', '') else nm, source=src, conf=conf, lvmin=(c or {}).get('lvmin', ''), lvmax=(c or {}).get('lvmax', ''),
                       begin=(c or {}).get('begin', ''), finish=(c or {}).get('finish', ''), prize=(c or {}).get('prize', ''),
                       code_refs=code_q[qid]['refs'] if qid in code_q else 0, in_code='sim' if qid in code_q else 'nao', in_saves=bin_q.get(qid, 0),
                       files='; '.join(sorted(code_q[qid]['files'])[:4]) if qid in code_q else ''))
client_only = [c for c in client_q.values() if c['mission'] not in all_q]

# ---------------- MONSTROS ----------------
mon = []
for f in sorted(glob.glob(os.path.join(DB, 'Monsters', '*.ini'))):
    t = rd(f); g = lambda k: (re.search(r'^' + k + r'\s*=\s*(.*)$', t, re.M) or [0, ''])[1].strip()
    if g('id'): mon.append(dict(id=int(g('id')), name=pretty(g('name')), raw=g('name'), level=int(g('level') or 0), life=int(g('life') or 0), lookface=g('lookface'), file=os.path.basename(f)))
mon_ids = {m['id'] for m in mon}
mtxt = rd(os.path.join(DB, 'monster.txt')); mon_txt = []
for m in re.finditer(r'^\[([^\]]+)\]\s*\n(.*?)(?=^\[|\Z)', mtxt, re.M | re.S):
    tid = (re.search(r'^TypeID=(\d+)', m.group(2), re.M) or [0, ''])[1]; lv = (re.search(r'^Level=(\d+)', m.group(2), re.M) or [0, ''])[1]
    mon_txt.append(dict(name=pretty(m.group(1)), raw=m.group(1), typeid=tid, level=lv))
gens = []
for f in sorted(glob.glob(os.path.join(DB, 'cq_generator', '*.ini'))):
    t = rd(f); g = lambda k: (re.search(r'^' + k + r'\s*=\s*(.*)$', t, re.M) or [0, ''])[1].strip()
    try: gens.append(dict(id=g('id'), map=int(g('mapid') or 0), npctype=int(g('npctype') or 0), maxnpc=g('maxnpc')))
    except Exception: pass
mon_by_id = {m['id']: m for m in mon}
gen_missing = collections.Counter(g['npctype'] for g in gens if g['npctype'] not in mon_ids)

# ---------------- MAGIAS ----------------
magic = collections.OrderedDict()
for ln in rd(os.path.join(DB, 'magictype.txt')).splitlines():
    d = ln.split('@@')
    if len(d) > 3 and d[0].isdigit():
        k = int(d[1]); e = magic.setdefault(k, dict(id=k, name=pretty(d[3]), raw=d[3], levels=0)); e['levels'] += 1

# ---------------- LOJAS / AUDITORIA ----------------
shop_items = collections.defaultdict(set)
for f in glob.glob(os.path.join(DB, 'shops', '*.ini')):
    for ln in rd(f).splitlines():
        m = re.match(r'^\s*(\d{5,7})[\s,]', ln)
        if m: shop_items[os.path.basename(f)].add(int(m.group(1)))
shop_missing = {k: sorted(x for x in v if x not in item_ids) for k, v in shop_items.items()}
prize_missing = set()
for t in (sq, rd(os.path.join(CLIENT, 'ini', 'Questinfo.ini'), True) if CLIENT else ''):
    for m in re.finditer(r'\[item (\d+),', t):
        if int(m.group(1)) not in item_ids: prize_missing.add(int(m.group(1)))

for i in items:
    pass
name_count = collections.Counter(i['raw'] for i in items)
audit = []
def A(area, sev, item, detail): audit.append((area, sev, item, detail))
A('Itens', 'info', f'{len(items)} itens', f'todos com nome; {len(name_count)} nomes distintos (variantes de qualidade/nivel compartilham o nome). Qualidade = ultimo digito do ID.')
for i in items:
    if i['bad']: A('Itens', 'warn', f"{i['id']} {i['name']}", 'linha malformada (<= 52 campos): o servidor IGNORA este item ao carregar o itemtype.txt')
A('Itens', 'info', 'nomes com marcador', f"{sum(1 for i in items if '~' in i['raw'] or '`' in i['raw'])} nomes usam ~ ou ` (convertidos em espaco/apostrofo na coluna 'Nome')")
for area, lst in (('NPCs', npcs),):
    c = collections.Counter(n['conf'] for n in lst)
    A('NPCs', 'info', f'{len(lst)} NPCs no catalogo', ', '.join(f'{k}={v}' for k, v in c.items()) + ' (high=nome citado em quest/SobNpcs/NpcAttribute; medium=identificador do enum NpcID; low=npc.ini do cliente (tipo, nao garante nome); placeholder=sugerido tipo+id+mapa, revisar)')
A('NPCs', 'warn', 'Npcs.txt sem nomes', f"{sum(1 for n in npcs if n['origin']=='Npcs.txt')} linhas, nenhuma com a coluna de nome (7a); o servidor so usa nome se a coluna existir. O npc.ini do cliente e indexado por tipo de NPC e so coincide com o nome real em poucos casos, por isso essas sugestoes ficam com confianca baixa.")
cq = collections.Counter(q['conf'] for q in quests)
A('Quests', 'info', f'{len(quests)} quests distintas', ', '.join(f'{k}={v}' for k, v in cq.items()) + f" | no codigo: {sum(1 for q in quests if q['in_code']=='sim')} | so em saves: {sum(1 for q in quests if q['in_code']=='nao' and q['in_saves'])}")
A('Quests', 'warn', 'Quests/*.bin', f'{len(bins)} arquivos sao SAVES de jogadores (nome do arquivo = UID do jogador), nao definicoes; Users/ esta vazio, entao sao saves orfaos.')
A('Quests', 'info', 'Questinfo.ini do servidor', f'define so {len(srv_q)} quests; as demais sao logica em codigo (NpcHandler) e seus nomes vem do cliente.')
if CLIENT: A('Quests', 'info', 'quests so no cliente', f'{len(client_only)} quests do cliente (Questinfo.ini do Placebo) sem nenhuma referencia no servidor')
A('Monstros', 'info', f'{len(mon)} monstros (Monsters/*.ini) | {len(mon_txt)} em monster.txt | {len(gens)} geradores', '')
mt_ids = {int(x) for x in re.findall(r'^TypeID=(\d+)', mtxt, re.M)}
gen_really = {k: v for k, v in gen_missing.items() if k not in mt_ids}
if gen_really: A('Monstros', 'warn', f'{len(gen_really)} npctype de gerador sem monstro (nem em Monsters/ nem no monster.txt)', 'ex.: ' + ', '.join(f'{k}(x{v})' for k, v in list(gen_really.items())[:10]))
elif gen_missing: A('Monstros', 'info', f'{len(gen_missing)} npctype de gerador estao so no monster.txt (TypeID), nao em Monsters/', 'ok: o servidor os define em monster.txt')
A('Magias', 'info', f'{len(magic)} magias / {sum(m["levels"] for m in magic.values())} linhas de nivel', '')
A('Mapas', 'info', f'{len(maps)} mapas em GameMap.txt', f"{sum(1 for m in maps.values() if not m['exists'])} com arquivo ausente em Database5700")
for k, v in shop_missing.items():
    if v: A('Lojas', 'warn', k, f'{len(v)} item(ns) inexistente(s) no itemtype: ' + ', '.join(map(str, v[:12])))
if prize_missing: A('Quests', 'warn', 'premios sem item', f'{len(prize_missing)} IDs de item usados como premio nao existem no itemtype (ex.: ' + ', '.join(map(str, sorted(prize_missing)[:10])) + ')')

# ---------------- SAIDA ----------------
def write_csv(name, rows, cols):
    with open(os.path.join(OUTD, 'catalogo', name), 'w', newline='', encoding='utf-8-sig') as f:
        w = csv.writer(f); w.writerow([c[0] for c in cols])
        for r in rows: w.writerow([r.get(c[1], '') for c in cols])
sheets = [
 ('Itens', items, [('ID', 'id'), ('Nome', 'name'), ('Nome original', 'raw'), ('Qualidade', 'quality'), ('Slot', 'slot'), ('Tipo', 'kind'), ('Nivel', 'level'), ('Ouro', 'gold'), ('CPs', 'cps'), ('Descricao', 'desc')]),
 ('NPCs', npcs, [('UID', 'uid'), ('Nome', 'name'), ('Fonte do nome', 'source'), ('Confianca', 'conf'), ('Tipo', 'type'), ('Mesh', 'mesh'), ('Mapa ID', 'map'), ('Mapa', 'mapname'), ('X', 'x'), ('Y', 'y'), ('Origem', 'origin')]),
 ('Quests', quests, [('ID', 'id'), ('Nome', 'name'), ('Fonte do nome', 'source'), ('Confianca', 'conf'), ('Lv min', 'lvmin'), ('Lv max', 'lvmax'), ('NPC inicio', 'begin'), ('NPC fim', 'finish'), ('Premio', 'prize'), ('Refs no codigo', 'code_refs'), ('No codigo', 'in_code'), ('Em saves', 'in_saves'), ('Arquivos', 'files')]),
 ('Monstros', mon, [('ID', 'id'), ('Nome', 'name'), ('Nome original', 'raw'), ('Nivel', 'level'), ('Vida', 'life'), ('Lookface', 'lookface'), ('Arquivo', 'file')]),
 ('Magias', list(magic.values()), [('ID', 'id'), ('Nome', 'name'), ('Nome original', 'raw'), ('Niveis', 'levels')]),
 ('Mapas', list(maps.values()), [('ID', 'id'), ('Nome', 'name'), ('Arquivo', 'file'), ('Existe', 'exists')]),
]
wb = Workbook(); ws = wb.active; ws.title = 'Resumo'
ws.append(['Area', 'Severidade', 'Item', 'Detalhe'])
for r in audit: ws.append(list(r))
for name, rows, cols in sheets:
    s = wb.create_sheet(name); s.append([c[0] for c in cols])
    for r in rows: s.append([r.get(c[1], '') for c in cols])
    write_csv(name.lower() + '.csv', rows, cols)
    s.freeze_panes = 'A2'; s.auto_filter.ref = s.dimensions
for s in wb.worksheets:
    for c in s[1]: c.font = Font(bold=True, color='FFFFFF'); c.fill = PatternFill('solid', fgColor='305496')
    for i, col in enumerate(s.columns, 1):
        w = max((len(str(c.value)) for c in list(col)[:300] if c.value is not None), default=8)
        s.column_dimensions[get_column_letter(i)].width = min(max(w + 2, 8), 60)
out = os.path.join(OUTD, 'Catalogo_TrinityConquer.xlsx'); wb.save(out)
print('salvo', out)
for r in audit: print(f'[{r[1]}] {r[0]} - {r[2]}: {r[3]}')
