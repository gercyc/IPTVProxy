# IPTVProxy - Solução Completa

Uma solução robusta e modular para criar proxies IPTV que simulam a API Xtream Codes a partir de arquivos M3U/M3U8.

## 📋 Descrição da Solução

IPTVProxy é uma solução multi-projetos que oferece **duas abordagens complementares** para implementar um proxy IPTV compatível com Xtream Codes:

- **IPTVProxy** (Controllers) - Implementação tradicional com ASP.NET Core Controllers
- **IPTVProxyMinimal** (Minimal APIs) - Implementação leve com Minimal APIs
- **IPTVProxy.Common** - Biblioteca compartilhada com toda a lógica de negócio

## 🏗️ Arquitetura da Solução

```
C:\source\Test-Projects\IPTVProxy/
├── IPTVProxy/
│   ├── Controllers/              # Controllers ASP.NET Core
│   ├── Extensions/               # Configurações customizadas
│   ├── Program.cs                # Entry point (Controllers)
│   ├── appsettings.json          # Configurações
│   ├── us-grc.m3u                # Playlist exemplo
│   └── README.md                 # Documentação do projeto
│
├── IPTVProxyMinimal/
│   ├── Program.cs                # Entry point (Minimal APIs)
│   ├── appsettings.json          # Configurações
│   ├── us-grc.m3u                # Playlist exemplo
│   └── README.md                 # Documentação do projeto
│
├── IPTVProxy.Common/
│   ├── Models/                   # Definições de dados
│   │   ├── M3u/                  # Modelos M3U
│   │   └── Xtream/               # Modelos Xtream Codes
│   ├── Services/                 # Lógica de negócio
│   │   ├── M3uParser.cs          # Parser M3U
│   │   ├── M3uPlaylistService.cs # Gerenciador de playlist
│   │   └── XtreamSimulator.cs    # Simulador Xtream
│   ├── Api/                      # Endpoints Minimal APIs
│   │   ├── XtreamApiEndpoints.cs # Mapeador de rotas
│   │   └── Endpoints/            # Implementações específicas
│   ├── Extensions/               # Helpers de configuração
│   │   ├── ServiceCollectionExtensions.cs
│   │   └── EndpointExtensions.cs
│   └── README.md                 # Documentação da biblioteca
│
└── README.md                     # Este arquivo
```

## 🔗 Relação entre Projetos

```
┌─────────────────────────────────────┐
│      IPTVProxy.Common               │
│  (Biblioteca Compartilhada)         │
│                                     │
│  • Models (M3U, Xtream)             │
│  • Services (Parser, Simulator)     │
│  • Extensions (DI, Endpoints)       │
└──────────────┬──────────────────────┘
               │
      ┌────────┴────────┐
      │                 │
      ▼                 ▼
  IPTVProxy      IPTVProxyMinimal
  (Controllers)  (Minimal APIs)
```

## 🎯 Qual projeto usar?

### **IPTVProxy** (Controllers) 👈 Use quando:
- Precisa de arquitetura **MVC tradicional** e familiar
- Quer **Controllers estruturados** com atributos
- Prefere um padrão que você já conhece bem
- Está trabalhando em um projeto **grande e complexo**
- Precisa de **middlewares customizados** extensivos

**Inicie com:** `dotnet run --project IPTVProxy`

### **IPTVProxyMinimal** (Minimal APIs) 👈 Use quando:
- Quer uma solução **leve e minimalista**
- Está criando um **microserviço** ou API simples
- Prefere uma abordagem **funcional e declarativa**
- Quer **menos boilerplate** de código
- Prioriza **performance e simplicidade**

**Inicie com:** `dotnet run --project IPTVProxyMinimal`

## 🚀 Quick Start

### 1. **Clone ou extraia o repositório**

```bash
cd C:\source\Test-Projects\IPTVProxy
```

### 2. **Restaure as dependências**

```bash
dotnet restore
```

### 3. **Escolha e execute um dos projetos**

#### Opção A: Com Controllers (Tradicional)
```bash
dotnet run --project IPTVProxy
```

