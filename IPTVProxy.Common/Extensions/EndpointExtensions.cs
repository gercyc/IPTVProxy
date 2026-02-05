namespace IPTVProxy.Common.Extensions;

using IPTVProxy.Common.Api;
using IPTVProxy.Common.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Extensões para mapeamento de endpoints da aplicação.
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Mapeia o endpoint de health check da aplicação.
    /// </summary>
    public static IEndpointRouteBuilder MapHealthCheckEndpoint(
        this IEndpointRouteBuilder endpoints,
        string serverUrl)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        endpoints.MapGet("/", (M3uPlaylistService playlistService) =>
        {
            return Results.Ok(new
            {
                status = "running",
                service = "IPTV Proxy - Xtream Codes API Simulator",
                version = "1.0.0",
                serverUrl,
                playlistLoaded = playlistService.IsLoaded,
                entriesCount = playlistService.Playlist?.Entries.Count ?? 0,
                credentials = new
                {
                    username = "demo",
                    password = "demo123"
                },
                endpoints = new[]
                {
                    $"{serverUrl}/api/xtreamapi/get.php?username=demo&password=demo123&type=m3u_plus&output=ts",
                    $"{serverUrl}/api/xtreamapi/player_api.php?username=demo&password=demo123",
                    $"{serverUrl}/api/xtreamapi/xmltv.php?username=demo&password=demo123"
                },
                documentation = new[]
                {
                    $"{serverUrl}/swagger",
                    $"{serverUrl}/docs",
                    $"{serverUrl}/openapi/v1.json"
                }
            });
        })
        .WithName("HealthCheck")
        .WithTags("Health")
        .WithSummary("Health check endpoint")
        .WithDescription("Retorna o status do serviço e informações de configuração");

        //endpoints.MapHealthChecks("/health");

        return endpoints;
    }

    /// <summary>
    /// Mapeia os endpoints da Xtream API usando Minimal APIs.
    /// Alternativa ao XtreamApiController.
    /// </summary>
    public static IEndpointRouteBuilder MapXtreamApiEndpoints(
        this IEndpointRouteBuilder endpoints,
        string baseRoute = "/api/xtreamapi")
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        return endpoints.MapXtreamApi(baseRoute);
    }
}
