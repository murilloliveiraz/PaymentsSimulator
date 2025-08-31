using BuildingBlocks.Core.EventBus.Dispatcher;
using BuildingBlocks.Core.EventBus.Events;
using BuildingBlocks.Core.Interfaces;
using NFCI.Handlers;
using NPCI.Repository;

namespace NPCI.Setup
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
        {
            services.AddScoped<IOutboxRepository, OutboxRepository>();
            services.AddScoped<IPaymentsRepository, PaymentsRepository>();
            services.AddSingleton<KafkaEventBus>();
            services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<KafkaEventBus>());
            services.AddSingleton<IEventBusProducer<string>>(sp => sp.GetRequiredService<KafkaEventBus>());


            // registra handlers
            services.AddScoped<CreditRequestHandler>();
            services.AddScoped<CreditSuccessHandler>();
            services.AddScoped<DebitRequestHandler>();
            services.AddScoped<DebitSuccessHandler>();
            services.AddScoped<PaymentInitiatedHandler>();
            services.AddScoped<PaymentSuccessHandler>();
            services.AddScoped<PaymentFailedHandler>();
            services.AddScoped<PaymentSuccessRetryHandler>();
            return services;
        }
    }
}
