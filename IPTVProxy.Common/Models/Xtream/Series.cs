namespace IPTVProxy.Common.Models.Xtream;

using System.Text.Json.Serialization;

/// <summary>
/// Série.
/// </summary>
public record Series
{
    [JsonPropertyName("num")]
    public int Num { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("series_id")]
    public int SeriesId { get; init; }

    [JsonPropertyName("cover")]
    public string Cover { get; init; } = string.Empty;

    [JsonPropertyName("plot")]
    public string Plot { get; init; } = string.Empty;

    [JsonPropertyName("cast")]
    public string Cast { get; init; } = string.Empty;

    [JsonPropertyName("director")]
    public string Director { get; init; } = string.Empty;

    [JsonPropertyName("genre")]
    public string Genre { get; init; } = string.Empty;

    [JsonPropertyName("releaseDate")]
    public string ReleaseDate { get; init; } = string.Empty;

    [JsonPropertyName("last_modified")]
    public string LastModified { get; init; } = string.Empty;

    [JsonPropertyName("rating")]
    public string Rating { get; init; } = string.Empty;

    [JsonPropertyName("rating_5based")]
    public double Rating5Based { get; init; }

    [JsonPropertyName("backdrop_path")]
    public List<string> BackdropPath { get; init; } = [];

    [JsonPropertyName("youtube_trailer")]
    public string? YoutubeTrailer { get; init; }

    [JsonPropertyName("episode_run_time")]
    public string EpisodeRunTime { get; init; } = string.Empty;

    [JsonPropertyName("category_id")]
    public string CategoryId { get; init; } = string.Empty;
}

/// <summary>
/// Informações detalhadas de uma série.
/// </summary>
public record SeriesInfo
{
    [JsonPropertyName("seasons")]
    public List<SeasonInfo> Seasons { get; init; } = [];

    [JsonPropertyName("info")]
    public SeriesDetails Info { get; init; } = new();

    [JsonPropertyName("episodes")]
    public Dictionary<string, List<Episode>> Episodes { get; init; } = [];
}

public record SeasonInfo
{
    [JsonPropertyName("season_number")]
    public int SeasonNumber { get; init; }

    [JsonPropertyName("air_date")]
    public string AirDate { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("overview")]
    public string Overview { get; init; } = string.Empty;

    [JsonPropertyName("episode_count")]
    public int EpisodeCount { get; init; }

    [JsonPropertyName("cover")]
    public string Cover { get; init; } = string.Empty;

    [JsonPropertyName("cover_big")]
    public string CoverBig { get; init; } = string.Empty;
}

public record SeriesDetails
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("cover")]
    public string Cover { get; init; } = string.Empty;

    [JsonPropertyName("plot")]
    public string Plot { get; init; } = string.Empty;

    [JsonPropertyName("cast")]
    public string Cast { get; init; } = string.Empty;

    [JsonPropertyName("director")]
    public string Director { get; init; } = string.Empty;

    [JsonPropertyName("genre")]
    public string Genre { get; init; } = string.Empty;

    [JsonPropertyName("releaseDate")]
    public string ReleaseDate { get; init; } = string.Empty;

    [JsonPropertyName("last_modified")]
    public string LastModified { get; init; } = string.Empty;

    [JsonPropertyName("rating")]
    public string Rating { get; init; } = string.Empty;

    [JsonPropertyName("rating_5based")]
    public double Rating5Based { get; init; }

    [JsonPropertyName("backdrop_path")]
    public List<string> BackdropPath { get; init; } = [];

    [JsonPropertyName("youtube_trailer")]
    public string? YoutubeTrailer { get; init; }

    [JsonPropertyName("episode_run_time")]
    public string EpisodeRunTime { get; init; } = string.Empty;

    [JsonPropertyName("category_id")]
    public string CategoryId { get; init; } = string.Empty;
}

/// <summary>
/// Episódio de uma série.
/// </summary>
public record Episode
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("episode_num")]
    public int EpisodeNum { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("container_extension")]
    public string ContainerExtension { get; init; } = "mp4";

    [JsonPropertyName("info")]
    public EpisodeInfo Info { get; init; } = new();

    [JsonPropertyName("custom_sid")]
    public string? CustomSid { get; init; }

    [JsonPropertyName("added")]
    public string Added { get; init; } = string.Empty;

    [JsonPropertyName("season")]
    public int Season { get; init; }

    [JsonPropertyName("direct_source")]
    public string DirectSource { get; init; } = string.Empty;
}

public record EpisodeInfo
{
    [JsonPropertyName("movie_image")]
    public string MovieImage { get; init; } = string.Empty;

    [JsonPropertyName("plot")]
    public string Plot { get; init; } = string.Empty;

    [JsonPropertyName("releasedate")]
    public string ReleaseDate { get; init; } = string.Empty;

    [JsonPropertyName("rating")]
    public double Rating { get; init; }

    [JsonPropertyName("duration_secs")]
    public int DurationSecs { get; init; }

    [JsonPropertyName("duration")]
    public string Duration { get; init; } = string.Empty;

    [JsonPropertyName("bitrate")]
    public int Bitrate { get; init; }

    [JsonPropertyName("video")]
    public VideoInfo Video { get; init; } = new();

    [JsonPropertyName("audio")]
    public AudioInfo Audio { get; init; } = new();
}
