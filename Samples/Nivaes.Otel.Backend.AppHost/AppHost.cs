var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Nivaes_Otel_Backend>("nivaes-otel-backend");

builder.Build().Run();
