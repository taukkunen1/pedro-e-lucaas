#!/usr/bin/env python3
"""Cobertura do registro: lista arquivos .cs do GameServer que nenhuma feature reivindica e conflitos (arquivo em 2+ features).
Uso: python3 tools/feature_scan.py [--report]   (--report imprime resumo por origem)"""
import json, os, re, sys, glob
ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'COServer')
reg = json.load(open(os.path.join(ROOT, 'Database5700', 'Features5017.json'), encoding='utf-8'))['features']
def rx(p):
    p = re.escape(p).replace(r'\*\*', '\x00').replace(r'\*', '[^/]*').replace('\x00', '.*')
    return re.compile('^' + p + '$')
comp = [(f, [rx(p) for p in f['paths']]) for f in reg]
files = [os.path.relpath(p, ROOT).replace('\\', '/') for p in glob.glob(os.path.join(ROOT, 'DFGameServer', '**', '*.cs'), recursive=True)]
files = [f for f in files if '/obj/' not in f and '/bin/' not in f]
unmapped, multi = [], []
count = {f['id']: 0 for f in reg}
for fl in files:
    hits = [f['id'] for f, rs in comp if any(r.match(fl) for r in rs)]
    if len(hits) > 1 and 'base.infra' in hits: hits.remove('base.infra')  # base.infra e' o fallback generico
    for h in hits: count[h] += 1
    if not hits: unmapped.append(fl)
    elif len(hits) > 1: multi.append((fl, hits))
print(f'{len(files)} arquivos | {len(files)-len(unmapped)} classificados | {len(unmapped)} sem feature | {len(multi)} em mais de uma')
for u in sorted(unmapped): print('  SEM FEATURE:', u)
for m, h in multi: print('  CONFLITO   :', m, h)
empty = [k for k, v in count.items() if v == 0]
if empty: print('Features sem nenhum arquivo:', empty)
if '--report' in sys.argv:
    by = {}
    for f in reg: by.setdefault(f['origin'], []).append(f'{f["id"]}({count[f["id"]]})')
    for o, l in by.items(): print(f'\n{o}:', ', '.join(l))
