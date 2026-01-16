using System.Diagnostics.Metrics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using OpenTelemetry;
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
            //await Test();

            {
                using var tracerProvider = ConfigureTrazeOpenTelemetry();
                await SendSpand(tracerProvider);
            }

            {
                using var meterProvider = ConfigureMeterOpenTelemetry();
                await SendMeter(meterProvider);
            }

            Console.ReadKey();
        }

        static async Task Test()
        {
            HttpClient _http = new HttpClient();
            var content = new StringContent("texto a enviar"/*, Encoding.UTF8, "application/octet-stream"*/);

            using var response = await _http.PostAsync("http://localhost:4318/api/Telemetry/traces", content);
            //using var response = await _http.GetAsync("http://localhost:5162/WeatherForecast");

            //response.EnsureSuccessStatusCode(); // lanza excepción si no es 2xx

            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine(result);           
        }

        static TracerProvider ConfigureTrazeOpenTelemetry()
        {
            var tracerProvider = Sdk.CreateTracerProviderBuilder()
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MyApp"))
                    .AddSource("MyApp.Source")
                    .AddConsoleExporter()
                    .AddOtlpExporter(o =>
                    {
                        //o.Endpoint = new Uri("http://localhost:4317");
                        //o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;

                        o.Endpoint = new Uri("http://localhost:4318/v1/traces"); // Collector real
                        o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                    })
                .Build();

            return tracerProvider;
        }

        static async Task SendSpand(TracerProvider tracerProvider)
        {
            var tracer = tracerProvider.GetTracer("MyApp.Source");

            using (var span = tracer.StartActiveSpan("ProcessOrder"))
            {
                span.SetAttribute("order.id", 12345);
                span.SetAttribute("order.amount", 99.99);

                {
                    using var span2 = tracer.StartActiveSpan("DoWork");
                    await Task.Delay(100);
                    span2.AddEvent("Trabajo hecho");

                }
                span.AddEvent("Evento importante dentro del span");

                Console.WriteLine("Send span");
                

                // Simular operación
                await Task.Delay(100);

                // Span se cierra automáticamente al salir del using
            }
        }

        static MeterProvider ConfigureMeterOpenTelemetry()
        {
           var meterProvider = Sdk.CreateMeterProviderBuilder()
           .SetResourceBuilder(
               ResourceBuilder.CreateDefault()
                   .AddService(serviceName: "my-metrics-service"))
                    .AddMeter("MyApp.Metrics")
                    .AddOtlpExporter(o =>
                    {
                        // Opción A → HTTP/Protobuf (puerto 4318)
                        o.Endpoint = new Uri("http://localhost:4318/v1/metrics");
                        o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;

                        // Opción B → gRPC (puerto 4317)
                        //o.Endpoint = new Uri("http://localhost:4317");
                        //o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    })
                    .Build();
            return meterProvider;
        }
        static async Task SendMeter(MeterProvider meterProvider)
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

        static async Task SenteProtobuf()
        {
            // OTLP payload ya serializado
            var otlpPayload = System.IO.File.ReadAllBytes("traces.bin"); // ejemplo

            using var httpClient = new HttpClient();
            //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var content = new ByteArrayContent(otlpPayload);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-protobuf");

            var response = await httpClient.PostAsync("https://api.miempresa.com/api/telemetry/traces", content);
            response.EnsureSuccessStatusCode();
        }
    }
}