#### Opção B: Com Minimal APIs (Moderno)
```bash
dotnet run --project IPTVProxyMinimal
```

### 4. **Acesse a documentação**

O servidor inicia em **`http://localhost:5000`**

- **Swagger UI**: `http://localhost:5000/swagger`
- **ReDoc**: `http://localhost:5000/docs`
- **OpenAPI JSON**: `http://localhost:5000/openapi/v1.json`

### 5. **Use as credenciais padrão**

```
Username: demo
Password: demo123
```

## 📦 Estrutura de Projetos

### IPTVProxy.Common (Biblioteca)
Contém toda a **lógica compartilhada**:
- Parsing de M3U/M3U8
- Simulação da API Xtream Codes
- Modelos de dados
- Configuração de serviços (DI)
- Endpoints Minimal APIs

**Referência:** Usado por IPTVProxy e IPTVProxyMinimal

[📖 Documentação detalhada](./IPTVProxy.Common/README.md)

### IPTVProxy (Controllers)
Implementação com **ASP.NET Core Controllers**:
- Controllers MVC tradicionais
- Injeção de dependência
- Configurações customizadas
- Documentação Swagger/ReDoc

**Tecnologias:** .NET 10.0, ASP.NET Core, Controllers, Serilog

[📖 Documentação detalhada](./IPTVProxy/README.md)

### IPTVProxyMinimal (Minimal APIs)
Implementação com **Minimal APIs**:
- Endpoints declarativos e funcionais
- Menos overhead e boilerplate
- Maior simplicidade
- Melhor para microserviços

**Tecnologias:** .NET 10.0, ASP.NET Core, Minimal APIs, Serilog

[📖 Documentação detalhada](./IPTVProxyMinimal/README.md)

## 🛠️ Tecnologias

- **.NET 10.0** - Framework target para todos os projetos
- **ASP.NET Core 10.0** - Web framework
- **C# 14** - Linguagem de programação
- **Serilog** - Logging estruturado
- **Swagger/OpenAPI** - Documentação interativa

## 📝 Configuração

Cada projeto web possui um arquivo `appsettings.json`:

```json
{
  "M3uFilePath": "us-grc.m3u",
  "ServerUrl": "http://localhost:5000",
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

### Variáveis de Ambiente

- `ASPNETCORE_URLS` - URL do servidor (padrão: http://localhost:5000)
- `M3uFilePath` - Caminho do arquivo M3U (padrão: us-grc.m3u)

## 🔨 Build e Test

### Build da solução completa
```bash
dotnet build
```

### Build de um projeto específico
```bash
dotnet build --project IPTVProxy
dotnet build --project IPTVProxyMinimal
dotnet build --project IPTVProxy.Common
```

### Limpar artefatos
```bash
dotnet clean
```

## 📡 Endpoints Disponíveis

Ambas as implementações (Controllers e Minimal APIs) suportam os mesmos endpoints:

### Player API
```
GET /player_api.php?username=demo&password=demo123
```

### Canais ao Vivo
```
GET /api/xtreamapi/get.php?action=get_live_categories
GET /api/xtreamapi/get.php?action=get_live_streams
GET /api/xtreamapi/get.php?action=get_live_streams&category_id={id}
```

### VOD (Filmes)
```
GET /api/xtreamapi/get.php?action=get_vod_categories
GET /api/xtreamapi/get.php?action=get_vod_streams
GET /api/xtreamapi/get.php?action=get_vod_info&vod_id={id}
```

### Séries
```
GET /api/xtreamapi/get.php?action=get_series_categories
GET /api/xtreamapi/get.php?action=get_series
GET /api/xtreamapi/get.php?action=get_series_info&series_id={id}
```

### EPG
```
GET /api/xtreamapi/get.php?action=get_short_epg&stream_id={id}&limit={n}
GET /api/xtreamapi/get.php?action=get_simple_data_table&stream_id={id}
```

### Playlist e Streaming
```
GET /get.php?username={user}&password={pass}&type=m3u_plus&output=ts
GET /xmltv.php?username={user}&password={pass}
GET /{username}/{password}/{stream_id}
GET /movie/{username}/{password}/{vod_id}.mp4
GET /series/{username}/{password}/{episode_id}.mp4
```

## 📊 Funcionalidades

- ✅ **Parsing M3U/M3U8** completo com suporte a metadados
- ✅ **Canais ao Vivo** com categorias
- ✅ **VOD (Filmes)** com informações detalhadas
- ✅ **Séries TV** com episódios
- ✅ **EPG Simulado** (Electronic Program Guide)
- ✅ **Proxy de Streaming** com Range requests
- ✅ **API Xtream Codes** compatível
- ✅ **Playlist M3U** e EPG XML
- ✅ **Dados Mock** para testes sem M3U
- ✅ **Documentação OpenAPI/Swagger**
- ✅ **Logging Estruturado** (Serilog)
- ✅ **Duas implementações** (Controllers e Minimal APIs)
- ✅ **Código compartilhado** e reutilizável

## 🎯 Comparação de Implementações

| Aspecto | Controllers | Minimal APIs |
|---------|-------------|--------------|
| Abordagem | MVC Tradicional | Declarativa |
| Complexidade | Média-Alta | Baixa |
| Estrutura | Organizada em Controllers | Endpoints simples |
| Performance | Otimizada | Otimizada |
| Aprendizado | Padrão MVC | Funcional |
| Ideal para | Projetos complexos | Microserviços |
| Projeto | IPTVProxy | IPTVProxyMinimal |

## 📝 Exemplo de Uso com Players

### IPTV Smarters
1. Adicionar novo provedor
2. Tipo: Xtream Codes API
3. URL: `http://localhost:5000`
4. Username: `demo`
5. Password: `demo123`

