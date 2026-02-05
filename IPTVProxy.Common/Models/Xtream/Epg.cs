namespace IPTVProxy.Common.Models.Xtream;

using System.Text.Json.Serialization;

/// <summary>
/// Entrada de EPG (Guia de Programação Eletrônica).
/// </summary>
public record EpgEntry
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("epg_id")]
    public string EpgId { get; init; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("lang")]
    public string Lang { get; init; } = "pt";

    [JsonPropertyName("start")]
    public string Start { get; init; } = string.Empty;

    [JsonPropertyName("end")]
    public string End { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("channel_id")]
    public string ChannelId { get; init; } = string.Empty;

    [JsonPropertyName("start_timestamp")]
    public long StartTimestamp { get; init; }

    [JsonPropertyName("stop_timestamp")]
    public long StopTimestamp { get; init; }

    [JsonPropertyName("now_playing")]
    public int NowPlaying { get; init; }

    [JsonPropertyName("has_archive")]
    public int HasArchive { get; init; }
}

/// <summary>
/// Resposta de EPG por canal.
/// </summary>
public record EpgResponse
{
    [JsonPropertyName("epg_listings")]
    public List<EpgEntry> EpgListings { get; init; } = [];
}
