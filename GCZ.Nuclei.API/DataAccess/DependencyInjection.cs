using Logic.Common.Interfaces.Persistence;
using DataAccess.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using DataAccess.Persistence.Common;
using Logic.Common.Interfaces.Persistence.Common;

namespace DataAccess
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddNucleiDbContext(
            this IServiceCollection services, 
            IConfiguration configuration
        )
        {
            services.AddDbContext<NucleiDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")!)
            );

            return services;
        }

        public static IServiceCollection AddPersistence(this IServiceCollection services)
        {
            //dependency injection for persistence repositories
            services.AddTransient<IEFCoreLoggingRepository, EFCoreLoggingRepository>();
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }
}
