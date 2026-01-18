using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace Nivaes.Otel.Backend.Sample
{
    internal class SampleLogging
    {
        public static ILoggerFactory ConfigureLogginOpenTelemetryGrpc(string endpoint)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddOpenTelemetry(options =>
                {
                    options.AddOtlpExporter(o =>
                    {
                        o.Endpoint = new Uri(endpoint);
                        o.Protocol = OtlpExportProtocol.Grpc;
                    });
                });

            });

            return loggerFactory;
        }

        public static ILoggerFactory ConfigureLogginOpenTelemetryHtml(string endpoint)
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddOpenTelemetry(options =>
                {
                    options.AddOtlpExporter(o =>
                    {
                        o.Endpoint = new Uri($"{endpoint}/v1/logs");
                        //o.Endpoint = new Uri($"{endpoint}");
                        o.Protocol = OtlpExportProtocol.HttpProtobuf;
                    });
                });

            });

            return loggerFactory;
        }


        public static async Task SendLog(ILoggerFactory loggerFactory)
        {
            for (int i = 0; i < 10; i++)
            {
                ILogger loggerConsole = loggerFactory.CreateLogger("console");
                loggerConsole.LogInformation("Log console enviado de prueba");
                Console.WriteLine("Log enviado");

                await Task.Delay(100);

                ILogger loggerSample = loggerFactory.CreateLogger<SampleLogging>();
                loggerSample.LogInformation("Log enviado de prueba");
                Console.WriteLine("Log enviado");

                await Task.Delay(100);
            }
        }
    }
}
