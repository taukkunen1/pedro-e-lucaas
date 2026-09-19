# TrinityConquer - servidor Placebo

Servidor Conquer Online 5695 usado no projeto Placebo.

Este repositório é a base compartilhada para desenvolvimento do servidor. O objetivo é manter aqui todas as alterações feitas no GameServer, AccountServer, API, Database5700, ferramentas e documentação para que a equipe consiga acompanhar sempre a versão atual.

## Estado atual do projeto

- MySQL foi removido do fluxo ativo.
- MongoDB 7 é usado para contas/autenticação.
- O GameServer continua usando arquivos em `COServer/Database5700`.
- API, AccountServer e GameServer compilam para `C:\Users\hecto\OneDrive\Desktop\Placebo`.
- Telemetria de economia está ativa para CPs, Bound CPs e Gold.
- Logs de servidor/pacotes/erros foram adicionados.
- Modos `DEV` e `PRODUCTION` foram adicionados.

## Como rodar localmente

1. Instale o .NET 10 SDK.
2. Instale Docker Desktop ou MongoDB 7.
3. Crie um arquivo `.env` local com:

```env
MONGO_USER=seu_usuario
MONGO_PASSWORD=sua_senha
```

4. Suba o MongoDB:

```powershell
docker compose up -d
```

5. Compile a solução:

```powershell
dotnet build .\COServer\TrinityConquerServer.sln
```

6. Inicie os serviços gerados na pasta Placebo:

```powershell
cd "C:\Users\hecto\OneDrive\Desktop\Placebo\API"
dotnet API.dll

cd "C:\Users\hecto\OneDrive\Desktop\Placebo\AccountServer"
dotnet AccountServer.dll

cd "C:\Users\hecto\OneDrive\Desktop\Placebo\GameServer"
dotnet GameServer.dll
```

Portas esperadas:

- API: `8080`
- AccountServer: `9958`
- GameServer: `5816`
- MongoDB: `27017`

## Modo DEV e producao

O modo pode ser controlado em `API/GameServerConfig.json`:

```json
"ServerMode": "DEV"
```

Valores:

- `DEV`: ativa TestServer e grava mais detalhes nos logs, incluindo bytes iniciais de pacotes.
- `PRODUCTION`: mantém logs mais enxutos.

Também é possível usar variável de ambiente:

```powershell
$env:TRINITY_SERVER_MODE = "DEV"
```

## Logs

Logs diários ficam em:

```text
COServer/Database5700/Logs/server-AAAA-MM-DD.ndjson
```

Categorias principais:

- `system`: inicialização e eventos gerais.
- `packet`: pacote desconhecido ou erro dentro de handler de pacote.
- `error`: exceções gerais do servidor.

Telemetria de economia:

```text
COServer/Database5700/Telemetry/economy/
```

## Colaboração pelo GitHub

Repositório:

```text
https://github.com/taukkunen1/pedro-e-lucaas
```

Para dar acesso ao Lucas:

1. Abra o repositório no GitHub.
2. Entre em `Settings`.
3. Clique em `Collaborators`.
4. Clique em `Invite collaborators`.
5. Procure pelo usuário do Lucas no GitHub ou email dele.
6. Envie o convite.
7. Lucas precisa aceitar o convite pelo GitHub/email.

Para Lucas baixar o projeto depois de aceitar:

```powershell
git clone https://github.com/taukkunen1/pedro-e-lucaas.git
cd pedro-e-lucaas
```

Para receber atualizações:

```powershell
git pull
```

Para enviar alterações:

```powershell
git add .
git commit -m "Descricao da mudanca"
git push
```

## Cuidados

- Não subir `.env`.
- Não subir `mongo_data`.
- Não subir logs e telemetria gerados em runtime.
- Manter o repositório privado se houver configs sensíveis ou trabalho interno.
- Antes de enviar mudanças grandes, compilar a solução.

---

# TrinityConquer original

Conquer Online Server. This source use Stream for send/receive the packets of conquer client. Version 5695.

