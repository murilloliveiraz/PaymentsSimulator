using GPay;
using GPay.PaymentInitiated;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
builder.Services.AddSingleton<PaymentInitiatedConsumer>();
builder.Services.AddSingleton<PaymentInitiatedProducer>();


var host = builder.Build();
host.Run();
