using System.Net.Http.Headers;
using Microsoft.AspNetCore.Server.Kestrel.Core;

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
            options.ListenAnyIP(4317, o =>
            {
                o.Protocols = HttpProtocols.Http2;
            });
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

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Trace);

        var app = builder.Build();

        // Middleware de logging
        app.Use(async (HttpContext context, RequestDelegate next) =>
        {
            Console.WriteLine($"--> {DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fff")}");
            Console.WriteLine($"{context.Request.Method} {context.Request.Path}");
            Console.WriteLine("Grpc");

            foreach (var header in context.Request.Headers)
            {
                Console.WriteLine($"{header.Key}: {header.Value}");
            }

            await next(context);

            Console.WriteLine($"<-- {context.Response.StatusCode}");
        });

        // "/opentelemetry.proto.collector.trace.v1.TraceService/Export"
        // "/opentelemetry.proto.collector.metrics.v1.MetricsService/Export"
        // "/opentelemetry.proto.collector.logs.v1.LogsService / Export"
        app.MapPost("{**path}", async (HttpContext context, IHttpClientFactory httpFactory) =>
        {
            var httpClient = httpFactory.CreateClient();

            var content = new StreamContent(context.Request.Body);
            content.Headers.ContentType = new MediaTypeHeaderValue(context.Request.ContentType ?? "application/grpc");

            Console.WriteLine(content);

            try
            {
                var response = await httpClient.PostAsync("http://otel-collector:4317", content);
                return Results.StatusCode((int)response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(ex.Message);
                return Results.Ok();
            }
        });

        return app.RunAsync();
    }
}
