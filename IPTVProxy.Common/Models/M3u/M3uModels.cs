namespace IPTVProxy.Common.Models.M3u;

/// <summary>
/// Representa uma entrada parseada de um arquivo M3U.
/// </summary>
public record M3uEntry
{
    /// <summary>
    /// Duração do stream (-1 para live).
    /// </summary>
    public int Duration { get; init; } = -1;

    /// <summary>
    /// ID do canal para EPG (tvg-id).
    /// </summary>
    public string? TvgId { get; init; }

    /// <summary>
    /// Nome do canal para EPG (tvg-name).
    /// </summary>
    public string? TvgName { get; init; }

    /// <summary>
    /// URL do logo (tvg-logo).
    /// </summary>
    public string? TvgLogo { get; init; }

    /// <summary>
    /// Grupo/categoria (group-title).
    /// </summary>
    public string? GroupTitle { get; init; }

    /// <summary>
    /// Nome de exibição do canal.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// URL do stream.
    /// </summary>
    public string Url { get; init; } = string.Empty;

    /// <summary>
    /// HTTP Referrer opcional.
    /// </summary>
    public string? HttpReferrer { get; init; }

    /// <summary>
    /// HTTP User-Agent opcional.
    /// </summary>
    public string? HttpUserAgent { get; init; }

    /// <summary>
    /// ID do canal para catchup/timeshift (tvg-chno).
    /// </summary>
    public string? TvgChno { get; init; }

    /// <summary>
    /// Tipo do stream (live, movie, series).
    /// </summary>
    public M3uStreamType StreamType { get; init; } = M3uStreamType.Live;

    /// <summary>
    /// ID numérico gerado para o stream.
    /// </summary>
    public int StreamId { get; init; }
}

/// <summary>
/// Tipo do stream M3U.
/// </summary>
public enum M3uStreamType
{
    Live,
    Movie,
    Series
}

/// <summary>
/// Resultado do parsing de um arquivo M3U.
/// </summary>
public record M3uPlaylist
{
    /// <summary>
    /// Lista de entradas do M3U.
    /// </summary>
    public List<M3uEntry> Entries { get; init; } = [];

    /// <summary>
    /// Categorias únicas encontradas.
    /// </summary>
    public List<string> Categories { get; init; } = [];

    /// <summary>
    /// Atributos globais do M3U (x-tvg-url, etc.).
    /// </summary>
    public Dictionary<string, string> GlobalAttributes { get; init; } = [];
}
