namespace IPTVProxy.Common.Services;

using IPTVProxy.Common.Models.M3u;
using System.Text.RegularExpressions;

/// <summary>
/// Parser para arquivos M3U/M3U8.
/// Suporta formatos padrão e extended M3U com atributos tvg-*.
/// </summary>
public static partial class M3uParser
{
    /// <summary>
    /// Faz o parse de um arquivo M3U a partir do caminho.
    /// </summary>
    public static M3uPlaylist ParseFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"M3U file not found: {filePath}");
        }

        var content = File.ReadAllText(filePath);
        return Parse(content);
    }

    /// <summary>
    /// Faz o parse de um arquivo M3U a partir do caminho (async).
    /// </summary>
    public static async Task<M3uPlaylist> ParseFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"M3U file not found: {filePath}");
        }

        var content = await File.ReadAllTextAsync(filePath, cancellationToken);
        return Parse(content);
    }

    /// <summary>
    /// Faz o parse do conteúdo M3U.
    /// </summary>
    public static M3uPlaylist Parse(string content)
    {
        ArgumentNullException.ThrowIfNull(content);

        var entries = new List<M3uEntry>();
        var categories = new HashSet<string>();
        var globalAttributes = new Dictionary<string, string>();

        var lines = content.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);

        if (lines.Length == 0 || !lines[0].StartsWith("#EXTM3U"))
        {
            throw new FormatException("Invalid M3U file: missing #EXTM3U header");
        }

        // Parse global attributes from #EXTM3U line
        var headerMatch = ExtM3uHeaderRegex().Match(lines[0]);
        if (headerMatch.Success)
        {
            var attrs = ParseAttributes(lines[0][7..]); // Skip "#EXTM3U"
            foreach (var attr in attrs)
            {
                globalAttributes[attr.Key] = attr.Value;
            }
        }

        var streamId = 1;
        for (var i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();

            if (string.IsNullOrEmpty(line) || line.StartsWith('#') && !line.StartsWith("#EXTINF"))
            {
                continue;
            }

            if (line.StartsWith("#EXTINF"))
            {
                var entry = ParseExtInf(line, streamId);

                // Próxima linha não vazia/comentário deve ser a URL
                for (var j = i + 1; j < lines.Length; j++)
                {
                    var urlLine = lines[j].Trim();
                    if (string.IsNullOrEmpty(urlLine))
                    {
                        continue;
                    }

                    if (urlLine.StartsWith('#'))
                    {
                        // Pode ser #EXTVLCOPT ou similar, pular
                        if (urlLine.StartsWith("#EXTVLCOPT") || urlLine.StartsWith("#EXTGRP"))
                        {
                            continue;
                        }
                        break;
                    }

                    entry = entry with { Url = urlLine, StreamId = streamId };
                    i = j;
                    break;
                }

                if (!string.IsNullOrEmpty(entry.Url))
                {
                    entries.Add(entry);
                    streamId++;

                    if (!string.IsNullOrEmpty(entry.GroupTitle))
                    {
                        categories.Add(entry.GroupTitle);
                    }
                }
            }
        }

        return new M3uPlaylist
        {
            Entries = entries,
            Categories = [.. categories.OrderBy(c => c)],
            GlobalAttributes = globalAttributes
        };
    }

    private static M3uEntry ParseExtInf(string line, int streamId)
    {
        // Format: #EXTINF:-1 tvg-id="..." tvg-logo="..." group-title="...",Channel Name
        // or: #EXTINF:-1,Channel Name

        var duration = -1;
        string? tvgId = null;
        string? tvgName = null;
        string? tvgLogo = null;
        string? groupTitle = null;
        string? httpReferrer = null;
        string? httpUserAgent = null;
        string? tvgChno = null;
        var name = string.Empty;

        // Extract duration
        var durationMatch = DurationRegex().Match(line);
        if (durationMatch.Success)
        {
            int.TryParse(durationMatch.Groups[1].Value, out duration);
        }

        // Extract attributes
        var attributes = ParseAttributes(line);

        tvgId = attributes.GetValueOrDefault("tvg-id");
        tvgName = attributes.GetValueOrDefault("tvg-name");
        tvgLogo = attributes.GetValueOrDefault("tvg-logo");
        groupTitle = attributes.GetValueOrDefault("group-title");
        httpReferrer = attributes.GetValueOrDefault(":http-referrer") ?? attributes.GetValueOrDefault("http-referrer");
        httpUserAgent = attributes.GetValueOrDefault(":http-user-agent") ?? attributes.GetValueOrDefault("http-user-agent");
        tvgChno = attributes.GetValueOrDefault("tvg-chno");

        // Extract name (after the last comma)
        var lastCommaIndex = line.LastIndexOf(',');
        if (lastCommaIndex >= 0 && lastCommaIndex < line.Length - 1)
        {
            name = line[(lastCommaIndex + 1)..].Trim();
        }

        // Fallback: use tvg-name if name is empty
        if (string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(tvgName))
        {
            name = tvgName;
        }

        // Determine stream type based on group or URL patterns
        var streamType = DetermineStreamType(groupTitle, name);

        return new M3uEntry
        {
            Duration = duration,
            TvgId = CleanAttributeValue(tvgId),
            TvgName = CleanAttributeValue(tvgName),
            TvgLogo = CleanAttributeValue(tvgLogo),
            GroupTitle = CleanAttributeValue(groupTitle),
            Name = name,
            HttpReferrer = CleanAttributeValue(httpReferrer),
            HttpUserAgent = CleanAttributeValue(httpUserAgent),
            TvgChno = CleanAttributeValue(tvgChno),
            StreamType = streamType,
            StreamId = streamId
        };
    }

    private static Dictionary<string, string> ParseAttributes(string line)
    {
        var attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Match patterns like: key="value" or :key="value"
        var matches = AttributeRegex().Matches(line);
        foreach (Match match in matches)
        {
            var key = match.Groups[1].Value.Trim();
            var value = match.Groups[2].Value;
            attributes[key] = value;
        }

        return attributes;
    }

    private static string? CleanAttributeValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        // Remove surrounding quotes if present
        value = value.Trim();
        if (value.StartsWith('"') && value.EndsWith('"'))
        {
            value = value[1..^1];
        }

        return string.IsNullOrEmpty(value) ? null : value;
    }

    private static M3uStreamType DetermineStreamType(string? groupTitle, string? name)
    {
        var searchText = $"{groupTitle} {name}".ToLowerInvariant();

        if (searchText.Contains("movie") || searchText.Contains("filme") ||
            searchText.Contains("vod") || searchText.Contains("cinema"))
        {
            return M3uStreamType.Movie;
        }

        if (searchText.Contains("series") || searchText.Contains("série") ||
            searchText.Contains("serie") || searchText.Contains("episode") ||
            searchText.Contains("temporada") || searchText.Contains("season"))
        {
            return M3uStreamType.Series;
        }

        return M3uStreamType.Live;
    }

    [GeneratedRegex(@"#EXTM3U(.*)")]
    private static partial Regex ExtM3uHeaderRegex();

    [GeneratedRegex(@"#EXTINF:\s*(-?\d+)")]
    private static partial Regex DurationRegex();

    [GeneratedRegex(@"([\w\-:]+)=""([^""]*)""")]
    private static partial Regex AttributeRegex();
}
