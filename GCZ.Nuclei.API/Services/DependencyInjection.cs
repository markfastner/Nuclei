using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services.Utilities;
using Logic.Common.Interfaces.Utilities;
using Logic.Common.Interfaces.Auth;
using Services.Auth;

namespace Services
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.AddAuth();

            //dependency injection for utilities
            services.AddTransient<IPasswordHasher, PasswordHasher>();

            return services;
        }
    
        public static IServiceCollection AddAuth(
            this IServiceCollection services
        )
        {
            services.AddSingleton<IAuthProvider, AuthProvider>();

            return services;
        }
    }
}
