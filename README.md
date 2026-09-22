# Soccer Star — arquitetura multiplayer servidor-autoritativo

Implementação de referência em C# sem dependência de engine. O núcleo usa `System.Numerics.Vector3`; Unity/Godot/Unreal/engine própria entram apenas por adapters que implementam as interfaces em `Client/Interfaces.cs` e `Server/Interfaces.cs`.

## Locais de integração

- `src/Shared`: tipos, mensagens de rede e limites compartilhados.
- `src/Server`: roteamento de mensagens, rate limiting, anti-cheat, validação de chute, matchmaking e estado de partida.
- `src/Client`: input, mira assistida, Auto Play, linha visual e menu.

## Regras de segurança

1. Cliente nunca aplica física autoritativa, moedas, placar ou XP.
2. Todo pacote possui `Sequence` para bloquear replay/duplicação simples.
3. O servidor valida tipo/faixa, distância, direção, cooldown, rate-limit e permissões.
4. Configurações de assistência são normalizadas pelo servidor; o cliente não pode reduzir limites mínimos.
5. Matchmaking reserva moedas no servidor antes da criação da partida.
6. Destaque de moderação exige permissão no servidor e é enviado somente ao moderador.

## Adaptação por engine

Implemente:

- `IServerTransport`: WebSocket/UDP confiável/custom RPC.
- `IWorldPhysics`: física no processo autoritativo do servidor.
- `IPlayerRepository`: banco transacional/atômico.
- `IInputSource`: teclado/controle/touch.
- `IClientWorld`: leitura do snapshot interpolado do cliente.
- `ILineRenderer`: renderização local da linha de mira.
- `IOverlayUi`: menu flutuante da engine.

## Controles

- Shoot: PC `F`; Mobile botão Shoot.
- Pass: PC `R`; Mobile botão Pass.
- Tackle: PC `E`; Mobile botão Tackle.
- Spin: PC `Q`; Mobile botão Spin.
- Dash: PC `Shift`; Mobile botão Dash.

## Observação de produção

Para produção, use snapshots com interpolação/reconciliação, TLS/DTLS conforme transporte, autenticação de sessão, limite de tamanho de pacote antes da desserialização, banco com transações e idempotência por `matchId`/`reservationId`, logs estruturados e métricas. Nunca deixe o cliente enviar posição final, placar, saldo ou XP.

## Compilação com Termux e GitHub

Este repositório inclui `SoccerStar.Core.csproj` com alvo `net8.0`.

### Termux

```bash
pkg update
pkg install -y git dotnet-sdk-8.0

git clone URL_DO_SEU_REPOSITORIO.git
cd soccer-star
chmod +x scripts/build-termux.sh
./scripts/build-termux.sh
```

Ou diretamente:

```bash
dotnet restore SoccerStar.Core.csproj
dotnet build SoccerStar.Core.csproj -c Release
```

A DLL gerada fica em:

```text
bin/Release/net8.0/SoccerStar.Core.dll
```

### GitHub Actions

O workflow `.github/workflows/build.yml` executa restore e build a cada push ou pull request e publica o diretório compilado como artifact `soccer-star-core-net8`.

> Observação: esta compilação gera a biblioteca do núcleo multiplayer. Para gerar APK, executável de desktop ou cliente jogável ainda é necessário criar o adapter/projeto da engine escolhida (Unity, Godot, Unreal ou engine própria).
