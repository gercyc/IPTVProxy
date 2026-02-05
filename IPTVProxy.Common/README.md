# IPTVProxy.Common - Biblioteca Compartilhada

Uma biblioteca .NET 10 que fornece funcionalidades compartilhadas para proxys IPTV que simulam a API Xtream Codes a partir de arquivos M3U/M3U8.

## 📋 Descrição

Este projeto é uma biblioteca de classes que encapsula toda a lógica de negócio, modelos de dados e extensões necessárias para criar aplicações IPTV compatíveis com Xtream Codes. É referenciada pelos projetos **IPTVProxy** (Controllers) e **IPTVProxyMinimal** (Minimal APIs), promovendo reutilização de código e consistência entre as implementações.

## 🏗️ Arquitetura

```
IPTVProxy.Common/
├── Models/
│   ├── M3u/
│   │   └── M3uModels.cs                # Modelos para parsing M3U
│   │       ├── Playlist
│   │       ├── Entry
│   │       └── StreamType
│   └── Xtream/
│       ├── Authentication.cs           # Resposta de autenticação
│       ├── Category.cs                 # Modelo de categorias
│       ├── Channel.cs                  # Modelo de canais ao vivo
│       ├── ChannelResponse.cs          # Response wrapper para canais
│       ├── Vod.cs                      # Modelo de filmes (VOD)
│       ├── Series.cs                   # Modelo de séries
│       ├── Epg.cs                      # Modelo de EPG (Electronic Program Guide)
│       └── PlayerApiRequest.cs         # Modelo de requisição player
├── Services/
│   ├── M3uParser.cs                    # Parser de arquivos M3U/M3U8
│   ├── M3uPlaylistService.cs           # Gerenciamento de playlist
│   └── XtreamSimulator.cs              # Simulador da API Xtream Codes
├── Api/
│   ├── XtreamApiEndpoints.cs           # Endpoints Minimal APIs (mapeamento de rotas)
│   └── Endpoints/                      # Implementações específicas dos endpoints
│       ├── LiveChannelEndpoints.cs     # Endpoints de canais ao vivo
│       ├── VodEndpoints.cs             # Endpoints de filmes (VOD)
│       ├── SeriesEndpoints.cs          # Endpoints de séries
│       ├── EpgEndpoints.cs             # Endpoints de EPG
│       ├── PlaylistEndpoints.cs        # Endpoints de playlist M3U
│       └── StreamingEndpoints.cs       # Endpoints de streaming direto
├── Extensions/
│   ├── ServiceCollectionExtensions.cs  # Extensões para DI (AddIptvServices)
│   └── EndpointExtensions.cs           # Extensões para Minimal APIs
└── IPTVProxy.Common.csproj             # Arquivo de projeto
```

## 🎯 Componentes Principais

### 1. **Modelos de Dados (Models)**

#### M3U Models
- **Playlist**: Representa uma playlist M3U completa com metadados
- **Entry**: Representa uma entrada de stream individual
- **StreamType**: Enumeração para tipos de stream (Live, VOD, Series)

```csharp
public class Playlist
{
    public List<Entry> Entries { get; set; }
    public Dictionary<StreamType, List<Category>> Categories { get; set; }
}

public class Entry
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Logo { get; set; }
    public string Group { get; set; }
    public string Url { get; set; }
    public StreamType Type { get; set; }
}
```

#### Xtream Models
- **Authentication**: Resposta de autenticação com informações do servidor
- **Category**: Categorias de canais, filmes ou séries
- **Channel**: Canais ao vivo com metadados
- **ChannelResponse**: Wrapper com status para respostas de canais
- **Vod**: Filmes com informações detalhadas
- **Series**: Séries TV com metadados
- **Epg**: Dados de Electronic Program Guide (grade de programação)

### 2. **Serviços (Services)**

#### M3uParser
Responsável pelo parsing de arquivos M3U/M3U8:
- Leitura de arquivos
- Parsing de linhas #EXTINF
- Extração de metadados (nome, logo, grupo, etc.)
- Classificação por tipo de stream
- Geração de dados mock quando arquivo não existe

