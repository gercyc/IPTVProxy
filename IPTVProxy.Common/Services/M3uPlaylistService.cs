namespace IPTVProxy.Common.Services;

using IPTVProxy.Common.Models.M3u;
using IPTVProxy.Common.Models.Xtream;

/// <summary>
/// Serviço para carregar e gerenciar dados de playlists M3U.
/// Converte entries M3U para os modelos da XtreamAPI.
/// </summary>
public class M3uPlaylistService
{
    private M3uPlaylist? _playlist;
    private List<Category> _liveCategories = [];
    private List<Category> _vodCategories = [];
    private List<Category> _seriesCategories = [];
    private List<Channel> _channels = [];
    private List<Vod> _vods = [];
    private List<Series> _series = [];
    private string _serverUrl = string.Empty;
    private string _username = string.Empty;
    private string _password = string.Empty;

    /// <summary>
    /// Inicializa o serviço com as configurações de servidor para gerar URLs de proxy.
    /// </summary>
    public M3uPlaylistService(string serverUrl, string username, string password)
    {
        ArgumentNullException.ThrowIfNull(serverUrl);
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty", nameof(username));
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        _serverUrl = serverUrl;
        _username = username;
        _password = password;
    }

    /// <summary>
    /// Inicializa o serviço sem configurações de proxy (padrão para compatibilidade).
    /// </summary>
    public M3uPlaylistService()
    {
    }

    /// <summary>
    /// Indica se há uma playlist carregada.
    /// </summary>
    public bool IsLoaded => _playlist != null;

    /// <summary>
    /// Playlist M3U carregada.
    /// </summary>
    public M3uPlaylist? Playlist
    {
        get => _playlist;
        set => _playlist = value;
    }

    /// <summary>
    /// Carrega uma playlist M3U de um arquivo.
    /// </summary>
    public void LoadFromFile(string filePath)
    {
        _playlist = M3uParser.ParseFile(filePath);
        ProcessPlaylistInternal();
    }

