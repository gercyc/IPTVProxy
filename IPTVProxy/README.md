# IPTV Proxy - Xtream Codes API Simulator

Um proxy IPTV que simula a API Xtream Codes a partir de arquivos M3U/M3U8, implementado com **Controllers** tradicionais do ASP.NET Core.

## 📋 Descrição

Este projeto oferece uma abordagem completa e robusta para converter playlists M3U padrão em uma API compatível com Xtream Codes. Utiliza o padrão de Controllers do ASP.NET Core para melhor estrutura e organização da lógica de negócio.

## 🏗️ Arquitetura

```
IPTVProxy/
├── Controllers/
│   └── XtreamApiController.cs         # Controller da API Xtream com Minimal APIs mapping
├── Extensions/
│   └── SwaggerConfiguration.cs        # Configuração do Swagger/ReDoc
├── Program.cs                         # Entry point da aplicação
├── appsettings.json                   # Configurações
├── appsettings.Development.json       # Configurações de desenvolvimento
└── IPTVProxy.csproj                   # Arquivo de projeto
```

### Dependências Externas

Este projeto referencia o projeto **IPTVProxy.Common** que contém:
- **Modelos**: Definições de dados (M3U, Xtream Codes)
- **Serviços**: Lógica de negócio (parser M3U, simulador Xtream)
- **Extensões**: Helpers de configuração
- **Endpoints**: Endpoints Minimal APIs

```
IPTVProxy → IPTVProxy.Common
```

## 🚀 Como usar

### 1. Configuração

Edite o arquivo `appsettings.json`:

```json
{
  "M3uFilePath": "us-grc.m3u",           // Caminho para o arquivo M3U
  "ServerUrl": "http://localhost:5000"    // URL do servidor
}
```

### 2. Executar

```bash
cd C:\source\Test-Projects\IPTVProxy\IPTVProxy
dotnet run
```

### 3. Acessar

O servidor inicia em `http://localhost:5000`

**Documentação da API:**
- **Swagger UI**: `http://localhost:5000/swagger` - Interface interativa para testar os endpoints
- **ReDoc**: `http://localhost:5000/docs` - Documentação completa e elegante
- **OpenAPI JSON**: `http://localhost:5000/openapi/v1.json` - Especificação OpenAPI

**Credenciais padrão:**
- Username: `demo`
- Password: `demo123`

## 📡 Endpoints da API

### Estrutura

Os endpoints estão implementados no `XtreamApiController` com injeção de dependência. O `XtreamSimulator` é registrado no container de DI do ASP.NET Core, permitindo melhor testabilidade e manutenibilidade.

**Rota base**: `/api/xtreamapi/`

### Player API
```
GET /player_api.php?username=demo&password=demo123
```

Retorna informações de login e servidor.

### Ações disponíveis

#### Canais ao vivo
- `get_live_categories` - Lista categorias de canais
- `get_live_streams` - Lista canais
- `get_live_streams?category_id={id}` - Canais por categoria

#### VOD (Filmes)
- `get_vod_categories` - Lista categorias de filmes
- `get_vod_streams` - Lista filmes
- `get_vod_streams?category_id={id}` - Filmes por categoria
- `get_vod_info?vod_id={id}` - Informações detalhadas do filme

#### Séries
- `get_series_categories` - Lista categorias de séries
- `get_series` - Lista séries
- `get_series?category_id={id}` - Séries por categoria
- `get_series_info?series_id={id}` - Informações detalhadas da série

#### EPG
- `get_short_epg?stream_id={id}&limit={n}` - EPG curto
- `get_simple_data_table?stream_id={id}` - EPG completo

### Playlist M3U
```
GET /get.php?username=demo&password=demo123&type=m3u_plus&output=ts
```

### EPG XML
```
GET /xmltv.php?username=demo&password=demo123
```

### Streaming

#### Canais ao vivo
```
GET /{username}/{password}/{stream_id}
GET /{username}/{password}/{stream_id}.ts
```

#### Filmes
```
GET /movie/{username}/{password}/{vod_id}.mp4
```

