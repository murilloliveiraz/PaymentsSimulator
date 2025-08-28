using NPCI.Repository;
using NPCI.Repository.Interfaces;

namespace NPCI.Setup
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
        {
            services.AddSingleton<IOutboxRepository, OutboxRepository>();
            services.AddSingleton<IPaymentsRepository, PaymentsRepository>();
            return services;
        }
    }
}
