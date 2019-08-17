using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ASS.Server.Services;
using ASS.Server.Helpers;
using System.Net.Http;

namespace ASS.Server
{
    class Program
    {
        static void Main(string[] args)
           => new Program().MainAsync(args).GetAwaiter().GetResult();


        public async Task MainAsync(string[] args)
        {
            
            using (var services = ConfigureServices(args))
            {
                Grpc.Core.GrpcEnvironment.SetLogger(new GrpcLoggingWraper(services.GetRequiredService<ILogger<GrpcService>>()));
                var grpc = services.GetRequiredService<GrpcService>();
                grpc.Initialize();
                await Task.Delay(-1);
            }
        }

        private ServiceProvider ConfigureServices(string[] args)
        {
            // Stage 1 services
            var services = new ServiceCollection()
                .AddSingleton<IConfiguration>(sp =>
                    new ConfigurationBuilder()
                    .AddJsonFile("defaultConfig.json", false, false)
                    .AddJsonFile("config.json", true, false)
                    .AddEnvironmentVariables()
                    .AddCommandLine(args)
                    .Build()
                );
            var serviceProvider = services.BuildServiceProvider();
            // Stage 2 services
            services
                .AddLogging(c =>
                {
                    c.ClearProviders();
                    c.AddConsole();
                    c.AddConfiguration(serviceProvider.GetRequiredService<IConfiguration>().GetSection("Logging"));
                })
                .AddSingleton(sp => new GrpcService(sp))
                .AddSingleton<InstanceService>()
                .AddSingleton<HttpClient>();
                ;
            return services.BuildServiceProvider();
        }
    }
}
