namespace IPTVProxy.Common.Models.Xtream;

using System.Text.Json.Serialization;

/// <summary>
/// VOD (Filme).
/// </summary>
public record Vod
{
    [JsonPropertyName("num")]
    public int Num { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("stream_type")]
    public string StreamType { get; init; } = "movie";

    [JsonPropertyName("stream_id")]
    public int StreamId { get; init; }

    [JsonPropertyName("stream_icon")]
    public string StreamIcon { get; init; } = string.Empty;

    [JsonPropertyName("rating")]
    public string Rating { get; init; } = string.Empty;

    [JsonPropertyName("rating_5based")]
    public double Rating5Based { get; init; }

    [JsonPropertyName("added")]
    public string Added { get; init; } = string.Empty;

    [JsonPropertyName("category_id")]
    public string CategoryId { get; init; } = string.Empty;

    [JsonPropertyName("container_extension")]
    public string ContainerExtension { get; init; } = "mp4";

    [JsonPropertyName("custom_sid")]
    public string? CustomSid { get; init; }

    [JsonPropertyName("direct_source")]
    public string DirectSource { get; init; } = string.Empty;
}

/// <summary>
/// Informações detalhadas de um VOD (Filme).
/// </summary>
public record VodInfo
{
    [JsonPropertyName("info")]
    public VodDetails Info { get; init; } = new();

    [JsonPropertyName("movie_data")]
    public MovieData MovieData { get; init; } = new();
}

public record VodDetails
{
    [JsonPropertyName("movie_image")]
    public string MovieImage { get; init; } = string.Empty;

    [JsonPropertyName("tmdb_id")]
    public string? TmdbId { get; init; }

    [JsonPropertyName("backdrop_path")]
    public List<string> BackdropPath { get; init; } = [];

    [JsonPropertyName("youtube_trailer")]
    public string? YoutubeTrailer { get; init; }

    [JsonPropertyName("genre")]
    public string Genre { get; init; } = string.Empty;

    [JsonPropertyName("plot")]
    public string Plot { get; init; } = string.Empty;

    [JsonPropertyName("cast")]
    public string Cast { get; init; } = string.Empty;

    [JsonPropertyName("rating")]
    public string Rating { get; init; } = string.Empty;

    [JsonPropertyName("director")]
    public string Director { get; init; } = string.Empty;

    [JsonPropertyName("releasedate")]
    public string ReleaseDate { get; init; } = string.Empty;

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

public record MovieData
{
    [JsonPropertyName("stream_id")]
    public int StreamId { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("added")]
    public string Added { get; init; } = string.Empty;

    [JsonPropertyName("category_id")]
    public string CategoryId { get; init; } = string.Empty;

    [JsonPropertyName("container_extension")]
    public string ContainerExtension { get; init; } = "mp4";

    [JsonPropertyName("custom_sid")]
    public string? CustomSid { get; init; }

    [JsonPropertyName("direct_source")]
    public string DirectSource { get; init; } = string.Empty;
}

public record VideoInfo
{
    [JsonPropertyName("codec_name")]
    public string CodecName { get; init; } = "h264";

    [JsonPropertyName("width")]
    public int Width { get; init; }

    [JsonPropertyName("height")]
    public int Height { get; init; }
}

public record AudioInfo
{
    [JsonPropertyName("codec_name")]
    public string CodecName { get; init; } = "aac";

    [JsonPropertyName("channels")]
    public int Channels { get; init; } = 2;

    [JsonPropertyName("sample_rate")]
    public string SampleRate { get; init; } = "44100";
}
