using Bank.Context;
using Microsoft.EntityFrameworkCore;

namespace Bank
{
    public static class ApiConfig
    {
        public static IServiceCollection AddApiConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<BankContext>(options =>
                options.UseSqlite(@"Data Source=user.db"));

            return services;
        }

        public static void EnsureDatabaseCreated(this IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<BankContext>();

                context.Database.EnsureCreated();
            }
        }
    }
}