    /// <summary>
    /// Carrega uma playlist M3U de um arquivo (async).
    /// </summary>
    public async Task LoadFromFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        _playlist = await M3uParser.ParseFileAsync(filePath, cancellationToken);
        ProcessPlaylistInternal();
    }

    /// <summary>
    /// Carrega uma playlist M3U de uma string.
    /// </summary>
    public void LoadFromContent(string content)
    {
        _playlist = M3uParser.Parse(content);
        ProcessPlaylistInternal();
    }

    /// <summary>
    /// Processa a playlist carregada (público para reinicialização).
    /// </summary>
    public void ProcessPlaylist() => ProcessPlaylistInternal();

    /// <summary>
    /// Retorna todas as categorias de canais ao vivo.
    /// </summary>
    public List<Category> GetLiveCategories() => _liveCategories;

    /// <summary>
    /// Retorna todas as categorias de VOD.
    /// </summary>
    public List<Category> GetVodCategories() => _vodCategories;

    /// <summary>
    /// Retorna todas as categorias de séries.
    /// </summary>
    public List<Category> GetSeriesCategories() => _seriesCategories;

    /// <summary>
    /// Retorna todos os canais ao vivo.
    /// </summary>
    public List<Channel> GetAllChannels() => _channels;

    /// <summary>
    /// Retorna canais por categoria.
    /// </summary>
    public List<Channel> GetChannelsByCategory(string categoryId) =>
        _channels.Where(c => c.CategoryId == categoryId).ToList();

    /// <summary>
    /// Retorna todos os VODs.
    /// </summary>
    public List<Vod> GetAllVods() => _vods;

    /// <summary>
    /// Retorna VODs por categoria.
    /// </summary>
    public List<Vod> GetVodsByCategory(string categoryId) =>
        _vods.Where(v => v.CategoryId == categoryId).ToList();

    /// <summary>
    /// Retorna todas as séries.
    /// </summary>
    public List<Series> GetAllSeries() => _series;

    /// <summary>
    /// Retorna séries por categoria.
    /// </summary>
    public List<Series> GetSeriesByCategory(string categoryId) =>
        _series.Where(s => s.CategoryId == categoryId).ToList();

    /// <summary>
    /// Busca um canal pelo StreamId.
    /// </summary>
    public Channel? GetChannelById(int streamId) =>
        _channels.FirstOrDefault(c => c.StreamId == streamId);

    /// <summary>
    /// Busca um VOD pelo StreamId.
    /// </summary>
    public Vod? GetVodById(int streamId) =>
        _vods.FirstOrDefault(v => v.StreamId == streamId);

    /// <summary>
    /// Busca uma série pelo SeriesId.
    /// </summary>
    public Series? GetSeriesById(int seriesId) =>
        _series.FirstOrDefault(s => s.SeriesId == seriesId);

    /// <summary>
    /// Retorna a URL de stream para um canal.
    /// </summary>
    public string? GetStreamUrl(int streamId)
    {
        var entry = _playlist?.Entries.FirstOrDefault(e => e.StreamId == streamId);
        return entry?.Url;
    }

    private void ProcessPlaylistInternal()
    {
        if (_playlist == null) return;

        var liveCategories = new Dictionary<string, int>();
        var vodCategories = new Dictionary<string, int>();
        var seriesCategories = new Dictionary<string, int>();

        _channels = [];
        _vods = [];
        _series = [];

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var liveCategoryId = 1;
        var vodCategoryId = 1000;
        var seriesCategoryId = 2000;

        foreach (var entry in _playlist.Entries)
        {
            var groupTitle = entry.GroupTitle ?? "Outros";

            switch (entry.StreamType)
            {
                case M3uStreamType.Live:
                    if (!liveCategories.TryGetValue(groupTitle, out var liveCatId))
                    {
                        liveCatId = liveCategoryId++;
                        liveCategories[groupTitle] = liveCatId;
                    }
                    _channels.Add(ConvertToChannel(entry, liveCatId.ToString(), now, entry.StreamId));
                    break;

                case M3uStreamType.Movie:
                    if (!vodCategories.TryGetValue(groupTitle, out var vodCatId))
                    {
                        vodCatId = vodCategoryId++;
                        vodCategories[groupTitle] = vodCatId;
                    }
                    _vods.Add(ConvertToVod(entry, vodCatId.ToString(), now, entry.StreamId));
                    break;

                case M3uStreamType.Series:
                    if (!seriesCategories.TryGetValue(groupTitle, out var seriesCatId))
                    {
                        seriesCatId = seriesCategoryId++;
                        seriesCategories[groupTitle] = seriesCatId;
                    }
                    _series.Add(ConvertToSeries(entry, seriesCatId.ToString(), now, entry.StreamId));
                    break;
            }
        }

        _liveCategories = liveCategories
            .Select(kvp => new Category
            {
                CategoryId = kvp.Value.ToString(),
                CategoryName = kvp.Key,
                ParentId = 0
            })
            .OrderBy(c => c.CategoryName)
            .ToList();

        _vodCategories = vodCategories
            .Select(kvp => new Category
            {
                CategoryId = kvp.Value.ToString(),
                CategoryName = kvp.Key,
                ParentId = 0
            })
            .OrderBy(c => c.CategoryName)
            .ToList();

        _seriesCategories = seriesCategories
            .Select(kvp => new Category
            {
                CategoryId = kvp.Value.ToString(),
                CategoryName = kvp.Key,
                ParentId = 0
            })
            .OrderBy(c => c.CategoryName)
            .ToList();
    }

    private Channel ConvertToChannel(M3uEntry entry, string categoryId, string addedTimestamp, int streamId)
    {
        // DirectSource deve sempre ser a URL ORIGINAL para evitar loops
        return new Channel
        {
            Num = entry.StreamId,
            Name = entry.Name,
            StreamType = "live",
            StreamId = entry.StreamId,
            StreamIcon = entry.TvgLogo ?? string.Empty,
            EpgChannelId = entry.TvgId,
            Added = addedTimestamp,
            CategoryId = categoryId,
            TvArchive = 0,
            TvArchiveDuration = 0,
            DirectSource = entry.Url // URL ORIGINAL
        };
    }

    private Vod ConvertToVod(M3uEntry entry, string categoryId, string addedTimestamp, int streamId)
    {
        // DirectSource deve sempre ser a URL ORIGINAL para evitar loops
        return new Vod
        {
            Num = entry.StreamId,
            Name = entry.Name,
            StreamType = "movie",
            StreamId = entry.StreamId,
            StreamIcon = entry.TvgLogo ?? string.Empty,
            Rating = "0",
            Rating5Based = 0,
            Added = addedTimestamp,
            CategoryId = categoryId,
            ContainerExtension = GetExtensionFromUrl(entry.Url),
            DirectSource = entry.Url // URL ORIGINAL
        };
    }

    private Series ConvertToSeries(M3uEntry entry, string categoryId, string addedTimestamp, int streamId)
    {
        return new Series
        {
            Num = entry.StreamId,
            Name = entry.Name,
            SeriesId = entry.StreamId,
            Cover = entry.TvgLogo ?? string.Empty,
            Plot = string.Empty,
            Cast = string.Empty,
            Director = string.Empty,
            Genre = entry.GroupTitle ?? string.Empty,
            ReleaseDate = string.Empty,
            LastModified = addedTimestamp,
            Rating = "0",
            Rating5Based = 0,
            BackdropPath = [],
            EpisodeRunTime = string.Empty,
            CategoryId = categoryId
        };
    }

    /// <summary>
    /// Gera URL de proxy para um stream (usado apenas na geração de playlist).
    /// </summary>
    public string GetProxyUrl(int streamId, string originalUrl, M3uStreamType streamType)
    {
        // Se não há servidor configurado, usar URL original
        if (string.IsNullOrEmpty(_serverUrl))
        {
            return originalUrl;
        }

        // Determinar a extensão apropriada
        var extension = GetExtensionFromUrl(originalUrl);
        if (streamType == M3uStreamType.Movie)
        {
            extension = "mp4";
        }

        // Construir URL de proxy no formato do XtreamSimulator
        if (streamType == M3uStreamType.Movie)
        {
            return $"{_serverUrl}/movie/{_username}/{_password}/{streamId}.{extension}";
        }

        return $"{_serverUrl}/{_username}/{_password}/{streamId}.{extension}";
    }

    private static string GetExtensionFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return "mp4";

        var uri = new Uri(url, UriKind.RelativeOrAbsolute);
        var path = uri.IsAbsoluteUri ? uri.AbsolutePath : url;

        var lastDot = path.LastIndexOf('.');
        if (lastDot > 0 && lastDot < path.Length - 1)
        {
            var ext = path[(lastDot + 1)..].ToLowerInvariant();
            // Remove query string if present
            var queryIndex = ext.IndexOf('?');
            if (queryIndex > 0)
            {
                ext = ext[..queryIndex];
            }

            if (ext is "mp4" or "mkv" or "avi" or "m3u8" or "ts")
            {
                return ext;
            }
        }

        return "mp4";
    }
}
