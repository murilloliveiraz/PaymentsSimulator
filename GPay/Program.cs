using BuildingBlocks.Core.EventBus.Dispatcher;
using GPay;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<KafkaEventBus>();
builder.Services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<KafkaEventBus>());
builder.Services.AddSingleton<IEventBusProducer<string>>(sp => sp.GetRequiredService<KafkaEventBus>());


// registra handlers
builder.Services.AddScoped<PaymentInitiatedHandler>();
builder.Services.AddScoped<PaymentFailedHandler>();
builder.Services.AddScoped<PaymentSuccessHandler>();

// registra worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
