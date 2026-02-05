namespace IPTVProxy.Common.Models.Xtream;

using System.Text.Json.Serialization;

/// <summary>
/// Categoria genérica para canais, VODs e séries.
/// </summary>
public record Category
{
    [JsonPropertyName("category_id")]
    public string CategoryId { get; init; } = string.Empty;

    [JsonPropertyName("category_name")]
    public string CategoryName { get; init; } = string.Empty;

    [JsonPropertyName("parent_id")]
    public int ParentId { get; init; }
}
