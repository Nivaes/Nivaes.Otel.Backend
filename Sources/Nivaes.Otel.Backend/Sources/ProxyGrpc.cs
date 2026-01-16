namespace Nivaes.Otel.Backend;

public static class ProxyGrpc
{
    public static Task CrateBuilder(string[] args)
    {
        //var builder = WebApplication.CreateSlimBuilder(args);
        //builder.AddServiceDefaults();
        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(4317);
        });
        builder.Services.AddGrpc();

        builder.Services.AddHttpClient();

        //builder.Services.AddOpenTelemetryTracing(b =>
        //{
        //    b.AddAspNetCoreInstrumentation()
        //     .AddHttpClientInstrumentation()
        //     .AddOtlpExporter(o =>
        //     {
        //         o.Endpoint = new Uri("http://otel-collector:4317");
        //         o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        //     });
        //});

        //builder.Services.AddControllers();

        //builder.Services.ConfigureHttpJsonOptions(options =>
        //{
        //    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
        //});

        //builder.Services.AddOpenApi();

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Trace);

        var app = builder.Build();

        // Middleware de logging
        app.Use(async (HttpContext context, RequestDelegate next) =>
        {
            Console.WriteLine($"--> {context.Request.Method} {context.Request.Path}");
            Console.WriteLine("Grpc");

            foreach (var header in context.Request.Headers)
            {
                Console.WriteLine($"{header.Key}: {header.Value}");
            }

            await next(context);

            Console.WriteLine($"<-- {context.Response.StatusCode}");
        });

        //app.MapDefaultEndpoints();
        //app.MapControllers();

        //app.MapGet("/", async (HttpContext context, IHttpClientFactory httpFactory) =>
        app.MapGet("{*catchall}", async (HttpContext context, IHttpClientFactory httpFactory) =>
        {
            var httpClient = httpFactory.CreateClient();

            var content = new StreamContent(context.Request.Body);
            content.Headers.ContentType = content.Headers.ContentType; ///new System.Net.Http.Headers.MediaTypeHeaderValue(context.Request.ContentType ?? "application/octet-stream");

            Console.WriteLine(content);

            var response = await httpClient.PostAsync("http://otel-collector:4317", content);
            return Results.StatusCode((int)response.StatusCode);
        });

        return app.RunAsync();
    }
}
