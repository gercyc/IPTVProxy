namespace IPTVProxy.Common.Services;

using IPTVProxy.Common.Models.Xtream;

/// <summary>
/// Serviço que simula uma XtreamAPI com dados mock ou de arquivo M3U.
/// </summary>
public class XtreamSimulator
{
    private const string ValidUsername = "demo";
    private const string ValidPassword = "demo123";
    private readonly string _serverUrl;

    private readonly M3uPlaylistService? _playlistService;
    private readonly List<Category> _channelCategories;
    private readonly List<Category> _vodCategories;
    private readonly List<Category> _seriesCategories;
    private readonly List<Channel> _channels;
    private readonly List<Vod> _vods;
    private readonly List<Series> _series;
    private readonly Dictionary<int, VodInfo> _vodInfos;
    private readonly Dictionary<int, SeriesInfo> _seriesInfos;

    /// <summary>
    /// Cria um simulador com dados mock.
    /// </summary>
    public XtreamSimulator(string serverUrl)
    {
        _serverUrl = serverUrl;
        _channelCategories = GenerateChannelCategories();
        _vodCategories = GenerateVodCategories();
        _seriesCategories = GenerateSeriesCategories();
        _channels = GenerateChannels();
        _vods = GenerateVods();
        _series = GenerateSeries();
        _vodInfos = GenerateVodInfos();
        _seriesInfos = GenerateSeriesInfos();
    }

    /// <summary>
    /// Cria um simulador usando um M3uPlaylistService já carregado.
    /// </summary>
    public XtreamSimulator(M3uPlaylistService playlistService, string serverUrl)
    {
        ArgumentNullException.ThrowIfNull(playlistService);

        _serverUrl = serverUrl;
        _playlistService = playlistService;

        // Usar diretamente o playlistService fornecido (já tem as URLs originais)
        _channelCategories = playlistService.GetLiveCategories();
        _vodCategories = playlistService.GetVodCategories();
        _seriesCategories = playlistService.GetSeriesCategories();
        _channels = playlistService.GetAllChannels();
        _vods = playlistService.GetAllVods();
        _series = playlistService.GetAllSeries();
        _vodInfos = GenerateVodInfosFromPlaylist();
        _seriesInfos = GenerateSeriesInfosFromPlaylist();
    }

    /// <summary>
    /// Valida as credenciais do usuário.
    /// </summary>
    public bool ValidateCredentials(string? username, string? password)
    {
        return username == ValidUsername && password == ValidPassword;
    }

    /// <summary>
    /// Retorna a resposta de login.
    /// </summary>
    public LoginResponse GetLoginResponse(string username, string password)
    {
        var now = DateTimeOffset.UtcNow;
        return new LoginResponse
        {
            UserInfo = new UserInfo
            {
                Username = username,
                Password = password,
                Message = "Welcome!",
                Auth = 1,
                Status = "Active",
                ExpDate = now.AddYears(1).ToUnixTimeSeconds().ToString(),
                IsTrial = "0",
                ActiveCons = "0",
                CreatedAt = now.AddMonths(-6).ToUnixTimeSeconds().ToString(),
                MaxConnections = "0",
                AllowedOutputFormats = ["m3u8", "ts", "rtmp"]
            },
            ServerInfo = new ServerInfo
            {
                Url = _serverUrl,
                Port = "5000",
                HttpsPort = "443",
                ServerProtocol = "http",
                RtmpPort = "8880",
                Timezone = "America/Sao_Paulo",
                TimestampNow = now.ToUnixTimeSeconds(),
                TimeNow = now.ToString("yyyy-MM-dd HH:mm:ss")
            }
        };
    }

    /// <summary>
    /// Retorna resposta de erro de autenticação.
    /// </summary>
    public AuthErrorResponse GetAuthErrorResponse()
    {
        return new AuthErrorResponse
        {
            UserInfo = new AuthErrorUserInfo
            {
                Auth = 0,
                Status = "Disabled",
                Message = "Invalid credentials"
            }
        };
    }

    public List<Category> GetChannelCategories() => _channelCategories;

    public List<Category> GetVodCategories() => _vodCategories;

    public List<Category> GetSeriesCategories() => _seriesCategories;

    public List<Channel> GetAllChannels() => _channels;

    public List<Channel> GetChannelsByCategory(string categoryId) =>
        _channels.Where(c => c.CategoryId == categoryId).ToList();

    public List<ChannelResponse> GetAllChannelsResponse(string username, string password) =>
        _channels.Select(c => ToChannelResponse(c, username, password)).ToList();

