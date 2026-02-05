namespace IPTVProxy.Common.Models.Xtream;

using System.Text.Json.Serialization;

/// <summary>
/// Canal de TV ao vivo.
/// </summary>
public record Channel
{
    [JsonPropertyName("num")]
    public int Num { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("stream_type")]
    public string StreamType { get; init; } = "live";

    [JsonPropertyName("stream_id")]
    public int StreamId { get; init; }

    [JsonPropertyName("stream_icon")]
    public string StreamIcon { get; init; } = string.Empty;

    [JsonPropertyName("epg_channel_id")]
    public string? EpgChannelId { get; init; }

    [JsonPropertyName("added")]
    public string Added { get; init; } = string.Empty;

    [JsonPropertyName("category_id")]
    public string CategoryId { get; init; } = string.Empty;

    [JsonPropertyName("custom_sid")]
    public string? CustomSid { get; init; }

    [JsonPropertyName("tv_archive")]
    public int TvArchive { get; init; }

    [JsonPropertyName("direct_source")]
    public string DirectSource { get; init; } = string.Empty;

    [JsonPropertyName("tv_archive_duration")]
    public int TvArchiveDuration { get; init; }
}
