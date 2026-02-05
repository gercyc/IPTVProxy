namespace IPTVProxy.Common.Api;

using IPTVProxy.Common.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Mapeia os endpoints da XtreamAPI simulada usando Minimal APIs.
/// Alternativa flexível para o XtreamApiController baseado em Controllers.
/// Documentação: https://github.com/tellytv/go.xtream-codes/wiki/Xtream-API
/// </summary>
public static class XtreamApiEndpoints
{
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    /// <summary>
    /// Registra todos os endpoints da XtreamAPI como Minimal APIs.
    /// </summary>
    public static IEndpointRouteBuilder MapXtreamApi(
        this IEndpointRouteBuilder endpoints,
        string baseRoute = "/api/xtreamapi")
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        return MapEndpoints(endpoints, baseRoute);
    }

    private static IEndpointRouteBuilder MapEndpoints(
        IEndpointRouteBuilder endpoints,
        string baseRoute)
    {
        var group = endpoints.MapGroup(baseRoute);

        // GET M3U Playlist - get.php
        group.MapGet("/get.php", HandleGetPlaylist)
            .WithName("GetM3uPlaylist")
            .WithTags("Playlist")
            .WithSummary("Obter playlist M3U")
            .WithDescription("Gera uma playlist M3U com todos os canais disponíveis. Parâmetros: username, password, type (opcional), output (opcional)");

        // Player API - player_api.php
        group.MapGet("/player_api.php", HandlePlayerApi)
            .WithName("PlayerApi")
            .WithTags("Xtream API")
            .WithSummary("API principal Xtream Codes")
            .WithDescription(@"Endpoint principal da API Xtream Codes com suporte a múltiplas actions:
            - get_live_categories: Lista categorias de canais ao vivo
            - get_live_streams: Lista canais (use category_id para filtrar)
            - get_vod_categories: Lista categorias de VOD
            - get_vod_streams: Lista VODs (use category_id para filtrar)
            - get_vod_info: Informações de VOD específico (use vod_id)
            - get_series_categories: Lista categorias de séries
            - get_series: Lista séries (use category_id para filtrar)
            - get_series_info: Informações de série específica (use series_id)
            - get_short_epg: EPG resumido (use stream_id e limit)
            - get_simple_data_table: EPG completo de um canal (use stream_id)");

        // Full EPG XML - xmltv.php
        group.MapGet("/xmltv.php", HandleGetXmltvEpg)
            .WithName("GetXmltvEpg")
            .WithTags("EPG")
            .WithSummary("Obter EPG em formato XMLTV")
            .WithDescription("Retorna o guia de programação eletrônica completo em formato XMLTV/XML. Parâmetros: username, password");

        // Live stream - /{username}/{password}/{stream_id}
        group.MapGet("/{username}/{password}/{streamId:int}", HandleGetLiveStream)
            .WithName("GetLiveStream")
            .WithTags("Streaming")
            .WithSummary("Stream de canal ao vivo")
            .WithDescription("Faz proxy do stream de um canal ao vivo");

        // Live stream com extensão - /{username}/{password}/{stream_id}.{ext}
        group.MapGet("/{username}/{password}/{streamId:int}.{ext}", HandleGetLiveStreamWithExtension)
            .WithName("GetLiveStreamWithExtension")
            .WithTags("Streaming");

        // Movie stream - /movie/{username}/{password}/{vod_id}.{ext}
        group.MapGet("/movie/{username}/{password}/{vodId:int}.{ext}", HandleGetMovieStream)
            .WithName("GetMovieStream")
            .WithTags("Streaming")
            .WithSummary("Stream de filme")
            .WithDescription("Faz proxy do stream de um filme");

        // Series episode stream - /series/{username}/{password}/{episode_id}.{ext}
        group.MapGet("/series/{username}/{password}/{episodeId}.{ext}", HandleGetSeriesEpisodeStream)
            .WithName("GetSeriesEpisodeStream")
            .WithTags("Streaming")
            .WithSummary("Stream de episódio")
            .WithDescription("Faz proxy do stream de um episódio de série");

        return endpoints;
    }

    private static IResult HandleGetPlaylist(
        XtreamSimulator simulator,
        string username,
        string password,
        string? type = null,
        string? output = null)
    {
        if (!simulator.ValidateCredentials(username, password))
        {
            return Results.Unauthorized();
        }

        var m3uContent = simulator.GenerateM3uPlaylist(username, password, type, output);
        return Results.Content(m3uContent, "application/x-mpegurl");
    }

    private static IResult HandlePlayerApi(
        XtreamSimulator simulator,
        string username,
        string password,
        string? action = null,
        string? category_id = null,
        int? stream_id = null,
        int? vod_id = null,
        int? series_id = null,
        int? limit = null)
    {
        if (!simulator.ValidateCredentials(username, password))
        {
            return Results.Ok(simulator.GetAuthErrorResponse());
        }

        if (string.IsNullOrEmpty(action))
        {
            return Results.Ok(simulator.GetLoginResponse(username, password));
        }

        return action switch
        {
            "get_live_categories" => Results.Ok(simulator.GetChannelCategories()),
            "get_live_streams" => Results.Ok(string.IsNullOrEmpty(category_id)
                ? simulator.GetAllChannelsResponse(username, password)
                : simulator.GetChannelsByCategoryResponse(category_id, username, password)),

            "get_vod_categories" => Results.Ok(simulator.GetVodCategories()),
            "get_vod_streams" => Results.Ok(string.IsNullOrEmpty(category_id)
                ? simulator.GetAllVodsResponse(username, password)
                : simulator.GetVodsByCategoryResponse(category_id, username, password)),
            "get_vod_info" => HandleVodInfo(simulator, vod_id),

            "get_series_categories" => Results.Ok(simulator.GetSeriesCategories()),
            "get_series" => Results.Ok(string.IsNullOrEmpty(category_id)
                ? simulator.GetAllSeriesResponse(username, password)
                : simulator.GetSeriesByCategoryResponse(category_id, username, password)),
            "get_series_info" => HandleSeriesInfo(simulator, series_id, username, password),

            "get_short_epg" => HandleShortEpg(simulator, stream_id, limit),
            "get_simple_data_table" => HandleSimpleDataTable(simulator, stream_id),

            _ => Results.Ok(new { error = "Action not supported" })
        };
    }

    private static IResult HandleGetXmltvEpg(
        XtreamSimulator simulator,
        string username,
        string password)
    {
        if (!simulator.ValidateCredentials(username, password))
        {
            return Results.Unauthorized();
        }

        var xmlContent = simulator.GenerateXmltvEpg();
        return Results.Content(xmlContent, "application/xml");
    }

    private static async Task HandleGetLiveStream(
        XtreamSimulator simulator,
        string username,
        string password,
        int streamId,
        HttpContext context)
    {
        if (!simulator.ValidateCredentials(username, password))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var channel = simulator.GetChannelById(streamId);
        if (channel == null || string.IsNullOrEmpty(channel.DirectSource))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        await ProxyStreamAsync(channel.DirectSource, context);
    }

    private static async Task HandleGetLiveStreamWithExtension(
        XtreamSimulator simulator,
        string username,
        string password,
        int streamId,
        string ext,
        HttpContext context)
    {
        if (!simulator.ValidateCredentials(username, password))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var channel = simulator.GetChannelById(streamId);
        if (channel == null || string.IsNullOrEmpty(channel.DirectSource))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        await ProxyStreamAsync(channel.DirectSource, context, ext);
    }

    private static async Task HandleGetMovieStream(
        XtreamSimulator simulator,
        string username,
        string password,
        int vodId,
        string ext,
        HttpContext context)
    {
        if (!simulator.ValidateCredentials(username, password))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var vod = simulator.GetVodById(vodId);
        if (vod == null || string.IsNullOrEmpty(vod.DirectSource))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        await ProxyStreamAsync(vod.DirectSource, context, ext);
    }

    private static async Task HandleGetSeriesEpisodeStream(
        XtreamSimulator simulator,
        string username,
        string password,
        string episodeId,
        string ext,
        HttpContext context)
    {
        if (!simulator.ValidateCredentials(username, password))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var episode = simulator.GetEpisodeById(episodeId);
        if (episode == null || string.IsNullOrEmpty(episode.DirectSource))
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        await ProxyStreamAsync(episode.DirectSource, context, ext);
    }

    /// <summary>
    /// Faz proxy do stream, baixando da fonte e enviando para o cliente.
    /// Para arquivos M3U8, reescreve URLs relativas para usar a URL da fonte original.
    /// </summary>
    private static async Task ProxyStreamAsync(
        string sourceUrl,
        HttpContext context,
        string? extension = null)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, sourceUrl);

            // Copiar headers relevantes do cliente
            if (context.Request.Headers.TryGetValue("Range", out var rangeHeader))
            {
                request.Headers.TryAddWithoutValidation("Range", rangeHeader.ToString());
            }

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);

            // Definir status code
            context.Response.StatusCode = (int)response.StatusCode;

            // Definir Content-Type baseado na extensão ou resposta original
            var contentType = GetContentType(extension, response.Content.Headers.ContentType?.MediaType);
            context.Response.ContentType = contentType;

            // Extrair nome do arquivo da URL ou gerar um
            var fileName = GetFileNameFromUrl(sourceUrl, extension);
            context.Response.Headers.ContentDisposition = $"attachment; filename=\"{fileName}\"";

            // Copiar headers relevantes da resposta
            if (response.Content.Headers.ContentLength.HasValue)
            {
                context.Response.ContentLength = response.Content.Headers.ContentLength;
            }

            if (response.Headers.TryGetValues("Accept-Ranges", out var acceptRanges))
            {
                context.Response.Headers["Accept-Ranges"] = acceptRanges.First();
            }

            if (response.Content.Headers.TryGetValues("Content-Range", out var contentRange))
            {
                context.Response.Headers["Content-Range"] = contentRange.First();
            }

            // Stream do conteúdo para download
            await using var sourceStream = await response.Content.ReadAsStreamAsync(context.RequestAborted);
            await sourceStream.CopyToAsync(context.Response.Body, context.RequestAborted);
        }
        catch (OperationCanceledException)
        {
            // Cliente cancelou a requisição
        }
        catch (HttpRequestException)
        {
            context.Response.StatusCode = 502; // Bad Gateway
        }
    }

    private static async Task CopyResponseToContext(
        HttpResponseMessage response,
        HttpContext context,
        string sourceUrl,
        string? extension,
        bool supportsRange)
    {
        if (!response.IsSuccessStatusCode)
        {
            context.Response.StatusCode = (int)response.StatusCode;
            return;
        }

        context.Response.StatusCode = (int)response.StatusCode;

        var contentType = GetContentType(extension, response.Content.Headers.ContentType?.MediaType);
        context.Response.ContentType = contentType;

        if (supportsRange && response.Headers.TryGetValues("Accept-Ranges", out var acceptRanges))
        {
            context.Response.Headers["Accept-Ranges"] = acceptRanges.First();
        }

        if (response.Content.Headers.TryGetValues("Content-Range", out var contentRange))
        {
            context.Response.Headers["Content-Range"] = contentRange.First();
        }

        if (contentType.Contains("mpegurl") || contentType.Contains("m3u8") || sourceUrl.EndsWith(".m3u8"))
        {
            var m3u8Content = await response.Content.ReadAsStringAsync(context.RequestAborted);
            var rewrittenContent = RewriteM3u8Urls(m3u8Content, sourceUrl);
            await context.Response.WriteAsync(rewrittenContent, context.RequestAborted);
        }
        else
        {
            if (response.Content.Headers.ContentLength.HasValue)
            {
                context.Response.ContentLength = response.Content.Headers.ContentLength;
            }

            await using var sourceStream = await response.Content.ReadAsStreamAsync(context.RequestAborted);
            await sourceStream.CopyToAsync(context.Response.Body, context.RequestAborted);
        }
    }

    /// <summary>
    /// Reescreve URLs relativas em arquivos M3U8 para URLs absolutas baseadas na fonte original.
    /// </summary>
    private static string RewriteM3u8Urls(string m3u8Content, string sourceUrl)
    {
        var sourceUri = new Uri(sourceUrl);
        var baseUrl = sourceUri.GetLeftPart(UriPartial.Path);
        var baseDir = baseUrl.Substring(0, baseUrl.LastIndexOf('/') + 1);

        var lines = m3u8Content.Split('\n');
        var rewrittenLines = new List<string>();

        foreach (var line in lines)
        {
            var trimmedLine = line.TrimEnd('\r');

            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith('#'))
            {
                rewrittenLines.Add(trimmedLine);
                continue;
            }

            if (!trimmedLine.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !trimmedLine.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                var absoluteUrl = trimmedLine.StartsWith("/")
                    ? $"{sourceUri.Scheme}://{sourceUri.Authority}{trimmedLine}"
                    : baseDir + trimmedLine;
                rewrittenLines.Add(absoluteUrl);
            }
            else
            {
                rewrittenLines.Add(trimmedLine);
            }
        }

        return string.Join("\n", rewrittenLines);
    }

    private static string GetContentType(string? extension, string? originalContentType)
    {
        return extension?.ToLowerInvariant() switch
        {
            "ts" => "video/mp2t",
            "m3u8" => "application/vnd.apple.mpegurl",
            "mp4" => "video/mp4",
            "mkv" => "video/x-matroska",
            "avi" => "video/x-msvideo",
            _ => originalContentType ?? "application/octet-stream"
        };
    }

    private static IResult HandleVodInfo(XtreamSimulator simulator, int? vodId)
    {
        if (!vodId.HasValue)
        {
            return Results.Ok(new { error = "Invalid vod_id" });
        }

        var vodInfo = simulator.GetVodInfo(vodId.Value);
        if (vodInfo == null)
        {
            return Results.Ok(new { error = "VOD not found" });
        }

        return Results.Ok(vodInfo);
    }

    private static IResult HandleSeriesInfo(XtreamSimulator simulator, int? seriesId, string username, string password)
    {
        if (!seriesId.HasValue)
        {
            return Results.Ok(new { error = "Invalid series_id" });
        }

        var seriesInfo = simulator.GetSeriesInfoResponse(seriesId.Value, username, password);
        if (seriesInfo == null)
        {
            return Results.Ok(new { error = "Series not found" });
        }

        return Results.Ok(seriesInfo);
    }

    private static IResult HandleShortEpg(XtreamSimulator simulator, int? streamId, int? limit)
    {
        if (!streamId.HasValue)
        {
            return Results.Ok(new { error = "Invalid stream_id" });
        }

        var epgLimit = limit ?? 4;
        return Results.Ok(simulator.GetShortEpg(streamId.Value, epgLimit));
    }

    private static IResult HandleSimpleDataTable(XtreamSimulator simulator, int? streamId)
    {
        if (!streamId.HasValue)
        {
            return Results.Ok(new { error = "Invalid stream_id" });
        }

        return Results.Ok(simulator.GetFullEpgForChannel(streamId.Value));
    }

    private static string GetFileNameFromUrl(string url, string? extension)
    {
        try
        {
            var uri = new Uri(url);
            var fileName = Path.GetFileName(uri.LocalPath);

            if (!string.IsNullOrEmpty(fileName) && fileName.Contains('.'))
            {
                return fileName;
            }
        }
        catch
        {
            // Ignorar erros de parsing
        }

        // Gerar nome padrão
        var ext = extension ?? "ts";
        return $"stream_{DateTime.UtcNow:yyyyMMddHHmmss}.{ext}";
    }
}
