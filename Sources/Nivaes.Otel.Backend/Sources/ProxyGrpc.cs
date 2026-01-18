using System.Net.Http.Headers;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;

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

        var configuration = builder.Configuration;

        var _endpoint = configuration["OpenTelemetry:GrpcEndpoint"];
        Console.WriteLine($"GrpcEndpoint:{_endpoint}");


        //builder.Services.AddOpenTelemetryTracing(b =>
        //{
        //    b.AddAspNetCoreInstrumentation()
        //     .AddHttpClientInstrumentation()
        //     .AddOtlpExporter(o =>
        //     {
        //         o.Endpoint = new Uri(_grpcEndpoint);
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
        builder.Logging.AddFilter("Microsoft.AspNetCore.Server.Kestrel", LogLevel.Error);
        builder.Logging.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Error);

        builder.Logging.AddOpenTelemetry(o =>
        {
            o.AddOtlpExporter(e =>
            {
                e.Endpoint = new Uri("http://otel-collector:4318/v1/logs");
                e.Protocol = OtlpExportProtocol.HttpProtobuf;
            });
        });

        var app = builder.Build();

        // Middleware de logging
        app.Use(async (HttpContext context, RequestDelegate next) =>
        {
            Console.WriteLine($"--> {DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fff")}");
            Console.WriteLine("Grpc");
            Console.WriteLine($"{context.Request.Method} {context.Request.Path}");
            Console.WriteLine($"QueryString:{context.Request.QueryString}");
            foreach (var item in context.Request.Query)
            {
                Console.WriteLine($"Query:{item.Key}-{item.Value}");
            }


            foreach (var header in context.Request.Headers)
            {
                Console.WriteLine($"{header.Key}: {header.Value}");
            }

            await next(context);

            Console.WriteLine($"<-- {context.Response.StatusCode}");

            Console.Out.Flush();
        });

        // "/opentelemetry.proto.collector.trace.v1.TraceService/Export"
        // "/opentelemetry.proto.collector.metrics.v1.MetricsService/Export"
        // "/opentelemetry.proto.collector.logs.v1.LogsService/Export"
        app.MapPost("{**path}", async (HttpContext context, IHttpClientFactory httpFactory) =>
        {
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine("ProxyGrpc - Init");

            //using var memoryBuffer = new MemoryStream();

            using var memoryBuffer = new MemoryStream();
            await context.Request.Body.CopyToAsync(memoryBuffer);
            memoryBuffer.Position = 0;
            var content = new StreamContent(memoryBuffer);
            content.Headers.ContentType = new MediaTypeHeaderValue(context.Request.ContentType ?? "application/grpc");

            var httpClient = httpFactory.CreateClient();
            try
            {
                //var response = await httpClient.PostAsync($"{_endpoint}{context.Request.Path}", content);
                var response = await httpClient.PostAsync($"{_endpoint}", content);

                Console.WriteLine($"ProxyGrpc - Fin - {response.StatusCode}");
                return Results.StatusCode((int)response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                //Console.WriteLine($"ProxyGrpc - Error:{ex.Message}");
                Console.WriteLine(ex.ToString());
                return Results.Ok();
            }
            finally
            {
                Console.WriteLine("----------------------------------------------------------------");
            }
        });

        //app.MapPost("/opentelemetry.proto.collector.logs.v1.LogsService/Export", async (HttpContext context, IHttpClientFactory httpFactory) =>
        //{
        //    Console.WriteLine("----------------------------------------------------------------");
        //    Console.WriteLine("ProxyGrpc - log-  Init");
        //    var httpClient = httpFactory.CreateClient();

        //    var content = new StreamContent(context.Request.Body);
        //    content.Headers.ContentType = new MediaTypeHeaderValue(context.Request.ContentType ?? "application/grpc");

        //    //Console.WriteLine($"Content:{content}");

        //    try
        //    {
        //        var response = await httpClient.PostAsync($"{_grpcEndpoint}/opentelemetry.proto.collector.logs.v1.LogsService/Export", content);
        //        Console.WriteLine($"ProxyGrpc - Fin - {response.StatusCode}");
        //        return Results.StatusCode((int)response.StatusCode);
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        Console.WriteLine($"ProxyGrpc - Error:{ex.Message}");
        //        return Results.Ok();
        //    }
        //    finally
        //    {
        //        Console.WriteLine("----------------------------------------------------------------");
        //    }
        //});

        return app.RunAsync();
    }

    //private static async Task WriteStream(Stream stream)
    //{
    //    using var reader = new StreamReader(stream, Encoding.Unicode, true, leaveOpen: true);
    //    Console.WriteLine(await reader.ReadToEndAsync());
    //}
}
