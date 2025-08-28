using BuildingBlocks.Core.EventBus.Dispatcher;
using GPay;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<KafkaEventBus>();
builder.Services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<KafkaEventBus>());
builder.Services.AddSingleton<IEventBusProducer<object>>(sp => sp.GetRequiredService<KafkaEventBus>());


// registra handlers
builder.Services.AddScoped<PaymentInitiatedHandler>();

// registra worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