**Métodos principais:**
```csharp
public Playlist ParseM3uFile(string filePath);
public Playlist GenerateMockData();
```

#### M3uPlaylistService
Gerenciador da playlist em memória:
- Carregamento e armazenamento de playlist
- Métodos de acesso aos dados
- Filtros por categoria
- Busca por ID

**Métodos principais:**
```csharp
public void Load(string filePath);
public Playlist GetPlaylist();
public List<Category> GetLiveCategories();
public List<Channel> GetLiveChannels(int? categoryId = null);
public List<Category> GetVodCategories();
public List<Vod> GetVodStreams(int? categoryId = null);
// ... e mais
```

#### XtreamSimulator
Simulador da API Xtream Codes:
- Geração de respostas compatíveis com Xtream
- Simulação de EPG
- Dados de autenticação
- Mapeamento entre modelos internos e respostas Xtream

**Métodos principais:**
```csharp
public Authentication GetPlayerApi();
public List<Category> GetLiveCategories();
public ChannelResponse GetLiveChannels(int? categoryId = null);
public List<Epg> GetShortEpg(int streamId, int limit = 10);
// ... e mais
```

### 3. **API Endpoints (Api)**

#### XtreamApiEndpoints
Mapeador de endpoints Minimal APIs:
- Registra todos os endpoints da API Xtream
- Suporta autenticação básica
- Retorna respostas JSON compatíveis

**Extensão principal:**
```csharp
public static IEndpointRouteBuilder MapXtreamApi(
    this IEndpointRouteBuilder endpoints,
    string baseRoute = "/api/xtreamapi")
```

### 4. **Extensões (Extensions)**

#### ServiceCollectionExtensions
Facilita a configuração de serviços via Dependency Injection:

```csharp
public static IServiceCollection AddIptvServices(
    this IServiceCollection services,
    string m3uFilePath,
    string serverUrl)
```

Registra:
- M3uParser (Singleton)
- M3uPlaylistService (Singleton)
- XtreamSimulator (Transient)
- Logging estruturado (Serilog)
- OpenAPI/Swagger

#### EndpointExtensions
Extensões para mapear endpoints Minimal APIs facilmente:
```csharp
endpoints.MapXtreamApiEndpoints("/");  // Raiz
// ou
endpoints.MapXtreamApiEndpoints("/api/xtreamapi");  // Com base route customizada
```

## 📡 Endpoints da API Xtream Simulada

### Autenticação
```
GET /player_api.php?username=demo&password=demo123
```

### Canais ao Vivo (Live)
```
GET /get.php?action=get_live_categories
GET /get.php?action=get_live_streams&category_id={id}
GET /get.php?action=get_live_streams
```

### VOD (Filmes)
```
GET /get.php?action=get_vod_categories
GET /get.php?action=get_vod_streams&category_id={id}
GET /get.php?action=get_vod_streams
GET /get.php?action=get_vod_info&vod_id={id}
```

### Séries
```
GET /get.php?action=get_series_categories
GET /get.php?action=get_series&category_id={id}
GET /get.php?action=get_series
GET /get.php?action=get_series_info&series_id={id}
```

### EPG (Grade de Programação)
```
GET /get.php?action=get_short_epg&stream_id={id}&limit={n}
GET /get.php?action=get_simple_data_table&stream_id={id}
```

### Playlist e Streaming
```
GET /get.php?username={user}&password={pass}&type=m3u_plus&output=ts
GET /xmltv.php?username={user}&password={pass}
GET /{username}/{password}/{stream_id}
GET /movie/{username}/{password}/{vod_id}.mp4
GET /series/{username}/{password}/{episode_id}.mp4
```

## 🛠️ Tecnologias

- **.NET 10.0** - Framework target
- **ASP.NET Core** - Web API framework
- **C# 14** - Linguagem
- **Serilog** - Logging estruturado

## 📦 Dependências

