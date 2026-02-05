namespace IPTVProxy.Common.Extensions;

using IPTVProxy.Common.Services;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensões para configuração de serviços da aplicação.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registra os serviços de M3U e Xtream na aplicação.
    /// </summary>
    public static IServiceCollection AddIptvServices(
        this IServiceCollection services,
        string m3uFilePath,
        string serverUrl)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register M3uPlaylistService with factory
        services.AddSingleton<M3uPlaylistService>(provider =>
        {
            var playlistService = new M3uPlaylistService();

            if (File.Exists(m3uFilePath))
            {
                playlistService.LoadFromFile(m3uFilePath);
            }

            return playlistService;
        });

        // Register XtreamSimulator with factory
        services.AddSingleton<XtreamSimulator>(provider =>
        {
            var playlistService = provider.GetRequiredService<M3uPlaylistService>();
            return playlistService.IsLoaded
                ? new XtreamSimulator(playlistService, serverUrl)
                : new XtreamSimulator(serverUrl);
        });

        return services;
    }
}
