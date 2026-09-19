# DEV, PRODUCAO e colaboracao

## Modos do servidor

O GameServer agora entende dois modos:

- `DEV`: logs mais completos para diagnosticar pacotes, sistemas e erros.
- `PRODUCTION`: logs mais enxutos, sem dump hexadecimal de pacote.

O modo pode ser definido em `API/GameServerConfig.json`:

```json
"ServerMode": "DEV"
```

Ou pela variavel de ambiente:

```powershell
$env:TRINITY_SERVER_MODE = "DEV"
```

Se nada for definido, o servidor usa `PRODUCTION`.

## Logs

Os logs diarios ficam em:

```text
Database5700/Logs/server-AAAA-MM-DD.ndjson
```

Cada linha e um evento em JSON. Os eventos principais sao:

- `system`: inicializacao e eventos gerais.
- `packet`: pacote desconhecido ou erro dentro de handler de pacote.
- `error`: excecoes gerais do servidor.

Em `DEV`, o evento de pacote inclui `packetHex` com os primeiros bytes do pacote.

## Colaboracao com amigo

Para duas pessoas acompanharem sempre a mesma versao, use um repositorio Git privado.

Fluxo recomendado:

1. Criar um repositorio privado no GitHub.
2. Adicionar seu amigo como colaborador.
3. Subir esta pasta do servidor para o repositorio.
4. Toda mudanca feita aqui vira commit e push.
5. Seu amigo usa pull para receber a atualizacao.

Comandos base, depois que o repositorio privado existir:

```powershell
git init
git add .
git commit -m "Base TrinityConquer sem MySQL e com logs"
git branch -M main
git remote add origin URL_DO_REPOSITORIO_PRIVADO
git push -u origin main
```

Quando fizermos novas mudancas:

```powershell
git add .
git commit -m "Descricao curta da mudanca"
git push
```

Do lado dele:

```powershell
git clone URL_DO_REPOSITORIO_PRIVADO
git pull
```