- **Microsoft.AspNetCore.OpenApi** (10.0.2) - OpenAPI/Swagger
- **Serilog.AspNetCore** (10.0.0) - Logging integrado
- **Serilog.Enrichers.*** - Enriquecedores de contexto
- **Serilog.Sinks.*** - Destinos de log (Console, File, Async)

## 🎯 Funcionalidades

- ✅ **Parser M3U/M3U8** - Parsing completo com suporte a metadados
- ✅ **API Xtream Codes** - Simulação completa e compatível
- ✅ **Canais ao Vivo** - Suporte a categorias e filtros
- ✅ **VOD (Filmes)** - Catálogo de filmes com informações
- ✅ **Séries TV** - Suporte a séries e episódios
- ✅ **EPG Simulado** - Grade de programação dinâmica
- ✅ **Proxy de Streaming** - Range requests e redirecionamento
- ✅ **Autenticação** - Validação básica de usuário/senha
- ✅ **Dados Mock** - Geração automática para testes
- ✅ **OpenAPI/Swagger** - Documentação interativa
- ✅ **Logging Estruturado** - Serilog com contexto rico

## 🔌 Como Usar em Seus Projetos

### 1. Adicionar Referência ao Projeto

```xml
<ItemGroup>
  <ProjectReference Include="..\IPTVProxy.Common\IPTVProxy.Common.csproj" />
</ItemGroup>
```

### 2. Registrar Serviços (Program.cs ou Startup.cs)

```csharp
using IPTVProxy.Common.Extensions;

var builder = WebApplication.CreateBuilder(args);

var m3uFilePath = builder.Configuration["M3uFilePath"] ?? "playlist.m3u";
var serverUrl = builder.Configuration["ServerUrl"] ?? "http://localhost:5000";

// Registra todos os serviços IPTV
builder.Services.AddIptvServices(m3uFilePath, serverUrl);

var app = builder.Build();

// Mapeia endpoints (Controllers ou Minimal APIs)
app.MapXtreamApiEndpoints("/");  // ou "/api/xtreamapi"

app.Run();
```

### 3. Usar Serviços via Dependency Injection

```csharp
[ApiController]
[Route("api/[controller]")]
public class MyController : ControllerBase
{
    private readonly M3uPlaylistService _playlistService;
    private readonly XtreamSimulator _simulator;

    public MyController(M3uPlaylistService playlistService, XtreamSimulator simulator)
    {
        _playlistService = playlistService;
        _simulator = simulator;
    }

    [HttpGet("channels")]
    public IActionResult GetChannels()
    {
        var channels = _playlistService.GetLiveChannels();
        return Ok(channels);
    }
}
```

## 📊 Estrutura de Dados Mock

Quando não há arquivo M3U válido, a biblioteca gera dados mock com:

- **24 canais ao vivo** distribuídos em 6 categorias
- **24 filmes (VOD)** distribuídos em 6 categorias
- **20 séries** distribuídas em 5 categorias
- **EPG simulado** para todos os canais (próximas 7 dias)

Útil para:
- Testes e desenvolvimento
- Demonstrações
- Prototipagem rápida

## 🔒 Segurança

- ✅ Validação de credenciais (demo/demo123)
- ✅ Input validation para IDs e parâmetros
- ✅ Não expõe informações sensíveis em logs
- ✅ Suporte para Range requests seguro

## 📊 Logging

Configuração automática com Serilog:
- **Console** - Output colorido em tempo real
- **Arquivo** - `Logs/logs.json` com rotação diária
- **Enriquecimento** - Contexto (Machine, ProcessId, ThreadId, ClientIp)

## 🧪 Testabilidade

Todos os componentes são projetados para serem testáveis:
- Uso de Dependency Injection
- Serviços stateless ou singleton
- Dados mock para testes sem I/O
- Interfaces limpas

## 📄 Licença

Este projeto é fornecido como está, sem garantias.

## 🤝 Contribuições

Contribuições são bem-vindas! Sinta-se à vontade para:
- Reportar issues
- Sugerir melhorias
- Enviar pull requests
- Compartilhar feedback

---

Desenvolvido com ❤️ para a comunidade IPTV
