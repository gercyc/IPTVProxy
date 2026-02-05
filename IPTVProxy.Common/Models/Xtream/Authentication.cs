namespace IPTVProxy.Common.Models.Xtream;

using System.Text.Json.Serialization;

/// <summary>
/// Resposta do login da XtreamAPI.
/// </summary>
public record LoginResponse
{
    [JsonPropertyName("user_info")]
    public UserInfo UserInfo { get; init; } = new();

    [JsonPropertyName("server_info")]
    public ServerInfo ServerInfo { get; init; } = new();
}

public record UserInfo
{
    [JsonPropertyName("username")]
    public string Username { get; init; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; init; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    [JsonPropertyName("auth")]
    public int Auth { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = "Active";

    [JsonPropertyName("exp_date")]
    public string? ExpDate { get; init; }

    [JsonPropertyName("is_trial")]
    public string IsTrial { get; init; } = "0";

    [JsonPropertyName("active_cons")]
    public string ActiveCons { get; init; } = "0";

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; init; } = string.Empty;

    [JsonPropertyName("max_connections")]
    public string MaxConnections { get; init; } = "1";

    [JsonPropertyName("allowed_output_formats")]
    public List<string> AllowedOutputFormats { get; init; } = ["m3u8", "ts"];
}

public record ServerInfo
{
    [JsonPropertyName("url")]
    public string Url { get; init; } = string.Empty;

    [JsonPropertyName("port")]
    public string Port { get; init; } = "80";

    [JsonPropertyName("https_port")]
    public string HttpsPort { get; init; } = "443";

    [JsonPropertyName("server_protocol")]
    public string ServerProtocol { get; init; } = "http";

    [JsonPropertyName("rtmp_port")]
    public string RtmpPort { get; init; } = "8880";

    [JsonPropertyName("timezone")]
    public string Timezone { get; init; } = "America/Sao_Paulo";

    [JsonPropertyName("timestamp_now")]
    public long TimestampNow { get; init; }

    [JsonPropertyName("time_now")]
    public string TimeNow { get; init; } = string.Empty;
}

/// <summary>
/// Resposta de erro de autenticação.
/// </summary>
public record AuthErrorResponse
{
    [JsonPropertyName("user_info")]
    public AuthErrorUserInfo UserInfo { get; init; } = new();
}

public record AuthErrorUserInfo
{
    [JsonPropertyName("auth")]
    public int Auth { get; init; }

    [JsonPropertyName("status")]
    public string Status { get; init; } = "Disabled";

    [JsonPropertyName("message")]
    public string Message { get; init; } = "Invalid credentials";
}
