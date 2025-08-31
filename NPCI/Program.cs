using NPCI;
using NPCI.Setup;
using NPCI.Workers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<Worker>();
builder.Services.AddHostedService<OutboxWorker>();

builder.Services.AddApiConfig(builder.Configuration);
builder.Services.AddDependencyInjection();

var host = builder.Build();

host.Services.EnsureDatabaseCreated();

host.Run();
