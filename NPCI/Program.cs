using NPCI;
using NPCI.Setup;
using NPCI.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<OutboxWorker>();

builder.Services.AddApiConfig(builder.Configuration);

var host = builder.Build();
host.Run();
