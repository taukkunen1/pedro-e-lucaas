#!/usr/bin/env python3
"""Inventario estatico de TODO ponto do GameServer que ganha (+=), gasta (-=) ou atribui (=) Gold / CPs / BoundCPs.
Saida: docs/catalogo/economia_pontos.csv  + resumo no console.
Complementa a telemetria em runtime (Telemetry/Economy.cs): aqui e' o mapa do codigo; la e' o que realmente aconteceu."""
import os, re, csv, glob, collections
REPO = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..'))
GS = os.path.join(REPO, 'COServer', 'DFGameServer')
OUT = os.path.join(REPO, 'docs', 'catalogo', 'economia_pontos.csv')
CUR = {'Money': 'Gold', 'ConquerPoints': 'CP', 'BoundConquerPoints': 'BoundCP'}
QUAL = re.compile(r'([A-Za-z_][\w\.\[\]\(\)]*)\.(Money|ConquerPoints|BoundConquerPoints)\s*(\+=|-=|=(?!=))\s*([^;]+);')
BARE = re.compile(r'(?<![\w\.])(Money|ConquerPoints|BoundConquerPoints)\s*(\+=|-=)\s*([^;]+);')
NPCATTR = re.compile(r'\[NpcAttribute\(NpcID\.(\w+)')
METHOD = re.compile(r'^\s*(?:(?:public|private|internal|protected|static|unsafe|async|override|virtual)\s+)+[\w<>\[\],\.\?]+\s+(\w+)\s*\(')
CASE = re.compile(r'^\s*case\s+([\w\.\(\)]+)\s*:')
STR = re.compile(r'"([^"]{6,90})"')
KW = {'if', 'for', 'while', 'switch', 'foreach', 'using', 'lock', 'catch', 'return', 'new'}

def rd(p):
    b = open(p, 'rb').read()
    for e in ('utf-8-sig', 'cp1252'):
        try: return b.decode(e)
        except UnicodeDecodeError: pass
    return b.decode('latin-1')

rows = []
for f in sorted(glob.glob(os.path.join(GS, '**', '*.cs'), recursive=True)):
    rel = os.path.relpath(f, GS).replace('\\', '/')
    lines = rd(f).splitlines()
    npc = method = case = ''
    for i, ln in enumerate(lines):
        m = NPCATTR.search(ln)
        if m: npc = m.group(1); method = ''; case = ''
        mm = METHOD.match(ln)
        if mm and mm.group(1) not in KW and not ln.strip().endswith(';'): method = mm.group(1)
        cm = CASE.match(ln)
        if cm: case = cm.group(1)
        for rx, qual in ((QUAL, True), (BARE, False)):
            for m in rx.finditer(ln):
                if qual: who, cur, op, expr = m.group(1), m.group(2), m.group(3), m.group(4)
                else: who, cur, op, expr = 'this', m.group(1), m.group(2), m.group(3)
                if cur == 'Money' and who.split('.')[-1] in ('WHMoney',): continue
                hint = ''
                for j in range(max(0, i - 6), min(len(lines), i + 7)):
                    s = STR.search(lines[j])
                    if s and 'Money' not in lines[j][:0]: hint = s.group(1); break
                rows.append(dict(currency=CUR[cur], op={'+=': 'gain', '-=': 'spend', '=': 'set'}[op], file=rel, line=i + 1, npc=npc if rel.endswith('NpcHandler.cs') else '',
                                 method=method, case=case, target=who, amount=expr.strip()[:80], hint=hint))
os.makedirs(os.path.dirname(OUT), exist_ok=True)
cols = ['currency', 'op', 'file', 'line', 'npc', 'method', 'case', 'target', 'amount', 'hint']
with open(OUT, 'w', newline='', encoding='utf-8-sig') as fh:
    w = csv.DictWriter(fh, fieldnames=cols); w.writeheader(); w.writerows(rows)
print(len(rows), 'pontos ->', OUT)
c = collections.Counter((r['currency'], r['op']) for r in rows)
for k, v in sorted(c.items()): print(' ', k, v)
byfile = collections.Counter(r['file'] for r in rows)
print('top arquivos:', byfile.most_common(8))
print('NPCs distintos que mexem em moeda:', len({r['npc'] for r in rows if r['npc']}))