#### Episódios de séries
```
GET /series/{username}/{password}/{episode_id}.mp4
```

## 🛠️ Tecnologias

- **.NET 10.0** - Framework
- **ASP.NET Core** - Web API
- **Controllers (MVC)** - Endpoints estruturados com injeção de dependência
- **Serilog** - Logging estruturado
- **Swagger/OpenAPI** - Documentação interativa
- **IPTVProxy.Common** - Biblioteca compartilhada

## 📦 Dependências

- **Microsoft.AspNetCore.OpenApi** (10.0.2) - OpenAPI/Swagger
- **Swashbuckle.AspNetCore.ReDoc** (10.1.0) - ReDoc para documentação elegante
- **Swashbuckle.AspNetCore.SwaggerUI** (10.1.0) - Interface Swagger
- **Serilog.AspNetCore** (10.0.0) - Logging integrado
- **Serilog.Enrichers.*** - Enriquecedores de contexto (ClientInfo, Environment, Process, Thread)
- **Serilog.Sinks.*** - Destinos de log (Console, File, Async, PeriodicBatching)
- **IPTVProxy.Common** - Referência de projeto local

## 🎯 Funcionalidades

- ✅ Parsing completo de arquivos M3U/M3U8 (via IPTVProxy.Common)
- ✅ Suporte a canais ao vivo, VOD e séries
- ✅ Geração de EPG simulado
- ✅ Proxy de streaming com suporte a Range requests
- ✅ API compatível com Xtream Codes
- ✅ Geração de playlist M3U e EPG XML
- ✅ Modo mock para testes sem arquivo M3U
- ✅ Documentação OpenAPI/Swagger completa
- ✅ Logging estruturado com Serilog (Console + Arquivo JSON)
- ✅ Health checks
- ✅ **Controllers (MVC)** - Arquitetura tradicional e robusta
- ✅ **Dependency Injection** - Melhor testabilidade e manutenibilidade

## 📊 Logs

Os logs são gravados em:
- **Console**: Logs coloridos em tempo real
- **Arquivo**: `Logs/logs.json` (rotação diária, mantém 7 dias)

Formato JSON estruturado com:
- Timestamp
- Level (Information, Warning, Error, etc.)
- Machine name, Process ID, Thread ID
- Client IP e User Agent (quando disponível)
- Mensagem e exceções

## 📝 Exemplo de uso com player

### IPTV Smarters
1. Adicionar novo provedor
2. Tipo: Xtream Codes API
3. URL: `http://localhost:5000`
4. Username: `demo`
5. Password: `demo123`

### Perfect Player
1. Configurações → Geral → Listas de reprodução
2. Adicionar lista → Xtream Codes
3. Nome: IPTV Proxy
4. URL: `http://localhost:5000`
5. Login: `demo`
6. Senha: `demo123`

## 🔧 Desenvolvimento

### Build
```bash
dotnet build
```

### Run
```bash
dotnet run
```

### Estrutura de dados mock

Caso não haja arquivo M3U, o sistema gera dados mock com:
- 24 canais ao vivo em 6 categorias
- 24 filmes em 6 categorias
- 20 séries em 5 categorias
- EPG simulado para todos os canais

## 📄 Licença

Este projeto é fornecido como está, sem garantias.

## 🤝 Contribuições

Contribuições são bem-vindas! Sinta-se à vontade para abrir issues e pull requests.

## 🎨 Comparação entre IPTVProxy e IPTVProxyMinimal

| Aspecto | IPTVProxy | IPTVProxyMinimal |
|---------|-----------|------------------|
| Abordagem | Controllers (MVC) | Minimal APIs |
| Complexidade | Completa com Controllers | Minimalista e simples |
| Estrutura | Padrão MVC tradicional | Endpoints declarativos |
| Performance | Otimizada | Otimizada |
| Uso ideal | Arquitetura robusta, grandes projetos | Microserviços, APIs leves |
| Linha de código | Mais extensa | Mais compacta |
| Curva de aprendizado | Padrão MVC familiar | Abordagem funcional |

---

Desenvolvido com ❤️ para a comunidade IPTV