### Perfect Player
1. Configurações → Geral → Listas de reprodução
2. Adicionar lista → Xtream Codes
3. URL: `http://localhost:5000`
4. Login: `demo`
5. Senha: `demo123`

## 📚 Documentação Adicional

Cada projeto possui sua própria documentação detalhada:

- **[IPTVProxy (Controllers)](./IPTVProxy/README.md)** - Documentação completa da versão com Controllers
- **[IPTVProxyMinimal (Minimal APIs)](./IPTVProxyMinimal/README.md)** - Documentação da versão minimalista
- **[IPTVProxy.Common (Biblioteca)](./IPTVProxy.Common/README.md)** - Documentação da biblioteca compartilhada

## 🔒 Segurança

- ✅ Validação básica de credenciais (demo/demo123)
- ✅ Input validation para parâmetros
- ✅ Range requests seguros
- ⚠️ **Nota:** Credenciais são apenas para demonstração. Em produção, implemente autenticação real.

## 📊 Logging

Ambos os projetos usam **Serilog** com:
- **Console** - Output colorido em tempo real
- **Arquivo** - `Logs/logs.json` com rotação diária
- **Estruturado** - Contexto rico (Machine, ProcessId, ThreadId, ClientIp, UserAgent)

## 🆘 Troubleshooting

### Porta 5000 já está em uso
Defina uma porta diferente via variável de ambiente:
```bash
set ASPNETCORE_URLS=http://localhost:5001
dotnet run --project IPTVProxy
```

### Arquivo M3U não encontrado
O sistema gera dados mock automaticamente. Para usar seu M3U:
1. Coloque o arquivo na pasta do projeto
2. Atualize `appsettings.json` com o caminho correto

### Swagger/ReDoc não aparece
Verifique se você tem as dependências Swashbuckle instaladas e se `AddSwaggerGen()` está configurado em Program.cs.

## 🤝 Contribuições

Contribuições são bem-vindas! Você pode:
- Reportar issues
- Sugerir melhorias
- Enviar pull requests
- Compartilhar feedback

## 📄 Licença

Este projeto é fornecido como está, sem garantias.

## 📞 Suporte

Para dúvidas ou problemas:
1. Verifique a documentação específica de cada projeto
2. Consulte os logs em `Logs/logs.json`
3. Abra uma issue no repositório

---

**Desenvolvido com ❤️ para a comunidade IPTV**

**Última atualização:** 2024
**Versão:** .NET 10.0