## Resources
Client Download Link: https://mega.nz/file/gZxnlR7I#9h7NFJ4EU5W7F_fsdGv7-NVpRIBYeGzeUtJ-7maecvY [Patch 1012]

Last patch: 1015

## Patches
Can get last patches from COServer\ConquerSite\wwwroot\patches (Contain fixes for some things in client)

## Alternative V5165
- Now have a alternative version of same source but adapted to V5165 (This version is only compatible with Windows). Download from here: https://github.com/darkfoxdeveloper/TrinityConquer5165 (Need request the access, Its Free!)

## Details about client
- In this custom client the real mounttype.dat is found in zftqat\zftqat.dat (only need encrypt/decrypt that for adding a mounttype)
- Is custom client with FPs Unlocked
- Compatible with 5695 Anticheat (Can buy this from me, the cost is 120€ at this time, for more details contact with me)

## Repository access buyers
If you have purchased the source from official distributors can access to the repository and have all the future updates for free

## Easy configuration
1 - Install MongoDB 7 (enable authentication and create a user, or run `docker compose up -d` with a `.env` containing MONGO_USER and MONGO_PASSWORD). MySQL is no longer used. Passwords are stored as PBKDF2 hashes.

2 - Compile the project (No run yet)

3 - Start the API once: it asks for the MongoDB host/port/database/user, creates the indexes and registers the server. Existing MySQL accounts can be imported with `python tools/migrate_mysql_to_mongo.py` (dry-run by default, `--apply` to write).

4 - BadMsg.txt now lives inside the Database folder (COServer\Database5700) - nothing to copy. Builds always go to the Placebo folder (see Directory.Build.props).

5 - Extract Database.rar to your C:\ Disk

6 - Run the project with multiple run config (Select Run for AccountServer, API and GameServer)

7 - Now the api console need your atention for config db and other things

8 - Source now is OK but need change IP from Hook of Loader if you need connect to other ip (no 127.0.0.1)

9 - Check if have the correct ServerName in AccountServerConfig.json (This need be the same of other configs file)

10- Change the ip with the project ConquerHook, open solution 'OpenSourceLoader5693\Hook\ConquerHook.sln' and change the IP searching in the project by 'char szIPAddress'

11 - After build the 'ConquerHook' need copy the 'ConquerHook.dll' to your client root

12 - All ready now can use the source and connect to this with your custom client and hook

## Considerations
- If you create account manually, set some IP for work and EntityUID >= 1000000

## TODO
- <s>Improve performance in Monster drop</s>
- <s>Monk skills (including auras)</s>
- <s>Auto maintenance after 24h of use or configurable auto restart</s>
- <s>Fix Top Pirate in trinity official client</s>
- <s>FrozenSky Not Working? (Reported and need check if have any issue and fix it)</s>
- <s>Bulletin System</s>
- <s>Change GameServer for use mysql database instead of files</s>
- <s>Squama System (Trap with ID 18 dynamic and rewards for it)</s>
- <s>Fix for Issue reported "Disproportionate Damage Scaling with Minor BP Differences in PVP"</s>
- <s>Tournaments Update (Fix some bugs)</s>
- Statue System (Not working good if you use Statue Guild, maybe is a client issue)

## Requeriments
- MongoDB 7
- .NET 10 SDK

## Enabling Slot Machines (Default not added in NPCs)
- Add to Npcs.txt of database, the next lines:

```
9826,60,19786,1036,232,233
9827,60,19796,1036,232,244
9828,60,19806,1036,242,233
9817,60,19776,1036,204,217
```

## About estructure
`ConquerSite: Have the website code.`

`DFAccountServer: The AccountServer Project.`

`DFGameServer: The GameServer Project.`

`DFCore: The Core Project. Contains common code for the other projects.`

`DFAPI: The API Project. This project is very important, manage all data using EntityFrameworkCore and have all configuration in JSON files.`

**For now DFAPI only manage data for the AccountServer because GameServer use files for saving/reading data for the server.**
