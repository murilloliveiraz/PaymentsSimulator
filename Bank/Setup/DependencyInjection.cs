using Bank.Consumers;
using Bank.Repository;
using Bank.Repository.Interfaces;
using Bank.Services;
using BuildingBlocks.Core.EventBus.Events;
using BuildingBlocks.Core.Interfaces;

namespace Bank.Setup
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
        {
            services.AddScoped<IOutboxRepository, OutboxRepository>();
            services.AddScoped<IPaymentsRepository, PaymentsRepository>();
            services.AddScoped<IRefundRepository, RefundRepository>();
            services.AddSingleton<KafkaEventBus>();
            services.AddSingleton<IEventBus>(sp => sp.GetRequiredService<KafkaEventBus>());
            services.AddSingleton<IEventBusProducer<string>>(sp => sp.GetRequiredService<KafkaEventBus>());

            services.AddScoped<DebitService>();
            services.AddScoped<CreditService>();
            services.AddScoped<PaymentSuccessService>();
            services.AddScoped<RefundService>();

            // registra handlers
            services.AddScoped<CreditRequestHandler>();
            services.AddScoped<DebitRequestHandler>();
            services.AddScoped<PaymentSuccessHandler>();
            services.AddScoped<RefundRequestHandler>();
            return services;
        }
    }
}
