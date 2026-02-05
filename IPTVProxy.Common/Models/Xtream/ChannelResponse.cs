namespace IPTVProxy.Common.Models.Xtream;

using System.Text.Json.Serialization;

/// <summary>
/// DTO de resposta para canais ao vivo (oculta DirectSource original).
/// Expõe a URL de proxy em vez da URL original.
/// </summary>
public record ChannelResponse(
    [property: JsonPropertyName("num")] int Num,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("stream_type")] string StreamType,
    [property: JsonPropertyName("stream_id")] int StreamId,
    [property: JsonPropertyName("stream_icon")] string StreamIcon,
    [property: JsonPropertyName("epg_channel_id")] string? EpgChannelId,
    [property: JsonPropertyName("added")] string Added,
    [property: JsonPropertyName("category_id")] string CategoryId,
    [property: JsonPropertyName("custom_sid")] string? CustomSid,
    [property: JsonPropertyName("tv_archive")] int TvArchive,
    [property: JsonPropertyName("direct_source")] string DirectSource, // URL de proxy
    [property: JsonPropertyName("tv_archive_duration")] int TvArchiveDuration
);

/// <summary>
/// DTO de resposta para VODs (oculta DirectSource original).
/// Expõe a URL de proxy em vez da URL original.
/// </summary>
public record VodResponse(
    [property: JsonPropertyName("num")] int Num,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("stream_type")] string StreamType,
    [property: JsonPropertyName("stream_id")] int StreamId,
    [property: JsonPropertyName("stream_icon")] string StreamIcon,
    [property: JsonPropertyName("rating")] string Rating,
    [property: JsonPropertyName("rating_5based")] double Rating5Based,
    [property: JsonPropertyName("added")] string Added,
    [property: JsonPropertyName("category_id")] string CategoryId,
    [property: JsonPropertyName("container_extension")] string ContainerExtension,
    [property: JsonPropertyName("custom_sid")] string? CustomSid,
    [property: JsonPropertyName("direct_source")] string DirectSource // URL de proxy
);

/// <summary>
/// DTO de resposta para séries (oculta DirectSource original).
/// Expõe a URL de proxy em vez da URL original.
/// </summary>
public record SeriesResponse(
    [property: JsonPropertyName("num")] int Num,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("series_id")] int SeriesId,
    [property: JsonPropertyName("cover")] string Cover,
    [property: JsonPropertyName("plot")] string Plot,
    [property: JsonPropertyName("cast")] string Cast,
    [property: JsonPropertyName("director")] string Director,
    [property: JsonPropertyName("genre")] string Genre,
    [property: JsonPropertyName("release_date")] string ReleaseDate,
    [property: JsonPropertyName("last_modified")] string LastModified,
    [property: JsonPropertyName("rating")] string Rating,
    [property: JsonPropertyName("rating_5based")] double Rating5Based,
    [property: JsonPropertyName("backdrop_path")] List<string> BackdropPath,
    [property: JsonPropertyName("episode_run_time")] string EpisodeRunTime,
    [property: JsonPropertyName("category_id")] string CategoryId
);

/// <summary>
/// DTO de resposta para episódios (oculta DirectSource original).
/// Expõe a URL de proxy em vez da URL original.
/// </summary>
public record EpisodeResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("episode_num")] int EpisodeNum,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("container_extension")] string ContainerExtension,
    [property: JsonPropertyName("info")] EpisodeInfoResponse Info,
    [property: JsonPropertyName("custom_sid")] string? CustomSid,
    [property: JsonPropertyName("added")] string Added,
    [property: JsonPropertyName("season")] int Season,
    [property: JsonPropertyName("direct_source")] string DirectSource // URL de proxy
);

/// <summary>
/// DTO de resposta para informações de episódios.
/// </summary>
public record EpisodeInfoResponse(
    [property: JsonPropertyName("movie_image")] string MovieImage,
    [property: JsonPropertyName("plot")] string Plot,
    [property: JsonPropertyName("releasedate")] string ReleaseDate,
    [property: JsonPropertyName("rating")] double Rating,
    [property: JsonPropertyName("duration_secs")] int DurationSecs,
    [property: JsonPropertyName("duration")] string Duration,
    [property: JsonPropertyName("bitrate")] int Bitrate,
    [property: JsonPropertyName("video")] VideoInfoResponse Video,
    [property: JsonPropertyName("audio")] AudioInfoResponse Audio
);

/// <summary>
/// DTO de resposta para informações de vídeo.
/// </summary>
public record VideoInfoResponse(
    [property: JsonPropertyName("codec_name")] string CodecName,
    [property: JsonPropertyName("width")] int Width,
    [property: JsonPropertyName("height")] int Height
);

/// <summary>
/// DTO de resposta para informações de áudio.
/// </summary>
public record AudioInfoResponse(
    [property: JsonPropertyName("codec_name")] string CodecName,
    [property: JsonPropertyName("channels")] int Channels,
    [property: JsonPropertyName("sample_rate")] string SampleRate
);

/// <summary>
/// DTO de resposta para informações detalhadas de uma série.
/// Usa EpisodeResponse para ocultar DirectSource original.
/// </summary>
public record SeriesInfoResponse(
    [property: JsonPropertyName("seasons")] List<SeasonInfoResponse> Seasons,
    [property: JsonPropertyName("info")] SeriesDetailsResponse Info,
    [property: JsonPropertyName("episodes")] Dictionary<string, List<EpisodeResponse>> Episodes
);

/// <summary>
/// DTO de resposta para informações de temporada.
/// </summary>
public record SeasonInfoResponse(
    [property: JsonPropertyName("season_number")] int SeasonNumber,
    [property: JsonPropertyName("air_date")] string AirDate,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("overview")] string Overview,
    [property: JsonPropertyName("episode_count")] int EpisodeCount,
    [property: JsonPropertyName("cover")] string Cover,
    [property: JsonPropertyName("cover_big")] string CoverBig
);

/// <summary>
/// DTO de resposta para detalhes de série.
/// </summary>
public record SeriesDetailsResponse(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("cover")] string Cover,
    [property: JsonPropertyName("plot")] string Plot,
    [property: JsonPropertyName("cast")] string Cast,
    [property: JsonPropertyName("director")] string Director,
    [property: JsonPropertyName("genre")] string Genre,
    [property: JsonPropertyName("releaseDate")] string ReleaseDate,
    [property: JsonPropertyName("last_modified")] string LastModified,
    [property: JsonPropertyName("rating")] string Rating,
    [property: JsonPropertyName("rating_5based")] double Rating5Based,
    [property: JsonPropertyName("backdrop_path")] List<string> BackdropPath,
    [property: JsonPropertyName("youtube_trailer")] string? YoutubeTrailer,
    [property: JsonPropertyName("episode_run_time")] string EpisodeRunTime,
    [property: JsonPropertyName("category_id")] string CategoryId
);
