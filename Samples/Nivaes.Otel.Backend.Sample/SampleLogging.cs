using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace Nivaes.Otel.Backend.Sample
{
    internal class SampleLogging
    {
        public static ILoggerFactory ConfigureLogginOpenTelemetryGrpc()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddOpenTelemetry(options =>
                {
                    options.AddOtlpExporter(o =>
                    {
                        //o.Endpoint = new Uri("http://localhost:4317");
                        o.Endpoint = new Uri("http://localhost:32777");
                        o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    });
                });

            });

            return loggerFactory;
        }

        public static ILoggerFactory ConfigureLogginOpenTelemetryHtml()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddOpenTelemetry(options =>
                {
                    options.AddOtlpExporter(o =>
                    {
                        //o.Endpoint = new Uri("http://localhost:4318/v1/logs");
                        o.Endpoint = new Uri("http://localhost:32778/v1/logs");
                        o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
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
