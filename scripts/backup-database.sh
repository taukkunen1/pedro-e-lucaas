#!/usr/bin/env bash
# Backup diario do Database5700 (personagens, itens, guilds, contas JSON, economia).
# Uso: backup-database.sh [dir_base_do_repo] [dir_de_destino]
set -euo pipefail

REPO_DIR="${1:-/opt/trinityconquer}"
BACKUP_DIR="${2:-/var/backups/trinityconquer}"
DB_DIR="$REPO_DIR/COServer/Database5700"
DAILY_KEEP=14      # dias de backup diario a manter
WEEKLY_KEEP=8       # semanas de backup semanal a manter (domingos)

if [ ! -d "$DB_DIR" ]; then
    echo "Database5700 nao encontrado em $DB_DIR" >&2
    exit 1
fi

mkdir -p "$BACKUP_DIR/daily" "$BACKUP_DIR/weekly"

TS="$(date +%F_%H%M%S)"
DOW="$(date +%u)"   # 1=segunda .. 7=domingo
FILE="Database5700_$TS.tar.gz"

# Exclui logs e telemetria de runtime (grandes e recriaveis) do backup diario;
# personagens, itens, guilds, contas e config sao o que realmente nao pode se perder.
tar -czf "$BACKUP_DIR/daily/$FILE" \
    -C "$REPO_DIR/COServer" \
    --exclude='Database5700/Logs' \
    --exclude='Database5700/Telemetry' \
    Database5700

echo "Backup criado: $BACKUP_DIR/daily/$FILE ($(du -h "$BACKUP_DIR/daily/$FILE" | cut -f1))"

# Aos domingos, copia tambem para a pasta semanal (retencao mais longa).
if [ "$DOW" = "7" ]; then
    cp "$BACKUP_DIR/daily/$FILE" "$BACKUP_DIR/weekly/$FILE"
    echo "Copia semanal criada: $BACKUP_DIR/weekly/$FILE"
fi

# Rotaciona: mantem so os N mais recentes de cada pasta.
find "$BACKUP_DIR/daily" -name 'Database5700_*.tar.gz' -printf '%T@ %p\n' \
    | sort -rn | tail -n +$((DAILY_KEEP + 1)) | cut -d' ' -f2- | xargs -r rm -f

find "$BACKUP_DIR/weekly" -name 'Database5700_*.tar.gz' -printf '%T@ %p\n' \
    | sort -rn | tail -n +$((WEEKLY_KEEP + 1)) | cut -d' ' -f2- | xargs -r rm -f
