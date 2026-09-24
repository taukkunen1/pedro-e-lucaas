#!/usr/bin/env python3
r"""
Gera o manifest.json do update do cliente, a partir de uma pasta com os arquivos
que o jogador deve ter (client 5695 + hook + resources).

Uso:
    python3 build_client_manifest.py <pasta_do_client> <versao> <base_url> [saida.json]

Exemplo:
    python3 build_client_manifest.py C:\Placebo\ClientPackage 2026.09.24.1 \
        https://trinityconquer.eu/client-updates/2026.09.24.1 \
        COServer/DFAPI/wwwroot/client-updates/manifest.json

O launcher usa "baseUrl" + "/" + o "path" de cada arquivo para baixar o que mudou.
Publique os arquivos reais em wwwroot/client-updates/<versao>/ (mesma estrutura de
pastas do client) e rode este script apontando para essa mesma pasta.

Arquivos ignorados de proposito, porque sao locais/gerados/config do jogador:
Conquer.exe.log, *.log, *.tmp, ConquerHook.dll (o launcher decide o hook, nao o update).
"""
import hashlib
import json
import os
import sys

IGNORE_NAMES = {"Conquer.exe.log", "ConquerHook.dll", "version.txt"}
IGNORE_EXT = {".log", ".tmp", ".bak"}


def sha256_of(path):
    h = hashlib.sha256()
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(1 << 20), b""):
            h.update(chunk)
    return h.hexdigest()


def main():
    if len(sys.argv) < 4:
        print(__doc__)
        sys.exit(1)

    client_dir = sys.argv[1]
    version = sys.argv[2]
    base_url = sys.argv[3].rstrip("/")
    out_path = sys.argv[4] if len(sys.argv) > 4 else "manifest.json"

    files = []
    for root, _dirs, names in os.walk(client_dir):
        for name in names:
            if name in IGNORE_NAMES or os.path.splitext(name)[1].lower() in IGNORE_EXT:
                continue
            full = os.path.join(root, name)
            rel = os.path.relpath(full, client_dir).replace(os.sep, "/")
            files.append({
                "path": rel,
                "sha256": sha256_of(full),
                "size": os.path.getsize(full),
            })

    files.sort(key=lambda f: f["path"])
    manifest = {"version": version, "baseUrl": base_url, "files": files}

    os.makedirs(os.path.dirname(out_path) or ".", exist_ok=True)
    with open(out_path, "w", encoding="utf-8") as f:
        json.dump(manifest, f, indent=2, ensure_ascii=False)

    total = sum(f["size"] for f in files)
    print(f"Manifest gerado: {out_path}")
    print(f"Versao: {version}  |  Arquivos: {len(files)}  |  Tamanho total: {total / 1024 / 1024:.1f} MB")
    print(f"Publique os arquivos em: {base_url}  (mesma estrutura de {client_dir})")


if __name__ == "__main__":
    main()
