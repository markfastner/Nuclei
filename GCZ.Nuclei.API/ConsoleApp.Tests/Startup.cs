using ConsoleApp.Common.Mapping;
using Logic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace ConsoleApp.Tests
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true);

            Configuration = builder.Build();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMappings();
            services.AddApplicationLogic();
            services.AddInfrastructure(Configuration);
            services.AddPersistence();

            services.AddDbContext<NucleiDbContext>(options =>
                options.UseInMemoryDatabase("inMemoryDb")
            );
        }
    }
}