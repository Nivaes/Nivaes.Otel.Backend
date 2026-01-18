using System.Net.Http.Headers;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;

namespace Nivaes.Otel.Backend;

public static class ProxyHttpProtobuf
{
    public static Task CrateBuilder(string[] args)
    {
        //var builder = WebApplication.CreateSlimBuilder(args);
        //builder.AddServiceDefaults();
        var builder = WebApplication.CreateBuilder(args);

        builder.WebHost.ConfigureKestrel(options =>
        {
            //options.ListenAnyIP(4318);
            options.ListenAnyIP(4318, o =>
            {
                //o.Protocols = HttpProtocols.Http1;
            });
        });

        builder.Services.AddHttpClient();

        var configuration = builder.Configuration;
        var _endpoint = configuration["OpenTelemetry:HttpEndpoint"];
        Console.WriteLine($"HttpEndpoint:{_endpoint}");

        builder.Logging.AddOpenTelemetry(o =>
        {
            o.AddOtlpExporter(e =>
            {
                e.Endpoint = new Uri("http://otel-collector:4318/v1/logs");
                e.Protocol = OtlpExportProtocol.HttpProtobuf;
            });
        });

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Trace);
        builder.Logging.AddFilter("Microsoft.AspNetCore.Server.Kestrel", LogLevel.Error);
        builder.Logging.AddFilter("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Error);

        var app = builder.Build();

        // Middleware de logging
        app.Use(async (HttpContext context, RequestDelegate next) =>
            {
                Console.WriteLine($"--> {DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fff")}");
                Console.WriteLine("HttpProtobuf");
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
                //var body = context.Request.Body;
                //var length = body.Length;
                //using var memoryBuffer = new MemoryStream();
                //var len = context.Request.Body.Length;
                //await context.Request.Body.CopyToAsync(memoryBuffer);

                await next(context);

                Console.WriteLine($"<-- {context.Response.StatusCode}");

                Console.Out.Flush();
            });

        app.MapPost("/v1/traces", async (HttpContext context, IHttpClientFactory httpFactory) =>
        {
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine("ProxyHttpProtobuf - Init - Traces");
            
            //context.Request.EnableBuffering();
            using var memoryBuffer = new MemoryStream();
            await context.Request.Body.CopyToAsync(memoryBuffer);
            memoryBuffer.Position = 0;
            var content = new StreamContent(memoryBuffer);
            content.Headers.ContentType = new MediaTypeHeaderValue(context.Request.ContentType ?? "application/x-protobuf");

            var httpClient = httpFactory.CreateClient();
            try
            {
                var response = await httpClient.PostAsync($"{_endpoint}/v1/traces", content);
                Console.WriteLine($"ProxyHttpProtobuf - StatusCode: {response.StatusCode.ToString()}");
                return Results.StatusCode((int)response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"ProxyHttpProtobuf - traces - Error:{ex.Message}");
                return Results.Ok();
            }
            finally
            {
                Console.WriteLine("----------------------------------------------------------------");
            }
        });

        app.MapPost("/v1/metrics", async (HttpContext context, IHttpClientFactory httpFactory) =>
        {
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine("ProxyHttpProtobuf - Init - metrics");

            //context.Request.EnableBuffering();
            using var memoryBuffer = new MemoryStream();
            await context.Request.Body.CopyToAsync(memoryBuffer);
            memoryBuffer.Position = 0;
            var content = new StreamContent(memoryBuffer);
            content.Headers.ContentType = new MediaTypeHeaderValue(context.Request.ContentType ?? "application/x-protobuf");

            var httpClient = httpFactory.CreateClient();
            try
            {
                var response = await httpClient.PostAsync($"{_endpoint}/v1/metrics", content);
                Console.WriteLine($"ProxyHttpProtobuf - StatusCode: {response.StatusCode.ToString()}");
                return Results.StatusCode((int)response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"ProxyHttpProtobuf - metrics - Error:{ex.Message}");
                return Results.Ok();
            }
            finally
            {
                Console.WriteLine("----------------------------------------------------------------");
            }
        });

