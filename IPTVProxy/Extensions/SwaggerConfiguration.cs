namespace IPTVProxy.Extensions;

/// <summary>
/// Configuração centralizada do Swagger/OpenAPI
/// </summary>
public static class SwaggerConfiguration
{
    /// <summary>
    /// Configura middleware do Swagger e Redoc
    /// </summary>
    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
    {
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/openapi/v1.json", "IPTV Proxy API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "IPTV Proxy - Swagger UI";
            options.DisplayRequestDuration();
            options.EnableDeepLinking();
            options.EnableFilter();
            options.EnableTryItOutByDefault();
        });

        app.UseReDoc(options =>
        {
            options.SpecUrl = "/openapi/v1.json";
            options.RoutePrefix = "docs";
            options.DocumentTitle = "IPTV Proxy - Documentação Completa";
            options.EnableUntrustedSpec();
            options.ScrollYOffset(10);
            options.HideHostname();
            options.HideDownloadButton();
            options.ExpandResponses("200,201");
            options.RequiredPropsFirst();
            options.NoAutoAuth();
            options.PathInMiddlePanel();
            options.HideLoading();
            options.NativeScrollbars();
            options.DisableSearch();
            options.SortPropsAlphabetically();
        });

        return app;
    }
}
