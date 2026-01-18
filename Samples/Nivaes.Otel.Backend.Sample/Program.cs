using System.Diagnostics.Metrics;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Nivaes.Otel.Backend.Sample
{
    internal class Program
    {
        //const string grpcEndpoint = "http://localhost:32803";
        //const string htmlEndpoint = "http://localhost:32804";

        const string grpcEndpoint = "http://localhost:4317";
        const string htmlEndpoint = "http://localhost:4318";


        static async Task Main(string[] args)
        {
            {
                using var tracerProvider = SampleTracer.ConfigureTrazeOpenTelemetryGrpc(grpcEndpoint);
                await SampleTracer.SendSpand(tracerProvider);
            }

            {
                using var tracerProvider = SampleTracer.ConfigureTrazeOpenTelemetryHtml(htmlEndpoint);
                await SampleTracer.SendSpand(tracerProvider);
            }

            {
                using var meterProvider = SampleMeter.ConfigureMeterOpenTelemetryGrpc(grpcEndpoint);
                await SampleMeter.SendMeter(meterProvider);
            }

            {
                using var meterProvider = SampleMeter.ConfigureMeterOpenTelemetryHtml(htmlEndpoint);
                await SampleMeter.SendMeter(meterProvider);
            }

            {
                using var loggerProvider = SampleLogging.ConfigureLogginOpenTelemetryGrpc(grpcEndpoint);
                await SampleLogging.SendLog(loggerProvider);
            }

            {
                using var loggerProvider = SampleLogging.ConfigureLogginOpenTelemetryHtml(htmlEndpoint);
                await SampleLogging.SendLog(loggerProvider);
            }

            Console.ReadKey();
        }
    }
}
