using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Nivaes.Otel.Backend.Sample
{
    internal static class SampleTracer
    {
        public static TracerProvider ConfigureTrazeOpenTelemetryGrpc()
        {
            var tracerProvider = Sdk.CreateTracerProviderBuilder()
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MyApp"))
                    .AddSource("MyApp.Source")
                    .AddConsoleExporter()
                    .AddOtlpExporter(o =>
                    {
                        //o.Endpoint = new Uri("http://localhost:4317");
                        o.Endpoint = new Uri("http://localhost:32777");
                        o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                    })
                .Build();

            return tracerProvider;
        }

        public static TracerProvider ConfigureTrazeOpenTelemetryHtml()
        {
            var tracerProvider = Sdk.CreateTracerProviderBuilder()
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MyApp"))
                    .AddSource("MyApp.Source")
                    .AddConsoleExporter()
                    .AddOtlpExporter(o =>
                    {
                        //o.Endpoint = new Uri("http://localhost:4318/v1/traces");
                        o.Endpoint = new Uri("http://localhost:32778/v1/traces");
                        o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                    })
                .Build();

            return tracerProvider;
        }

        public static async Task SendSpand(TracerProvider tracerProvider)
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

        //static async Task SenteProtobuf()
        //{
        //    // OTLP payload ya serializado
        //    var otlpPayload = System.IO.File.ReadAllBytes("traces.bin"); // ejemplo

        //    using var httpClient = new HttpClient();
        //    //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        //    var content = new ByteArrayContent(otlpPayload);
        //    content.Headers.ContentType = new MediaTypeHeaderValue("application/x-protobuf");

        //    var response = await httpClient.PostAsync("https://api.miempresa.com/api/telemetry/traces", content);
        //    response.EnsureSuccessStatusCode();
        //}
    }
}
