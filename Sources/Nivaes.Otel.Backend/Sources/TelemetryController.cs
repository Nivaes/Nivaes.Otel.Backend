//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;

//namespace Nivaes.Otel.Backend.Sources;

//[ApiController]
//[Route("api/[controller]")]
//public class TelemetryController : ControllerBase
//{
//    private readonly HttpClient _httpClient;

//    public TelemetryController(IHttpClientFactory factory)
//    {
//        _httpClient = factory.CreateClient();
//    }

//    [HttpPost("traces")]
//    //[Authorize] // Solo apps con token válido
//    public async Task<IActionResult> ReceiveTraces()
//    {
//        // Reenvía el cuerpo de la petición directamente al Collector
//        var content = new StreamContent(Request.Body);
//        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(/*Request.ContentType ??*/ "application/octet-stream");

//        Console.WriteLine(content);

//        var response = await _httpClient.PostAsync("http://otel-collector:4318/v1/traces", content);
//        return StatusCode((int)response.StatusCode);
//    }

//    [HttpPost("metrics")]
//    //[Authorize]
//    public async Task<IActionResult> ReceiveMetrics()
//    {
//        var content = new StreamContent(Request.Body);
//        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(Request.ContentType ?? "application/octet-stream");

//        Console.WriteLine(content);

//        var response = await _httpClient.PostAsync("http://otel-collector:4318/v1/metrics", content);
//        return StatusCode((int)response.StatusCode);
//    }
//}
