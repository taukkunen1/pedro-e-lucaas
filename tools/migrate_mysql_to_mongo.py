#!/usr/bin/env python3
"""Migra o banco de autenticacao do MySQL (cq.auth) para o MongoDB.
Requer: pip install pymysql pymongo
Uso:   python tools/migrate_mysql_to_mongo.py --mysql-host localhost --mysql-user root --mysql-pass X --mysql-db cq.auth \
                                               --mongo-uri mongodb://localhost:27017 --mongo-db cq_auth [--apply]
Sem --apply so mostra o que faria (dry-run). Senhas em texto puro sao convertidas para hash PBKDF2-SHA256
(mesmo formato do Core.Security.PasswordHasher); senhas que ja estao em hash sao copiadas como estao.
"""
import argparse, base64, hashlib, os, sys
import pymysql
from pymongo import MongoClient, ReplaceOne

ITER = 210_000
def hash_pw(pw: str) -> str:
    salt = os.urandom(16)
    h = hashlib.pbkdf2_hmac('sha256', pw.encode('utf-8'), salt, ITER, 32)
    return f"pbkdf2-sha256${ITER}${base64.b64encode(salt).decode()}${base64.b64encode(h).decode()}"

ap = argparse.ArgumentParser()
ap.add_argument('--mysql-host', default='localhost'); ap.add_argument('--mysql-port', type=int, default=3306)
ap.add_argument('--mysql-user', default='root'); ap.add_argument('--mysql-pass', default='')
ap.add_argument('--mysql-db', default='cq.auth')
ap.add_argument('--mongo-uri', default='mongodb://localhost:27017'); ap.add_argument('--mongo-db', default='cq_auth')
ap.add_argument('--apply', action='store_true')
a = ap.parse_args()

my = pymysql.connect(host=a.mysql_host, port=a.mysql_port, user=a.mysql_user, password=a.mysql_pass,
                     database=a.mysql_db, cursorclass=pymysql.cursors.DictCursor)
mg = MongoClient(a.mongo_uri)[a.mongo_db]

def rows(table):
    with my.cursor() as c:
        c.execute(f"SELECT * FROM `{table}`")
        return c.fetchall()

plan = {}
acc = []
for r in rows('accounts'):
    pw = r.get('Password') or ''
    if not pw.startswith('pbkdf2-sha256$'): pw = hash_pw(pw)
    acc.append({'_id': int(r['EntityID']), 'Username': r['Username'], 'Password': pw, 'Email': r.get('Email') or '',
                'IP': r.get('IP') or '', 'State': int(r.get('State') or 0)})
plan['accounts'] = acc
plan['servers'] = [{'_id': int(r['Id']), 'Name': r['Name'], 'IP': r['IP'], 'Port': int(r['Port']),
                    'TransferKey': r['TransferKey'], 'TransferSalt': r['TransferSalt']} for r in rows('servers')]
plan['votes'] = [{'_id': int(r['ID']), 'EntityID': int(r['EntityID']), 'Votes': int(r['Votes']), 'LastVoteDate': r['LastVoteDate']} for r in rows('votes')]
plan['online'] = [{'_id': int(r['Id']), 'Name': r['Name'], 'OnlineCount': int(r['OnlineCount'])} for r in rows('online')]
plan['configurations'] = [{'_id': int(r['Id']), 'Key': r['Key'], 'Value': r['Value']} for r in rows('configurations')]

for name, docs in plan.items():
    print(f"{name}: {len(docs)} registros")
if not a.apply:
    print("dry-run: nada gravado. Use --apply."); sys.exit(0)

for name, docs in plan.items():
    if docs:
        mg[name].bulk_write([ReplaceOne({'_id': d['_id']}, d, upsert=True) for d in docs])
# contadores (proximo id = maior id + 1); "base" e "seq" seguem o formato de AuthMongo.NextIdAsync
for name, docs in plan.items():
    ids = [d['_id'] for d in docs]
    if ids:
        first = 1000000 if name == 'accounts' else 1
        mg['counters'].replace_one({'_id': name}, {'_id': name, 'base': first, 'seq': max(ids) - first + 1}, upsert=True)
mg.accounts.create_index('Username', unique=True)
print("migracao concluida")
