using Logic.Common.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Logic
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationLogic(this IServiceCollection services)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CancellationTokenBehavior<,>));
            services.AddMediatR(typeof(DependencyInjection).Assembly);

            return services;
        }
    }
}
