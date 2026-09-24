# Backup automatico

## Instalar

```bash
sudo mkdir -p /opt/trinityconquer /var/backups/trinityconquer
sudo cp -r . /opt/trinityconquer   # ou clone o repo direto em /opt/trinityconquer
sudo cp deploy/systemd/trinity-backup.service deploy/systemd/trinity-backup.timer /etc/systemd/system/
sudo systemctl daemon-reload
sudo systemctl enable --now trinity-backup.timer
```

## Conferir

```bash
systemctl list-timers trinity-backup.timer
sudo systemctl start trinity-backup.service   # roda uma vez, na hora, para testar
ls -lh /var/backups/trinityconquer/daily
```

## O que entra no backup

Todo `COServer/Database5700` exceto `Logs/` e `Telemetry/` (recriados pelo proprio servidor).
Isso inclui personagens, itens, guilds, contas (AuthJson), configuracoes e o
`Features5017.json`.

## Retencao

- 14 backups diarios (`daily/`)
- 8 backups semanais, feitos aos domingos (`weekly/`)

Ajuste `DAILY_KEEP` e `WEEKLY_KEEP` no script conforme o espaco em disco disponivel.

## Restaurar

```bash
sudo systemctl stop trinity-gameserver trinity-accountserver trinity-api   # pare os servicos primeiro
tar -xzf /var/backups/trinityconquer/daily/Database5700_<data>.tar.gz -C /opt/trinityconquer/COServer
```

## Fora do servidor

Este script guarda os backups no disco local, o que nao protege contra a perda
do servidor inteiro. Depois de escolher o provedor Linux, acrescente uma copia
externa (rclone para um object storage, ou rsync para outra maquina) rodando
logo apos este script.
