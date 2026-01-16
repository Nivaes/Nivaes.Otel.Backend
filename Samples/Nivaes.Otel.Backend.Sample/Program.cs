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
        static async Task Main(string[] args)
        {
            Console.ReadKey();

            {
                using var tracerProvider = SampleTracer.ConfigureTrazeOpenTelemetryGrpc();
                await SampleTracer.SendSpand(tracerProvider);
            }

            {
                using var meterProvider = SampleMeter.ConfigureMeterOpenTelemetryGrpc();
                await SampleMeter.SendMeter(meterProvider);
            }

            {
                using var tracerProvider = SampleTracer.ConfigureTrazeOpenTelemetryHtml();
                await SampleTracer.SendSpand(tracerProvider);
            }

            {
                using var meterProvider = SampleMeter.ConfigureMeterOpenTelemetryHtml();
                await SampleMeter.SendMeter(meterProvider);
            }

            {
                using var loggerProvider = SampleLogging.ConfigureLogginOpenTelemetryGrpc();
                await SampleLogging.SendLog(loggerProvider);
            }

            Console.ReadKey();
        }
    }
}
