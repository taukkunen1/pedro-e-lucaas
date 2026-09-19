#!/usr/bin/env python3
"""Relatorio de economia (mint/burn) a partir da telemetria do servidor.
Uso: python3 tools/economy_report.py [--dir <pasta Telemetry/economy>] [--from YYYY-MM-DD] [--to YYYY-MM-DD] [--supply]
  --supply : soma Money/WHMoney/CPs de todos os personagens em Database5700/Users (oferta atual em circulacao)
Mint = criada | Burn = destruida | Transfer = entre jogadores/armazem (residuo != 0 => vazamento a investigar)."""
import os, sys, json, glob, collections, configparser
REPO = os.path.normpath(os.path.join(os.path.dirname(os.path.abspath(__file__)), '..'))
DB = os.path.join(REPO, 'COServer', 'Database5700')
arg = lambda k, d=None: sys.argv[sys.argv.index(k) + 1] if k in sys.argv else d
D = arg('--dir', os.path.join(DB, 'Telemetry', 'economy')); F, T = arg('--from', '0000'), arg('--to', '9999')
files = [f for f in sorted(glob.glob(os.path.join(D, '*.ndjson'))) if F <= os.path.basename(f)[:10] <= T]
if not files and '--supply' not in sys.argv: sys.exit(f'sem dados em {D}')
sysagg = collections.defaultdict(lambda: collections.defaultdict(lambda: [0, 0, 0, 0, 0]))  # cur -> system -> [mint, burn, tin, tout, n]
reasons = collections.defaultdict(lambda: collections.defaultdict(lambda: [0, 0, 0]))       # cur -> reason -> [in, out, n]
players = collections.defaultdict(lambda: collections.defaultdict(lambda: [0, 0, ''])) # cur -> uid -> [mint, burn, name]
alerts = []; n = 0; days = collections.Counter()
for f in files:
    for ln in open(f, encoding='utf-8'):
        try: e = json.loads(ln)
        except ValueError: continue
        n += 1; days[e['t'][:10]] += 1
        a = sysagg[e['cur']][e['system']]; d = e['delta']; r = reasons[e['cur']][e['reason']]
        a[4] += 1; r[2] += 1
        if e['flow'] == 'Transfer':
            if d > 0: a[2] += d; r[0] += d
            else: a[3] += -d; r[1] += -d
        else:
            p = players[e['cur']][e['uid']]; p[2] = e['name']
            if d > 0: a[0] += d; r[0] += d; p[0] += d
            else: a[1] += -d; r[1] += -d; p[1] += -d
        if e.get('alert'): alerts.append(e)
fmt = lambda x: f'{x:,}'.replace(',', '.')
print(f'{n} eventos em {len(files)} arquivo(s): ' + ', '.join(f'{k}={v}' for k, v in sorted(days.items())))
for cur in ('Gold', 'CP', 'BoundCP'):
    if cur not in sysagg: continue
    mint = sum(v[0] for v in sysagg[cur].values()); burn = sum(v[1] for v in sysagg[cur].values())
    print(f'\n=== {cur}: mint {fmt(mint)} | burn {fmt(burn)} | liquido {fmt(mint - burn)} | burn/mint {burn / mint:.1%}' if mint else f'\n=== {cur}: mint 0 | burn {fmt(burn)}')
    print(f'{"sistema":<18}{"mint":>16}{"burn":>16}{"transf.in":>14}{"transf.out":>14}{"residuo":>12}{"eventos":>9}')
    for s, v in sorted(sysagg[cur].items(), key=lambda kv: -(kv[1][0] + kv[1][1] + kv[1][2] + kv[1][3])):
        print(f'{s:<18}{fmt(v[0]):>16}{fmt(v[1]):>16}{fmt(v[2]):>14}{fmt(v[3]):>14}{fmt(v[2] - v[3]):>12}{fmt(v[4]):>9}')
    print('  maiores motivos:')
    for k, v in sorted(reasons[cur].items(), key=lambda kv: -(kv[1][0] + kv[1][1]))[:12]: print(f'    {k:<46} +{fmt(v[0]):>14} -{fmt(v[1]):>14} ({fmt(v[2])}x)')
    top = sorted(players[cur].items(), key=lambda kv: -kv[1][0])[:5]
    print('  maiores minters: ' + ', '.join(f'{p[2]}({u}) {fmt(p[0])}' for u, p in top))
    other = sysagg[cur].get('Other')
    if other and (other[0] + other[1]) > 0.05 * (mint + burn): print('  ! "Other" passa de 5%: refine as regras em Database5700/EconomyTelemetry.json (veja "maiores motivos")')
if alerts:
    print(f'\n{len(alerts)} ALERTAS:')
    for e in alerts[:15]: print(f'  {e["t"]} {e["name"]}({e["uid"]}) {e["cur"]} {e["delta"]:+,} {e["system"]} <- {e["reason"]}')
if '--supply' in sys.argv:
    tot = collections.Counter(); cnt = 0
    for f in glob.glob(os.path.join(DB, 'Users', '*.ini')):
        cp = configparser.ConfigParser(strict=False, interpolation=None)
        try: cp.read(f, encoding='utf-8-sig')
        except Exception: continue
        if not cp.has_section('Character'): continue
        c = cp['Character']; cnt += 1
        for k in ('Money', 'WHMoney', 'ConquerPoints', 'BoundConquerPoints'):
            try: tot[k] += int(c.get(k, '0'))
            except ValueError: pass
    print(f'\nOferta em circulacao ({cnt} personagens): ' + ', '.join(f'{k}={fmt(v)}' for k, v in tot.items()))
