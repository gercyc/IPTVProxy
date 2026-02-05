using IPTVProxy.Common.Models.Xtream;
using IPTVProxy.Common.Services;
using Microsoft.AspNetCore.Mvc;

namespace IPTVProxy.Controllers;

/// <summary>
/// Controller para os endpoints da Xtream API simulada.
/// Documentação: https://github.com/tellytv/go.xtream-codes/wiki/Xtream-API
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class XtreamApiController : ControllerBase
{
    private readonly XtreamSimulator _simulator;
    private readonly ILogger<XtreamApiController> _logger;
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(300) // 5 minutos para streaming de longa duração
    };

    public XtreamApiController(XtreamSimulator simulator, ILogger<XtreamApiController> logger)
    {
        ArgumentNullException.ThrowIfNull(simulator);
        ArgumentNullException.ThrowIfNull(logger);
        _simulator = simulator;
        _logger = logger;
    }

    /// <summary>
    /// Obtém a playlist M3U com todos os canais disponíveis.
    /// </summary>
    [HttpGet("get.php")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetM3uPlaylist(string username, string password, [FromQuery] string? type = null, [FromQuery] string? output = null)
    {
        if (!_simulator.ValidateCredentials(username, password))
        {
            return Unauthorized();
        }

        var m3uContent = _simulator.GenerateM3uPlaylist(username, password, type, output);
        return Content(m3uContent, "application/x-mpegurl");
    }

    /// <summary>
    /// API principal da Xtream Codes com suporte a múltiplas ações.
    /// </summary>
    /// <remarks>
    /// - get_live_categories: Lista categorias de canais ao vivo
    /// - get_live_streams: Lista canais(use category_id para filtrar)
    /// - get_vod_categories: Lista categorias de VOD
    /// - get_vod_streams: Lista VODs(use category_id para filtrar)
    /// - get_vod_info: Informações de VOD específico(use vod_id)
    /// - get_series_categories: Lista categorias de séries
    /// - get_series: Lista séries(use category_id para filtrar)
    /// - get_series_info: Informações de série específica(use series_id)
    /// - get_short_epg: EPG resumido(use stream_id e limit)
    /// - get_simple_data_table: EPG completo de um canal(use stream_id)
    /// </remarks>
    [HttpGet("player_api.php")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult PlayerApi([FromQuery] PlayerApiRequest request)
    {
        if (!_simulator.ValidateCredentials(request.Username, request.Password))
        {
            return Ok(_simulator.GetAuthErrorResponse());
        }

        // Se não há action, retorna info de login
        if (string.IsNullOrEmpty(request.Action))
        {
            return Ok(_simulator.GetLoginResponse(request.Username, request.Password));
        }

        return request.Action switch
        {
            // Live Streams
            "get_live_categories" => Ok(_simulator.GetChannelCategories()),
            "get_live_streams" => Ok(string.IsNullOrEmpty(request.CategoryId)
                ? _simulator.GetAllChannelsResponse(request.Username, request.Password)
                : _simulator.GetChannelsByCategoryResponse(request.CategoryId, request.Username, request.Password)),

            // VOD (Movies)
            "get_vod_categories" => Ok(_simulator.GetVodCategories()),
            "get_vod_streams" => Ok(string.IsNullOrEmpty(request.CategoryId)
                ? _simulator.GetAllVodsResponse(request.Username, request.Password)
                : _simulator.GetVodsByCategoryResponse(request.CategoryId, request.Username, request.Password)),
            "get_vod_info" => HandleVodInfo(request.VodId),

            // Series
            "get_series_categories" => Ok(_simulator.GetSeriesCategories()),
            "get_series" => Ok(string.IsNullOrEmpty(request.CategoryId)
                ? _simulator.GetAllSeriesResponse(request.Username, request.Password)
                : _simulator.GetSeriesByCategoryResponse(request.CategoryId, request.Username, request.Password)),
            "get_series_info" => HandleSeriesInfo(request.SeriesId, request.Username, request.Password),

            // EPG
            "get_short_epg" => HandleShortEpg(request.StreamId, request.Limit),
            "get_simple_data_table" => HandleSimpleDataTable(request.StreamId),

            _ => Ok(new { error = "Action not supported" })
        };
    }

    /// <summary>
    /// Obtém o guia de programação eletrônica completo em formato XMLTV/XML.
    /// </summary>
    [HttpGet("xmltv.php")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetXmltvEpg(string username, string password)
    {
        if (!_simulator.ValidateCredentials(username, password))
        {
            return Unauthorized();
        }

        var xmlContent = _simulator.GenerateXmltvEpg();
        return Content(xmlContent, "application/xml");
    }

    /// <summary>
    /// Faz proxy do stream de um canal ao vivo.
    /// </summary>
    [HttpGet("/{username}/{password}/{streamId:int}")]
    public async Task<IActionResult> GetLiveStream(string username, string password, int streamId, CancellationToken cancellationToken)
    {
        if (!_simulator.ValidateCredentials(username, password))
        {
            return Unauthorized();
        }

        var channel = _simulator.GetChannelById(streamId);
        if (channel == null || string.IsNullOrEmpty(channel.DirectSource))
        {
            return NotFound();
        }

        try
        {
            await ProxyStreamAsync(channel.DirectSource, null, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Cliente desconectou - não escrever headers adicionais
            Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
        }
        catch (HttpRequestException ex)
        {
            if (!Response.HasStarted)
            {
                Response.StatusCode = StatusCodes.Status502BadGateway;
                Response.ContentType = "application/json";
                await Response.WriteAsJsonAsync(new { error = "Failed to fetch stream", details = ex.Message }, cancellationToken);
            }
        }
        catch (IOException)
        {
            // Conexão fechada pelo cliente
            if (!Response.HasStarted)
            {
                Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            }
        }

        return new EmptyResult();
    }

    /// <summary>
    /// Faz proxy do stream de um canal ao vivo com extensão específica.
    /// </summary>
    [HttpGet("/{username}/{password}/{streamId:int}.{ext}")]
    public async Task<IActionResult> GetLiveStreamWithExtension(string username, string password, int streamId, string ext, CancellationToken cancellationToken)
    {
        if (!_simulator.ValidateCredentials(username, password))
        {
            return Unauthorized();
        }

        var channel = _simulator.GetChannelById(streamId);
        if (channel == null || string.IsNullOrEmpty(channel.DirectSource))
        {
            return NotFound();
        }

        try
        {
            // Se o DirectSource já é um M3U8 (HLS), retornar a playlist
            // Caso contrário, fazer proxy do stream
            if (channel.DirectSource.Contains(".m3u8", StringComparison.OrdinalIgnoreCase))
            {
                await ProxyStreamAsync(channel.DirectSource, "m3u8", cancellationToken);
            }
            else
            {
                await ProxyStreamAsync(channel.DirectSource, ext, cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
            // Cliente desconectou
            if (!Response.HasStarted)
            {
                Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            }
        }
        catch (HttpRequestException ex)
        {
            if (!Response.HasStarted)
            {
                Response.StatusCode = StatusCodes.Status502BadGateway;
                Response.ContentType = "application/json";
                await Response.WriteAsJsonAsync(new { error = "Failed to fetch stream", details = ex.Message }, cancellationToken);
            }
        }
        catch (IOException)
        {
            if (!Response.HasStarted)
            {
                Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            }
        }

        return new EmptyResult();
    }

    /// <summary>
    /// Faz proxy do stream de um filme.
    /// </summary>
    [HttpGet("movie/{username}/{password}/{vodId:int}.{ext}")]
    public async Task<IActionResult> GetMovieStream(string username, string password, int vodId, string ext, CancellationToken cancellationToken)
    {
        if (!_simulator.ValidateCredentials(username, password))
        {
            return Unauthorized();
        }

        var vod = _simulator.GetVodById(vodId);
        if (vod == null || string.IsNullOrEmpty(vod.DirectSource))
        {
            return NotFound();
        }

        try
        {
            await ProxyStreamAsync(vod.DirectSource, ext, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Cliente desconectou
            if (!Response.HasStarted)
            {
                Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            }
        }
        catch (HttpRequestException ex)
        {
            if (!Response.HasStarted)
            {
                Response.StatusCode = StatusCodes.Status502BadGateway;
                Response.ContentType = "application/json";
                await Response.WriteAsJsonAsync(new { error = "Failed to fetch stream", details = ex.Message }, cancellationToken);
            }
        }
        catch (IOException)
        {
            if (!Response.HasStarted)
            {
                Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            }
        }

        return new EmptyResult();
    }

    /// <summary>
    /// Faz proxy do stream de um episódio de série.
    /// </summary>
    /// <param name="username">Nome de usuário para autenticação</param>
    /// <param name="password">Senha para autenticação</param>
    /// <param name="episodeId">ID do episódio da série</param>
    /// <param name="ext">Extensão do arquivo (mp4, mkv, avi, etc.)</param>
    /// <param name="cancellationToken">Token de cancelamento da operação</param>
    /// <returns>Stream do episódio no formato especificado</returns>
    /// <response code="200">Stream iniciado com sucesso</response>
    /// <response code="401">Credenciais inválidas</response>
    /// <response code="404">Episódio não encontrado</response>
    /// <response code="499">Cliente cancelou a requisição</response>
    /// <response code="502">Falha ao buscar o stream da fonte</response>
    [HttpGet("series/{username}/{password}/{episodeId}.{ext}")]
    public async Task<IActionResult> GetSeriesEpisodeStream(string username, string password, string episodeId, string ext, CancellationToken cancellationToken)
    {
        if (!_simulator.ValidateCredentials(username, password))
        {
            return Unauthorized();
        }

        var episode = _simulator.GetEpisodeById(episodeId);
        if (episode == null || string.IsNullOrEmpty(episode.DirectSource))
        {
            return NotFound();
        }

        try
        {
            await ProxyStreamAsync(episode.DirectSource, ext, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Cliente desconectou
            if (!Response.HasStarted)
            {
                Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            }
        }
        catch (HttpRequestException ex)
        {
            if (!Response.HasStarted)
            {
                Response.StatusCode = StatusCodes.Status502BadGateway;
                Response.ContentType = "application/json";
                await Response.WriteAsJsonAsync(new { error = "Failed to fetch stream", details = ex.Message }, cancellationToken);
            }
        }
        catch (IOException)
        {
            if (!Response.HasStarted)
            {
                Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            }
        }

        return new EmptyResult();
    }

    /// <summary>
    /// Faz proxy do stream, baixando da fonte e enviando para o cliente.
    /// Para arquivos M3U8, reescreve URLs relativas para usar a URL da fonte original.
    /// </summary>
    private async Task ProxyStreamAsync(string sourceUrl, string? extension, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting proxy stream from {SourceUrl}", sourceUrl);

        using var request = new HttpRequestMessage(HttpMethod.Get, sourceUrl);

        // Copiar headers relevantes do cliente
        var hasRangeRequest = false;
        if (Request.Headers.TryGetValue("Range", out var rangeHeader))
        {
            request.Headers.TryAddWithoutValidation("Range", rangeHeader.ToString());
            hasRangeRequest = true;
            _logger.LogDebug("Range request: {Range}", rangeHeader);
        }

        _logger.LogDebug("Sending request to upstream source");
        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        _logger.LogInformation("Received response from upstream: {StatusCode}", response.StatusCode);

        // Se o upstream retornou 416 (Range Not Satisfiable) e tínhamos um Range request,
        // tentar novamente sem Range
        if (response.StatusCode == System.Net.HttpStatusCode.RequestedRangeNotSatisfiable && hasRangeRequest)
        {
            _logger.LogWarning("Range not satisfiable, retrying without range");
            response.Dispose();
            using var retryRequest = new HttpRequestMessage(HttpMethod.Get, sourceUrl);
            using var retryResponse = await _httpClient.SendAsync(retryRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            await CopyResponseToContext(retryResponse, sourceUrl, extension, supportsRange: false, cancellationToken);
            return;
        }

        await CopyResponseToContext(response, sourceUrl, extension, supportsRange: hasRangeRequest && response.IsSuccessStatusCode, cancellationToken);
    }

    private async Task CopyResponseToContext(HttpResponseMessage response, string sourceUrl, string? extension, bool supportsRange, CancellationToken cancellationToken)
    {
        try
        {
            // Verificar se a resposta foi bem-sucedida
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Upstream returned error status: {StatusCode}", response.StatusCode);
                Response.StatusCode = (int)response.StatusCode;
                return;
            }

            // Definir status code
            Response.StatusCode = (int)response.StatusCode;

            // Definir Content-Type baseado na extensão ou resposta original
            var contentType = GetContentType(extension, response.Content.Headers.ContentType?.MediaType);
            Response.ContentType = contentType;
            _logger.LogDebug("Content-Type set to: {ContentType}", contentType);

            // Copiar headers relevantes da resposta apenas se o upstream suportar ranges
            if (supportsRange && response.Headers.TryGetValues("Accept-Ranges", out var acceptRanges))
            {
                Response.Headers["Accept-Ranges"] = acceptRanges.First();
            }

            if (response.Content.Headers.TryGetValues("Content-Range", out var contentRange))
            {
                Response.Headers["Content-Range"] = contentRange.First();
            }

            // Para M3U8, reescrever URLs relativas para absolutas
            if (contentType.Contains("mpegurl") || contentType.Contains("m3u8") || sourceUrl.EndsWith(".m3u8"))
            {
                _logger.LogDebug("Processing M3U8 playlist");
                var m3u8Content = await response.Content.ReadAsStringAsync(cancellationToken);
                var rewrittenContent = RewriteM3u8Urls(m3u8Content, sourceUrl);

                await Response.WriteAsync(rewrittenContent, cancellationToken);
                _logger.LogInformation("M3U8 playlist sent successfully");
            }
            else
            {
                // Para outros tipos, fazer streaming direto
                if (response.Content.Headers.ContentLength.HasValue)
                {
                    Response.ContentLength = response.Content.Headers.ContentLength;
                    _logger.LogDebug("Content-Length: {Length} bytes", response.Content.Headers.ContentLength);
                }

                _logger.LogInformation("Starting content streaming");
                await using var sourceStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                await sourceStream.CopyToAsync(Response.Body, cancellationToken);
                _logger.LogInformation("Stream completed successfully");
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Client cancelled the request");
            // Cliente cancelou durante streaming - é esperado
            throw;
        }
        catch (IOException ex)
        {
            _logger.LogWarning(ex, "IOException during streaming (client likely disconnected)");
            // Conexão fechada - é esperado quando cliente desconecta
            throw;
        }
        finally
        {
            response.Dispose();
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

            // Ignorar linhas vazias, comentários ou tags
            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith('#'))
            {
                rewrittenLines.Add(trimmedLine);
                continue;
            }

            // Se a linha é uma URL relativa (não começa com http:// ou https://)
            if (!trimmedLine.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !trimmedLine.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                // Converter para URL absoluta
                var absoluteUrl = trimmedLine.StartsWith("/")
                    ? $"{sourceUri.Scheme}://{sourceUri.Authority}{trimmedLine}"
                    : baseDir + trimmedLine;
                rewrittenLines.Add(absoluteUrl);
            }
            else
            {
                // Já é absoluta, manter como está
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

    private IActionResult HandleVodInfo(int? vodId)
    {
        if (!vodId.HasValue)
        {
            return Ok(new { error = "Invalid vod_id" });
        }

        var vodInfo = _simulator.GetVodInfo(vodId.Value);
        if (vodInfo == null)
        {
            return Ok(new { error = "VOD not found" });
        }

        return Ok(vodInfo);
    }

    private IActionResult HandleSeriesInfo(int? seriesId, string username, string password)
    {
        if (!seriesId.HasValue)
        {
            return Ok(new { error = "Invalid series_id" });
        }

        var seriesInfo = _simulator.GetSeriesInfoResponse(seriesId.Value, username, password);
        if (seriesInfo == null)
        {
            return Ok(new { error = "Series not found" });
        }

        return Ok(seriesInfo);
    }

    private IActionResult HandleShortEpg(int? streamId, int? limit)
    {
        if (!streamId.HasValue)
        {
            return Ok(new { error = "Invalid stream_id" });
        }

        var epgLimit = limit ?? 4; // default

        return Ok(_simulator.GetShortEpg(streamId.Value, epgLimit));
    }

    private IActionResult HandleSimpleDataTable(int? streamId)
    {
        if (!streamId.HasValue)
        {
            return Ok(new { error = "Invalid stream_id" });
        }

        return Ok(_simulator.GetFullEpgForChannel(streamId.Value));
    }
}
