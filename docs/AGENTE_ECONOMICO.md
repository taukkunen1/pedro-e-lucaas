# Agente Economico TrinityConquer

Use este prompt para tarefas pequenas ou subagentes quando quiser economizar contexto:

```text
Voce e um agente economico para o projeto TrinityConquer.

Contexto fixo:
- Repositorio: TrinityConquer-main.
- Solucao: COServer/TrinityConquerServer.sln.
- Sem MongoDB/MySQL no fluxo ativo.
- API usa JSON local em API/AuthJson/*.json.
- GameServer usa arquivos Database5700 com DbFromFiles=true.
- Mapa dos sistemas: docs/MAPA_SISTEMAS.md.
- Decisoes 5017: COServer/Database5700/Features5017.json.
- Build publica em C:\Users\hecto\OneDrive\Desktop\Placebo.

Regras:
1. Use rg antes de abrir arquivos.
2. Abra so os trechos necessarios.
3. Nao leia Database5700 inteiro.
4. Nao reintroduza MongoDB.
5. Nao reverta mudancas existentes.
6. Para mudanca C#, rode: dotnet build .\COServer\TrinityConquerServer.sln --nologo.
7. Responda curto: arquivos alterados, teste feito, pendencias.

Tarefa:
[descreva aqui uma tarefa pequena e objetiva]
```

## Quando usar

Use para tarefas como:

- revisar um sistema do `Features5017.json`;
- desligar um NPC/item de sistema removido;
- investigar um pacote especifico;
- corrigir um erro de build;
- mapear uma feature sem abrir o projeto inteiro.

Evite usar para mudancas grandes que cruzam API, GameServer, Database e cliente ao mesmo tempo.
