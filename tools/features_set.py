#!/usr/bin/env python3
"""Muda a decisao de uma feature no registro.  Uso: python3 tools/features_set.py <id> keep|remove|review [<id> ...]
                                             python3 tools/features_set.py --list [origin]
Efeito: com 'remove' os handlers (pacotes/NPCs) da feature deixam de ser registrados e os toggles/tournaments correspondentes ficam inativos no proximo start do GameServer."""
import json, os, sys
P = os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'COServer', 'Database5700', 'Features5017.json')
doc = json.load(open(P, encoding='utf-8')); feats = {f['id']: f for f in doc['features']}
a = sys.argv[1:]
if not a or a[0] == '--list':
    for f in doc['features']:
        if len(a) > 1 and f['origin'].lower() != a[1].lower(): continue
        print(f"{f['id']:<28}{f['origin']:<13}{f['confidence']:<8}{f['decision']:<8}{f['name']}")
    sys.exit()
i = 0
while i + 1 < len(a):
    fid, dec = a[i], a[i + 1]
    if fid not in feats or dec not in ('keep', 'remove', 'review'): sys.exit(f'invalido: {fid} {dec}')
    feats[fid]['decision'] = dec; print(fid, '->', dec); i += 2
json.dump(doc, open(P, 'w', encoding='utf-8'), ensure_ascii=False, indent=2)
