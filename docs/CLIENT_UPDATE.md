# Atualização do cliente (launcher)

Como funciona: o launcher (`ConquerLoader`) confere a versão local contra
`GET /api/clientupdate/manifest`, baixa só os arquivos com hash diferente
(SHA-256), grava cada um primeiro como `.part` e só substitui o arquivo real
se o hash bater, e então abre o jogo com o hook. Se a API estiver fora do ar,
o launcher avisa e tenta abrir o jogo do mesmo jeito com o que já está
instalado — nunca impede o jogador de jogar por causa disso.

## Publicar uma nova versão

1. Monte a pasta com **todos** os arquivos que o jogador deve ter (client,
   resources, `ConquerHook.dll` — veja a exceção abaixo).
2. Gere o manifesto:

```bash
python3 tools/build_client_manifest.py \
    /caminho/para/ClientPackage \
    2026.09.24.1 \
    https://SEU_DOMINIO/client-updates/2026.09.24.1 \
    COServer/DFAPI/wwwroot/client-updates/manifest.json
```

3. Copie a pasta `ClientPackage` inteira para
   `COServer/DFAPI/wwwroot/client-updates/2026.09.24.1/` no servidor (mesma
   estrutura de pastas usada no passo 2 — o `baseUrl` do manifesto aponta
   para lá).
4. Reinicie ou apenas aguarde: a API serve `wwwroot` estaticamente, não
   precisa reiniciar para novos arquivos aparecerem, só para o
   `Program.cs` (se você mudar código da API).

O próximo launcher que abrir compara a versão, vê que mudou e baixa só o que
for diferente.

## O que NUNCA entra no manifesto

`build_client_manifest.py` já ignora por conta própria:

- `ConquerHook.dll` — o launcher decide o hook, não o update (evita baixar um
  binário que antivírus costuma sinalizar, e mantém o hook fora do canal de
  update público).
- `*.log`, `*.tmp`, `*.bak`, `version.txt` — arquivos locais/gerados.

## Testando localmente

Rode a API (`dotnet API.dll`, porta padrão 8080) com uma pasta
`wwwroot/client-updates/manifest.json` de teste (pode ser só 2-3 arquivos
pequenos) e aponte o `ManifestUrl` do `app.config` do launcher para
`http://localhost:8080/api/clientupdate/manifest`.

## Antes de distribuir para jogadores

- Trocar `ManifestUrl` no `app.config` do launcher para o domínio/IP
  definitivo (HTTPS, se tiver certificado).
- Servir os arquivos por HTTPS reduz o risco de alguém interceptar e trocar
  um arquivo no meio do caminho — o SHA-256 detecta corrupção acidental, mas
  não foi pensado como proteção contra um MITM ativo.
