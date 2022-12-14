using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Logic.Tests
{
    public class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            var config = TypeAdapterConfig.GlobalSettings;

            config.Scan(Assembly.GetExecutingAssembly());

            services.AddSingleton(config);
            services.AddScoped<IMapper, ServiceMapper>();
        }
    }
}