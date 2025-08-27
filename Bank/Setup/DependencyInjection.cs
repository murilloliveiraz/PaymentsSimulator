using Bank.Producers.DebitRequest;
using BuildingBlocks.Core.EventBus.Events;

namespace Bank.Setup
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
        {
            services.AddSingleton<KafkaDispatcher<string, DebitSuccessEvent>>();
            services.AddSingleton<KafkaDispatcher<string, DebitFailedEvent>>();
            services.AddSingleton<KafkaDispatcher<string, DebitSuccessEvent>>();
            services.AddSingleton<KafkaDispatcher<string, DebitRequestEvent>>();
            services.AddScoped<DebitRequestProducer>();
            return services;
        }
    }
}
