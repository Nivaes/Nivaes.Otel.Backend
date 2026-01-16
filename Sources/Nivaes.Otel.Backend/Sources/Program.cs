//using System.Text.Json.Serialization;
//using Microsoft.AspNetCore.Http.HttpResults;

using System.Net.Http;
using System.Threading.Tasks;

namespace Nivaes.Otel.Backend.Sources;

public class Program
{
    public static async Task Main(string[] args)
    {
        var proxyGrpc = ProxyGrpc.CrateBuilder(args);
        var proxyHttpProtobuf = ProxyHttpProtobuf.CrateBuilder(args);

        await Task.WhenAll(proxyGrpc, proxyHttpProtobuf);
    }
}