        app.MapPost("/v1/logs", async (HttpContext context, IHttpClientFactory httpFactory) =>
        {
            Console.WriteLine("----------------------------------------------------------------");
            Console.WriteLine("ProxyHttpProtobuf - Init - logs");

            //context.Request.EnableBuffering();
            using var memoryBuffer = new MemoryStream();
            await context.Request.Body.CopyToAsync(memoryBuffer);
            memoryBuffer.Position = 0;
            var content = new StreamContent(memoryBuffer);
            content.Headers.ContentType = new MediaTypeHeaderValue(context.Request.ContentType ?? "application/x-protobuf");

            var httpClient = httpFactory.CreateClient();
            try
            {
                var response = await httpClient.PostAsync($"{_endpoint}/v1/logs", content);
                Console.WriteLine($"ProxyHttpProtobuf - StatusCode: {response.StatusCode.ToString()}");
                return Results.StatusCode((int)response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"ProxyHttpProtobuf - logs - Error:{ex.Message}");
                return Results.Ok();
            }
            finally
            {
                Console.WriteLine("----------------------------------------------------------------");
            }
        });

        //app.MapPost("{**path}", async (HttpContext context, IHttpClientFactory httpFactory) =>
        //{
        //    Console.WriteLine("----------------------------------------------------------------");
        //    Console.WriteLine("ProxyHttpProtobuf - Init - path");

        //    context.Request.EnableBuffering();

        //    if(context.Request.Body.Length == 0)
        //    {
        //        return Results.Ok();
        //    }

        //    //using var memoryBuffer = new MemoryStream();
        //    //var body = context.Request.Body;
        //    //var buffer = new byte[8192];
        //    //int bytesRead;

        //    //while ((bytesRead = await body.ReadAsync(buffer, 0, buffer.Length)) > 0)
        //    //{
        //    //    memoryBuffer.Write(buffer);
        //    //}

        //    //await context.Request.Body.CopyToAsync(memoryBuffer);
        //    //buffer.Position = 0;
        //    //context.Request.Body.Position = 0;

        //    //var content = new StreamContent(memoryBuffer);
        //    var content = new StreamContent(context.Request.Body);
        //    content.Headers.ContentType = new MediaTypeHeaderValue(context.Request.ContentType ?? "application/x-protobuf");
        //    content.Headers.Add("User-Agent", context.Request.Headers["User-Agent"][0]);
            
        //    //Console.WriteLine($"Content:{content}");

        //    try
        //    {
        //        var httpClient = httpFactory.CreateClient();
        //        var response = await httpClient.PostAsync($"{_endpoint}{context.Request.Path}", content);
        //        Console.WriteLine($"ProxyHttpProtobuf - StatusCode: {response.StatusCode.ToString()}");
        //        return Results.StatusCode((int)response.StatusCode);
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        //Console.WriteLine($"ProxyHttpProtobuf - Error:{ex.Message}");
        //        Console.WriteLine(ex.ToString());
        //        return Results.Ok();
        //    }
        //    finally
        //    {
        //        Console.WriteLine("----------------------------------------------------------------");
        //    }
        //});

        //app.MapPost("/opentelemetry.proto.collector.logs.v1.LogsService/Export", async (HttpContext context, IHttpClientFactory httpFactory) =>
        //{
        //    Console.WriteLine("----------------------------------------------------------------");
        //    Console.WriteLine("ProxyHttpProtobuf - Init - *****");
        //    var httpClient = httpFactory.CreateClient();

        //    var content = new StreamContent(context.Request.Body);
        //    content.Headers.ContentType = new MediaTypeHeaderValue(context.Request.ContentType ?? "application/grpc");

        //    //Console.WriteLine($"Content:{content}");

        //    try
        //    {
        //        var response = await httpClient.PostAsync($"{_httpEndpoint}/opentelemetry.proto.collector.logs.v1.LogsService/Export", content);
        //        Console.WriteLine($"ProxyHttpProtobuf - StatusCode: {response.StatusCode.ToString()}");
        //        return Results.StatusCode((int)response.StatusCode);
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        Console.WriteLine($"ProxyHttpProtobuf - Error:{ex.Message}");
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
