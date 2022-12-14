using ConsoleApp.Common.Mapping;
using DataAccess;
using Logic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Services;

namespace ConsoleApp;

public static class Program {
    public static void Main(string[] args)
    {
        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddJsonFile("appsettings.json", true, true).Build();
            })
            .ConfigureServices((builderContext, services) =>
            {
                services
                    .AddMappings()
                    .AddApplicationLogic()
                    .AddNucleiDbContext(builderContext.Configuration)
                    .AddPersistence()
                    .AddInfrastructure(builderContext.Configuration);
            })
            .Build();
    }
}