    public List<ChannelResponse> GetChannelsByCategoryResponse(string categoryId, string username, string password) =>
        _channels
            .Where(c => c.CategoryId == categoryId)
            .Select(c => ToChannelResponse(c, username, password))
            .ToList();

    public List<Vod> GetAllVods() => _vods;

    public List<Vod> GetVodsByCategory(string categoryId) =>
        _vods.Where(v => v.CategoryId == categoryId).ToList();

    public List<VodResponse> GetAllVodsResponse(string username, string password) =>
        _vods.Select(v => ToVodResponse(v, username, password)).ToList();

    public List<VodResponse> GetVodsByCategoryResponse(string categoryId, string username, string password) =>
        _vods
            .Where(v => v.CategoryId == categoryId)
            .Select(v => ToVodResponse(v, username, password))
            .ToList();

    public List<Series> GetAllSeries() => _series;

    public List<Series> GetSeriesByCategory(string categoryId) =>
        _series.Where(s => s.CategoryId == categoryId).ToList();

    public List<SeriesResponse> GetAllSeriesResponse(string username, string password) =>
        _series.Select(s => ToSeriesResponse(s, username, password)).ToList();

    public List<SeriesResponse> GetSeriesByCategoryResponse(string categoryId, string username, string password) =>
        _series
            .Where(s => s.CategoryId == categoryId)
            .Select(s => ToSeriesResponse(s, username, password))
            .ToList();

    public VodInfo? GetVodInfo(int id) =>
        _vodInfos.GetValueOrDefault(id);

    public SeriesInfo? GetSeriesInfo(int id) =>
        _seriesInfos.GetValueOrDefault(id);

