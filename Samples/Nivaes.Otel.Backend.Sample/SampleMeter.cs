using System.Diagnostics.Metrics;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Nivaes.Otel.Backend.Sample
{
    internal class SampleMeter
    {
        public static MeterProvider ConfigureMeterOpenTelemetryGrpc(string endpoint)
        {
            var meterProvider = Sdk.CreateMeterProviderBuilder()
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService(serviceName: "my-metrics-service"))
                     .AddMeter("MyApp.Metrics")
                     //.SetExemplarFilter(ExemplarFilterType.AlwaysOn)
                     .AddOtlpExporter(o =>
                     {
                         o.Endpoint = new Uri(endpoint);
                         //o.Endpoint = new Uri("http://localhost:32777");
                         //o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                         //o.ExportProcessorType = ExportProcessorType.Batch;
                     })
                     .Build();
            return meterProvider;
        }

        public static MeterProvider ConfigureMeterOpenTelemetryHtml(string endpoint)
        {
            var meterProvider = Sdk.CreateMeterProviderBuilder()
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService(serviceName: "my-metrics-service"))
                     .AddMeter("MyApp.Metrics")
                     .AddOtlpExporter(o =>
                     {
                         o.Endpoint = new Uri($"{endpoint}/v1/metrics");
                         o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                     })
                     .Build();
            return meterProvider;
        }

        public static async Task SendMeter(MeterProvider meterProvider)
        {
            Meter _meter = new Meter("MyApp.Metrics", "1.0.0");
            Counter<long> _requestCounter = _meter.CreateCounter<long>("myapp.requests.count");

            int i = 0;
            while (i++ < 10)
            {
                // incrementa la métrica
                _requestCounter.Add(1, new KeyValuePair<string, object?>("route", "/home"));

                Console.WriteLine("Métrica enviada");
                await Task.Delay(100);
            }
        }
    }
}
