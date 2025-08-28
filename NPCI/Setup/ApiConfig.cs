using Bank.Context;
using Microsoft.EntityFrameworkCore;

namespace NPCI.Setup
{
    public static class ApiConfig
    {
        public static IServiceCollection AddApiConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<NFCIContext>(options =>
                options.UseSqlite(@"Data Source=nfci.db"));

            return services;
        }

        public static void EnsureDatabaseCreated(this IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<NFCIContext>();

                context.Database.EnsureCreated();
            }
        }
    }
}