    /// <summary>
    /// Retorna informações de série com URLs de proxy para episódios.
    /// </summary>
    public SeriesInfoResponse? GetSeriesInfoResponse(int seriesId, string username, string password)
    {
        var seriesInfo = _seriesInfos.GetValueOrDefault(seriesId);
        if (seriesInfo == null)
        {
            return null;
        }

        return ConvertToSeriesInfoResponse(seriesInfo, username, password);
    }

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
    /// Busca um episódio pelo Id.
    /// </summary>
    public Episode? GetEpisodeById(string episodeId)
    {
        foreach (var seriesInfo in _seriesInfos.Values)
        {
            foreach (var episodeList in seriesInfo.Episodes.Values)
            {
                var episode = episodeList.FirstOrDefault(e => e.Id == episodeId);
                if (episode != null)
                {
                    return episode;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Retorna EPG curto para um canal (get_short_epg).
    /// </summary>
    public EpgResponse GetShortEpg(int streamId, int limit = 4)
    {
        var channel = _channels.FirstOrDefault(c => c.StreamId == streamId);
        if (channel == null)
        {
            return new EpgResponse();
        }

        var entries = GenerateEpgForChannel(channel, hoursRange: 6)
            .Take(limit)
            .ToList();

        return new EpgResponse { EpgListings = entries };
    }

    /// <summary>
    /// Retorna EPG completo para um canal (get_simple_data_table).
    /// </summary>
    public EpgResponse GetFullEpgForChannel(int streamId)
    {
        var channel = _channels.FirstOrDefault(c => c.StreamId == streamId);
        if (channel == null)
        {
            return new EpgResponse();
        }

        return new EpgResponse
        {
            EpgListings = GenerateEpgForChannel(channel, hoursRange: 24)
        };
    }

    /// <summary>
    /// Gera a playlist M3U completa (get.php).
    /// </summary>
    public string GenerateM3uPlaylist(string username, string password, string? type, string? output)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("#EXTM3U");

        // Se output não foi especificado, usar padrão baseado no tipo de stream
        var defaultExtension = output ?? "m3u8";

        foreach (var channel in _channels)
        {
            var category = _channelCategories.FirstOrDefault(c => c.CategoryId == channel.CategoryId);
            sb.AppendLine($"#EXTINF:-1 tvg-id=\"{channel.EpgChannelId}\" tvg-name=\"{channel.Name}\" tvg-logo=\"{channel.StreamIcon}\" group-title=\"{category?.CategoryName ?? "Outros"}\",{channel.Name}");

            // Determinar a extensão apropriada baseada no DirectSource
            string extension;
            if (string.IsNullOrEmpty(channel.DirectSource))
            {
                extension = defaultExtension;
            }
            else if (channel.DirectSource.Contains(".m3u8", StringComparison.OrdinalIgnoreCase))
            {
                // Se a fonte é HLS, usar m3u8
                extension = "m3u8";
            }
            else
            {
                // Para streams diretos, usar a extensão especificada ou sem extensão
                extension = defaultExtension;
            }

            sb.AppendLine($"{_serverUrl}/{username}/{password}/{channel.StreamId}.{extension}");
        }

        foreach (var vod in _vods)
        {
            var category = _vodCategories.FirstOrDefault(c => c.CategoryId == vod.CategoryId);
            sb.AppendLine($"#EXTINF:-1 tvg-name=\"{vod.Name}\" tvg-logo=\"{vod.StreamIcon}\" group-title=\"{category?.CategoryName ?? "Filmes"}\",{vod.Name}");
            sb.AppendLine($"{_serverUrl}/{username}/{password}/{vod.StreamId}.{vod.ContainerExtension}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gera o EPG completo em formato XMLTV (xmltv.php).
    /// </summary>
    public string GenerateXmltvEpg()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<!DOCTYPE tv SYSTEM \"xmltv.dtd\">");
        sb.AppendLine("<tv generator-info-name=\"XtreamSimulator\">");

        // Canais
        foreach (var channel in _channels)
        {
            sb.AppendLine($"  <channel id=\"{channel.EpgChannelId ?? channel.StreamId.ToString()}\">");
            sb.AppendLine($"    <display-name>{System.Security.SecurityElement.Escape(channel.Name)}</display-name>");
            sb.AppendLine($"    <icon src=\"{channel.StreamIcon}\" />");
            sb.AppendLine("  </channel>");
        }

        // Programas
        foreach (var channel in _channels)
        {
            var epgEntries = GenerateEpgForChannel(channel, hoursRange: 48);
            foreach (var entry in epgEntries)
            {
                var startFormatted = DateTimeOffset.FromUnixTimeSeconds(entry.StartTimestamp).ToString("yyyyMMddHHmmss +0000");
                var stopFormatted = DateTimeOffset.FromUnixTimeSeconds(entry.StopTimestamp).ToString("yyyyMMddHHmmss +0000");

                sb.AppendLine($"  <programme start=\"{startFormatted}\" stop=\"{stopFormatted}\" channel=\"{entry.ChannelId}\">");
                sb.AppendLine($"    <title lang=\"{entry.Lang}\">{System.Security.SecurityElement.Escape(entry.Title)}</title>");
                sb.AppendLine($"    <desc lang=\"{entry.Lang}\">{System.Security.SecurityElement.Escape(entry.Description)}</desc>");
                sb.AppendLine("  </programme>");
            }
        }

        sb.AppendLine("</tv>");
        return sb.ToString();
    }

    private List<Category> GenerateChannelCategories() =>
    [
        new() { CategoryId = "1", CategoryName = "Esportes", ParentId = 0 },
        new() { CategoryId = "2", CategoryName = "Filmes e Séries", ParentId = 0 },
        new() { CategoryId = "3", CategoryName = "Notícias", ParentId = 0 },
        new() { CategoryId = "4", CategoryName = "Infantil", ParentId = 0 },
        new() { CategoryId = "5", CategoryName = "Documentários", ParentId = 0 },
        new() { CategoryId = "6", CategoryName = "Variedades", ParentId = 0 }
    ];

    private List<Category> GenerateVodCategories() =>
    [
        new() { CategoryId = "101", CategoryName = "Ação", ParentId = 0 },
        new() { CategoryId = "102", CategoryName = "Comédia", ParentId = 0 },
        new() { CategoryId = "103", CategoryName = "Drama", ParentId = 0 },
        new() { CategoryId = "104", CategoryName = "Terror", ParentId = 0 },
        new() { CategoryId = "105", CategoryName = "Ficção Científica", ParentId = 0 },
        new() { CategoryId = "106", CategoryName = "Romance", ParentId = 0 }
    ];

    private List<Category> GenerateSeriesCategories() =>
    [
        new() { CategoryId = "201", CategoryName = "Drama", ParentId = 0 },
        new() { CategoryId = "202", CategoryName = "Comédia", ParentId = 0 },
        new() { CategoryId = "203", CategoryName = "Ação", ParentId = 0 },
        new() { CategoryId = "204", CategoryName = "Suspense", ParentId = 0 },
        new() { CategoryId = "205", CategoryName = "Ficção Científica", ParentId = 0 }
    ];

    private List<Channel> GenerateChannels()
    {
        var channels = new List<Channel>();
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        var channelData = new (string Name, string CategoryId, string? EpgId)[]
        {
            ("ESPN Brasil", "1", "espn.br"),
            ("SporTV", "1", "sportv.br"),
            ("Fox Sports", "1", "foxsports.br"),
            ("Band Sports", "1", "bandsports.br"),
            ("HBO", "2", "hbo.br"),
            ("Telecine Premium", "2", "telecine.br"),
            ("TNT", "2", "tnt.br"),
            ("Warner Channel", "2", "warner.br"),
            ("Globo News", "3", "globonews.br"),
            ("CNN Brasil", "3", "cnn.br"),
            ("Band News", "3", "bandnews.br"),
            ("Record News", "3", "recordnews.br"),
            ("Cartoon Network", "4", "cartoon.br"),
            ("Disney Channel", "4", "disney.br"),
            ("Nickelodeon", "4", "nick.br"),
            ("Discovery Kids", "4", "discoverykids.br"),
            ("Discovery Channel", "5", "discovery.br"),
            ("National Geographic", "5", "natgeo.br"),
            ("History Channel", "5", "history.br"),
            ("Animal Planet", "5", "animalplanet.br"),
            ("Multishow", "6", "multishow.br"),
            ("GNT", "6", "gnt.br"),
            ("Comedy Central", "6", "comedycentral.br"),
            ("MTV", "6", "mtv.br")
        };

        for (var i = 0; i < channelData.Length; i++)
        {
            var (name, categoryId, epgId) = channelData[i];
            channels.Add(new Channel
            {
                Num = i + 1,
                Name = name,
                StreamType = "live",
                StreamId = 1000 + i,
                StreamIcon = $"{_serverUrl}/icons/channel_{1000 + i}.png",
                EpgChannelId = epgId,
                Added = now,
                CategoryId = categoryId,
                TvArchive = 1,
                TvArchiveDuration = 7,
                DirectSource = string.Empty
            });
        }

        return channels;
    }

    private List<Vod> GenerateVods()
    {
        var vods = new List<Vod>();
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        var vodData = new (string Name, string CategoryId, double Rating)[]
        {
            ("Velozes e Furiosos 10", "101", 7.5),
            ("John Wick 4", "101", 8.2),
            ("Missão Impossível 7", "101", 7.8),
            ("Operação Fortune", "101", 6.5),
            ("Se Beber Não Case 4", "102", 6.8),
            ("Barbie", "102", 7.0),
            ("Super Mario Bros", "102", 7.2),
            ("Elementos", "102", 7.5),
            ("Oppenheimer", "103", 8.9),
            ("Pobres Criaturas", "103", 8.1),
            ("Anatomia de uma Queda", "103", 7.9),
            ("Os Fabelmans", "103", 7.6),
            ("O Exorcista: O Devoto", "104", 5.5),
            ("A Freira 2", "104", 5.8),
            ("O Telefone Preto", "104", 7.0),
            ("M3GAN", "104", 6.9),
            ("Duna: Parte 2", "105", 8.8),
            ("Avatar 2", "105", 7.6),
            ("Guardiões da Galáxia 3", "105", 8.0),
            ("The Flash", "105", 6.8),
            ("A Pequena Sereia", "106", 7.2),
            ("Todos Menos Você", "106", 6.5),
            ("Persuasão", "106", 5.9),
            ("Não Se Preocupe Querida", "106", 6.2)
        };

        for (var i = 0; i < vodData.Length; i++)
        {
            var (name, categoryId, rating) = vodData[i];
            vods.Add(new Vod
            {
                Num = i + 1,
                Name = name,
                StreamType = "movie",
                StreamId = 2000 + i,
                StreamIcon = $"{_serverUrl}/posters/vod_{2000 + i}.jpg",
                Rating = rating.ToString("F1"),
                Rating5Based = rating / 2,
                Added = now,
                CategoryId = categoryId,
                ContainerExtension = "mp4",
                DirectSource = string.Empty
            });
        }

        return vods;
    }

    private List<Series> GenerateSeries()
    {
        var series = new List<Series>();
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        var seriesData = new (string Name, string CategoryId, double Rating, string Genre)[]
        {
            ("Breaking Bad", "201", 9.5, "Drama, Crime"),
            ("The Crown", "201", 8.6, "Drama, Biografia"),
            ("Succession", "201", 8.9, "Drama"),
            ("House of the Dragon", "201", 8.4, "Drama, Fantasia"),
            ("The Office", "202", 9.0, "Comédia"),
            ("Brooklyn Nine-Nine", "202", 8.4, "Comédia"),
            ("Ted Lasso", "202", 8.8, "Comédia, Drama"),
            ("Only Murders in the Building", "202", 8.1, "Comédia, Mistério"),
            ("The Mandalorian", "203", 8.7, "Ação, Aventura"),
            ("Jack Ryan", "203", 8.0, "Ação, Drama"),
            ("Reacher", "203", 8.1, "Ação, Crime"),
            ("The Last of Us", "203", 8.8, "Ação, Drama"),
            ("True Detective", "204", 8.9, "Suspense, Crime"),
            ("Mindhunter", "204", 8.6, "Suspense, Crime"),
            ("Severance", "204", 8.7, "Suspense, Drama"),
            ("Dark", "204", 8.8, "Suspense, Ficção Científica"),
            ("Stranger Things", "205", 8.7, "Ficção Científica, Drama"),
            ("Black Mirror", "205", 8.8, "Ficção Científica"),
            ("The Expanse", "205", 8.5, "Ficção Científica"),
            ("Westworld", "205", 8.6, "Ficção Científica")
        };

        for (var i = 0; i < seriesData.Length; i++)
        {
            var (name, categoryId, rating, genre) = seriesData[i];
            series.Add(new Series
            {
                Num = i + 1,
                Name = name,
                SeriesId = 3000 + i,
                Cover = $"{_serverUrl}/posters/series_{3000 + i}.jpg",
                Plot = $"Uma série incrível sobre {name}.",
                Cast = "Elenco variado",
                Director = "Diretor talentoso",
                Genre = genre,
                ReleaseDate = "2020-01-01",
                LastModified = now,
                Rating = rating.ToString("F1"),
                Rating5Based = rating / 2,
                BackdropPath = [$"{_serverUrl}/backdrops/series_{3000 + i}.jpg"],
                EpisodeRunTime = "45",
                CategoryId = categoryId
            });
        }

        return series;
    }

    private Dictionary<int, VodInfo> GenerateVodInfos()
    {
        var infos = new Dictionary<int, VodInfo>();
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        foreach (var vod in _vods)
        {
            infos[vod.StreamId] = new VodInfo
            {
                Info = new VodDetails
                {
                    MovieImage = vod.StreamIcon,
                    TmdbId = (vod.StreamId * 10).ToString(),
                    BackdropPath = [$"{_serverUrl}/backdrops/vod_{vod.StreamId}.jpg"],
                    YoutubeTrailer = "dQw4w9WgXcQ",
                    Genre = GetGenreForCategory(vod.CategoryId),
                    Plot = $"Uma história emocionante em {vod.Name}. Este filme vai te prender do início ao fim.",
                    Cast = "Ator Principal, Atriz Principal, Coadjuvante Famoso",
                    Rating = vod.Rating,
                    Director = "Diretor Famoso",
                    ReleaseDate = "2023-06-15",
                    DurationSecs = 7200,
                    Duration = "02:00:00",
                    Bitrate = 5000,
                    Video = new VideoInfo
                    {
                        CodecName = "h264",
                        Width = 1920,
                        Height = 1080
                    },
                    Audio = new AudioInfo
                    {
                        CodecName = "aac",
                        Channels = 6,
                        SampleRate = "48000"
                    }
                },
                MovieData = new MovieData
                {
                    StreamId = vod.StreamId,
                    Name = vod.Name,
                    Added = now,
                    CategoryId = vod.CategoryId,
                    ContainerExtension = vod.ContainerExtension,
                    DirectSource = string.Empty
                }
            };
        }

        return infos;
    }

    private Dictionary<int, SeriesInfo> GenerateSeriesInfos()
    {
        var infos = new Dictionary<int, SeriesInfo>();
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        foreach (var series in _series)
        {
            var seasons = new List<SeasonInfo>();
            var episodes = new Dictionary<string, List<Episode>>();
            var seasonCount = Random.Shared.Next(2, 6);

            for (var s = 1; s <= seasonCount; s++)
            {
                var episodeCount = Random.Shared.Next(8, 13);
                seasons.Add(new SeasonInfo
                {
                    SeasonNumber = s,
                    AirDate = $"{2019 + s}-01-15",
                    Name = $"Temporada {s}",
                    Overview = $"A {s}ª temporada de {series.Name}.",
                    EpisodeCount = episodeCount,
                    Cover = $"{_serverUrl}/posters/series_{series.SeriesId}_s{s}.jpg",
                    CoverBig = $"{_serverUrl}/posters/series_{series.SeriesId}_s{s}_big.jpg"
                });

                var episodeList = new List<Episode>();
                for (var e = 1; e <= episodeCount; e++)
                {
                    episodeList.Add(new Episode
                    {
                        Id = $"{series.SeriesId}{s:D2}{e:D2}",
                        EpisodeNum = e,
                        Title = $"Episódio {e}",
                        ContainerExtension = "mp4",
                        Info = new EpisodeInfo
                        {
                            MovieImage = $"{_serverUrl}/episodes/series_{series.SeriesId}_s{s}e{e}.jpg",
                            Plot = $"No episódio {e} da temporada {s}, acontecimentos surpreendentes.",
                            ReleaseDate = $"{2019 + s}-{(e % 12) + 1:D2}-15",
                            Rating = 8.0 + Random.Shared.NextDouble(),
                            DurationSecs = 2700,
                            Duration = "00:45:00",
                            Bitrate = 4500,
                            Video = new VideoInfo { CodecName = "h264", Width = 1920, Height = 1080 },
                            Audio = new AudioInfo { CodecName = "aac", Channels = 6, SampleRate = "48000" }
                        },
                        Added = now,
                        Season = s,
                        DirectSource = string.Empty
                    });
                }
                episodes[s.ToString()] = episodeList;
            }

            infos[series.SeriesId] = new SeriesInfo
            {
                Seasons = seasons,
                Info = new SeriesDetails
                {
                    Name = series.Name,
                    Cover = series.Cover,
                    Plot = series.Plot,
                    Cast = series.Cast,
                    Director = series.Director,
                    Genre = series.Genre,
                    ReleaseDate = series.ReleaseDate,
                    LastModified = series.LastModified,
                    Rating = series.Rating,
                    Rating5Based = series.Rating5Based,
                    BackdropPath = series.BackdropPath,
                    YoutubeTrailer = "dQw4w9WgXcQ",
                    EpisodeRunTime = series.EpisodeRunTime,
                    CategoryId = series.CategoryId
                },
                Episodes = episodes
            };
        }

        return infos;
    }

    private static List<EpgEntry> GenerateEpgForChannel(Channel channel, int hoursRange)
    {
        var entries = new List<EpgEntry>();
        var now = DateTimeOffset.UtcNow;
        var startTime = now.AddHours(-2);

        var programs = new[]
        {
            "Jornal da Manhã", "Programa de Variedades", "Filme da Tarde",
            "Notícias", "Talk Show", "Série Especial", "Documentário",
            "Programa Esportivo", "Magazine", "Filme da Noite",
            "Late Show", "Reprise"
        };

        var currentTime = startTime;
        var endTime = now.AddHours(hoursRange);
        var programIndex = 0;

        while (currentTime < endTime)
        {
            var duration = TimeSpan.FromMinutes(Random.Shared.Next(30, 120));
            var programEnd = currentTime.Add(duration);
            var isNowPlaying = now >= currentTime && now < programEnd ? 1 : 0;

            entries.Add(new EpgEntry
            {
                Id = $"{channel.StreamId}_{currentTime.ToUnixTimeSeconds()}",
                EpgId = channel.EpgChannelId ?? channel.StreamId.ToString(),
                Title = programs[programIndex % programs.Length],
                Lang = "pt",
                Start = currentTime.ToString("yyyy-MM-dd HH:mm:ss"),
                End = programEnd.ToString("yyyy-MM-dd HH:mm:ss"),
                Description = $"Descrição do programa {programs[programIndex % programs.Length]}.",
                ChannelId = channel.EpgChannelId ?? channel.StreamId.ToString(),
                StartTimestamp = currentTime.ToUnixTimeSeconds(),
                StopTimestamp = programEnd.ToUnixTimeSeconds(),
                NowPlaying = isNowPlaying,
                HasArchive = channel.TvArchive
            });

            currentTime = programEnd;
            programIndex++;
        }

        return entries;
    }

    private static string GetGenreForCategory(string categoryId) => categoryId switch
    {
        "101" => "Ação",
        "102" => "Comédia",
        "103" => "Drama",
        "104" => "Terror",
        "105" => "Ficção Científica",
        "106" => "Romance",
        _ => "Outros"
    };

    /// <summary>
    /// Converte um Channel para ChannelResponse com URL de proxy.
    /// </summary>
    private ChannelResponse ToChannelResponse(Channel channel, string username, string password)
    {
        var proxyUrl = $"{_serverUrl}/{username}/{password}/{channel.StreamId}";
        // Detectar extensão pela URL original
        if (!string.IsNullOrEmpty(channel.DirectSource) && channel.DirectSource.Contains(".m3u8", StringComparison.OrdinalIgnoreCase))
        {
            proxyUrl += ".m3u8";
        }
        else
        {
            proxyUrl += ".ts";
        }

        return new ChannelResponse(
            Num: channel.Num,
            Name: channel.Name,
            StreamType: channel.StreamType,
            StreamId: channel.StreamId,
            StreamIcon: channel.StreamIcon,
            EpgChannelId: channel.EpgChannelId,
            Added: channel.Added,
            CategoryId: channel.CategoryId,
            CustomSid: null,
            TvArchive: channel.TvArchive,
            DirectSource: proxyUrl,
            TvArchiveDuration: channel.TvArchiveDuration
        );
    }

    /// <summary>
    /// Converte um Vod para VodResponse com URL de proxy.
    /// </summary>
    private VodResponse ToVodResponse(Vod vod, string username, string password)
    {
        var proxyUrl = $"{_serverUrl}/movie/{username}/{password}/{vod.StreamId}.{vod.ContainerExtension}";

        return new VodResponse(
            Num: vod.Num,
            Name: vod.Name,
            StreamType: vod.StreamType,
            StreamId: vod.StreamId,
            StreamIcon: vod.StreamIcon,
            Rating: vod.Rating,
            Rating5Based: vod.Rating5Based,
            Added: vod.Added,
            CategoryId: vod.CategoryId,
            ContainerExtension: vod.ContainerExtension,
            CustomSid: null,
            DirectSource: proxyUrl
        );
    }

    /// <summary>
    /// Converte uma Series para SeriesResponse.
    /// </summary>
    private SeriesResponse ToSeriesResponse(Series series, string username, string password)
    {
        return new SeriesResponse(
            Num: series.Num,
            Name: series.Name,
            SeriesId: series.SeriesId,
            Cover: series.Cover,
            Plot: series.Plot,
            Cast: series.Cast,
            Director: series.Director,
            Genre: series.Genre,
            ReleaseDate: series.ReleaseDate,
            LastModified: series.LastModified,
            Rating: series.Rating,
            Rating5Based: series.Rating5Based,
            BackdropPath: series.BackdropPath,
            EpisodeRunTime: series.EpisodeRunTime,
            CategoryId: series.CategoryId
        );
    }

    /// <summary>
    /// Converte uma SeriesInfo para SeriesInfoResponse com URLs de proxy para episódios.
    /// </summary>
    private SeriesInfoResponse ConvertToSeriesInfoResponse(SeriesInfo seriesInfo, string username, string password)
    {
        var seasons = seriesInfo.Seasons
            .Select(s => new SeasonInfoResponse(
                SeasonNumber: s.SeasonNumber,
                AirDate: s.AirDate,
                Name: s.Name,
                Overview: s.Overview,
                EpisodeCount: s.EpisodeCount,
                Cover: s.Cover,
                CoverBig: s.CoverBig
            ))
            .ToList();

        var episodes = new Dictionary<string, List<EpisodeResponse>>();
        foreach (var seasonEntry in seriesInfo.Episodes)
        {
            var seasonNumber = seasonEntry.Key;
            var episodeList = seasonEntry.Value
                .Select(e => ConvertToEpisodeResponse(e, username, password))
                .ToList();
            episodes[seasonNumber] = episodeList;
        }

        var info = new SeriesDetailsResponse(
            Name: seriesInfo.Info.Name,
            Cover: seriesInfo.Info.Cover,
            Plot: seriesInfo.Info.Plot,
            Cast: seriesInfo.Info.Cast,
            Director: seriesInfo.Info.Director,
            Genre: seriesInfo.Info.Genre,
            ReleaseDate: seriesInfo.Info.ReleaseDate,
            LastModified: seriesInfo.Info.LastModified,
            Rating: seriesInfo.Info.Rating,
            Rating5Based: seriesInfo.Info.Rating5Based,
            BackdropPath: seriesInfo.Info.BackdropPath,
            YoutubeTrailer: seriesInfo.Info.YoutubeTrailer,
            EpisodeRunTime: seriesInfo.Info.EpisodeRunTime,
            CategoryId: seriesInfo.Info.CategoryId
        );

        return new SeriesInfoResponse(
            Seasons: seasons,
            Info: info,
            Episodes: episodes
        );
    }

    /// <summary>
    /// Converte um Episode para EpisodeResponse com URL de proxy.
    /// </summary>
    private EpisodeResponse ConvertToEpisodeResponse(Episode episode, string username, string password)
    {
        var proxyUrl = $"{_serverUrl}/series/{username}/{password}/{episode.Id}.{episode.ContainerExtension}";

        var videoInfo = new VideoInfoResponse(
            CodecName: episode.Info.Video.CodecName,
            Width: episode.Info.Video.Width,
            Height: episode.Info.Video.Height
        );

        var audioInfo = new AudioInfoResponse(
            CodecName: episode.Info.Audio.CodecName,
            Channels: episode.Info.Audio.Channels,
            SampleRate: episode.Info.Audio.SampleRate
        );

        var episodeInfo = new EpisodeInfoResponse(
            MovieImage: episode.Info.MovieImage,
            Plot: episode.Info.Plot,
            ReleaseDate: episode.Info.ReleaseDate,
            Rating: episode.Info.Rating,
            DurationSecs: episode.Info.DurationSecs,
            Duration: episode.Info.Duration,
            Bitrate: episode.Info.Bitrate,
            Video: videoInfo,
            Audio: audioInfo
        );

        return new EpisodeResponse(
            Id: episode.Id,
            EpisodeNum: episode.EpisodeNum,
            Title: episode.Title,
            ContainerExtension: episode.ContainerExtension,
            Info: episodeInfo,
            CustomSid: episode.CustomSid,
            Added: episode.Added,
            Season: episode.Season,
            DirectSource: proxyUrl
        );
    }

    private Dictionary<int, VodInfo> GenerateVodInfosFromPlaylist()
    {
        var infos = new Dictionary<int, VodInfo>();
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        foreach (var vod in _vods)
        {
            var category = _vodCategories.FirstOrDefault(c => c.CategoryId == vod.CategoryId);

            infos[vod.StreamId] = new VodInfo
            {
                Info = new VodDetails
                {
                    MovieImage = vod.StreamIcon,
                    Genre = category?.CategoryName ?? "Outros",
                    Plot = string.Empty,
                    Cast = string.Empty,
                    Rating = vod.Rating,
                    Director = string.Empty,
                    ReleaseDate = string.Empty,
                    DurationSecs = 0,
                    Duration = string.Empty,
                    Bitrate = 0,
                    Video = new VideoInfo { CodecName = "h264", Width = 1920, Height = 1080 },
                    Audio = new AudioInfo { CodecName = "aac", Channels = 2, SampleRate = "44100" }
                },
                MovieData = new MovieData
                {
                    StreamId = vod.StreamId,
                    Name = vod.Name,
                    Added = now,
                    CategoryId = vod.CategoryId,
                    ContainerExtension = vod.ContainerExtension,
                    DirectSource = vod.DirectSource
                }
            };
        }

        return infos;
    }

    private Dictionary<int, SeriesInfo> GenerateSeriesInfosFromPlaylist()
    {
        var infos = new Dictionary<int, SeriesInfo>();
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        foreach (var series in _series)
        {
            // Para séries vindas de M3U, criamos uma estrutura básica com 1 temporada e 1 episódio
            var seasons = new List<SeasonInfo>
            {
                new()
                {
                    SeasonNumber = 1,
                    AirDate = string.Empty,
                    Name = "Temporada 1",
                    Overview = string.Empty,
                    EpisodeCount = 1,
                    Cover = series.Cover,
                    CoverBig = series.Cover
                }
            };

            var episodes = new Dictionary<string, List<Episode>>
            {
                ["1"] =
                [
                    new()
                    {
                        Id = series.SeriesId.ToString(),
                        EpisodeNum = 1,
                        Title = series.Name,
                        ContainerExtension = "mp4",
                        Info = new EpisodeInfo
                        {
                            MovieImage = series.Cover,
                            Plot = series.Plot,
                            ReleaseDate = series.ReleaseDate,
                            Rating = double.TryParse(series.Rating, out var r) ? r : 0,
                            DurationSecs = 0,
                            Duration = string.Empty,
                            Bitrate = 0,
                            Video = new VideoInfo { CodecName = "h264", Width = 1920, Height = 1080 },
                            Audio = new AudioInfo { CodecName = "aac", Channels = 2, SampleRate = "44100" }
                        },
                        Added = now,
                        Season = 1,
                        DirectSource = _playlistService?.GetStreamUrl(series.SeriesId) ?? string.Empty
                    }
                ]
            };

            infos[series.SeriesId] = new SeriesInfo
            {
                Seasons = seasons,
                Info = new SeriesDetails
                {
                    Name = series.Name,
                    Cover = series.Cover,
                    Plot = series.Plot,
                    Cast = series.Cast,
                    Director = series.Director,
                    Genre = series.Genre,
                    ReleaseDate = series.ReleaseDate,
                    LastModified = series.LastModified,
                    Rating = series.Rating,
                    Rating5Based = series.Rating5Based,
                    BackdropPath = series.BackdropPath,
                    EpisodeRunTime = series.EpisodeRunTime,
                    CategoryId = series.CategoryId
                },
                Episodes = episodes
            };
        }

        return infos;
    }
}
