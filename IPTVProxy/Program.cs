using IPTVProxy.Common.Extensions;
using IPTVProxy.Common.Services;
using IPTVProxy.Extensions;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((_, config) =>
{
    config.WriteTo.Console(theme: AnsiConsoleTheme.Sixteen)
        .ReadFrom.Configuration(builder.Configuration);
});

// Get configuration
var config = builder.Configuration;
var m3uFilePath = config["M3uFilePath"] ?? "us-grc.m3u";
var applicationUrls = config["ASPNETCORE_URLS"] ?? "http://localhost:5000";
var serverUrl = applicationUrls.Split(';').FirstOrDefault() ?? config["ServerUrl"] ?? "http://localhost:5000";

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddHealthChecks();
builder.Services.AddIptvServices(m3uFilePath, serverUrl);

var app = builder.Build();

// Configure the HTTP request pipeline
app.MapOpenApi();
app.UseSwaggerDocumentation();
app.MapHealthCheckEndpoint(serverUrl);
app.MapControllers();
//app.MapXtreamApiEndpoints("/");

// Get loaded services for logging
var playlistService = app.Services.GetRequiredService<M3uPlaylistService>();

app.Logger.LogInformation("Loading M3U playlist from: {FilePath}", m3uFilePath);
if (playlistService.IsLoaded)
{
    app.Logger.LogInformation("M3U playlist loaded successfully with {Count} entries",
        playlistService.Playlist?.Entries.Count ?? 0);
}
else
{
    app.Logger.LogWarning("M3U file not found at: {FilePath}. Using mock data.", m3uFilePath);
}

app.Logger.LogInformation("IPTV Proxy started");
app.Logger.LogInformation("Server URL: {ServerUrl}", serverUrl);
app.Logger.LogInformation("M3U Playlist: {Status}",
    playlistService.IsLoaded ? $"Loaded ({playlistService.Playlist?.Entries.Count} entries)" : "Mock data");
app.Logger.LogInformation("Credentials: demo / demo123");
app.Logger.LogInformation("Swagger UI: {ServerUrl}/swagger", serverUrl);
app.Logger.LogInformation("ReDoc: {ServerUrl}/docs", serverUrl);
app.Logger.LogInformation("OpenAPI Spec: {ServerUrl}/openapi/v1.json", serverUrl);

await app.RunAsync();
