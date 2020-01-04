using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using ASS.Server.Services;
using ASS.Server.Helpers;
using System.Net.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using ASS.Server.Web;

namespace ASS.Server
{
    class Program
    {
        static void Main(string[] args)
           => CreateHostBuilder(args).Build().Run();


        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((hbc, cb) =>
                    cb.AddJsonFile("defaultConfig.json", false, false)
                    .AddJsonFile("config.json", true, false)
                    .AddEnvironmentVariables()
                    .AddCommandLine(args)
                )
                .ConfigureServices((hbc, sc) =>
                    sc.AddSingleton(sp => new GrpcService(sp)) // This service is not started.
                    .AddSingleton<InstanceService>()
                    .AddSingleton<ByondService>()
                    .AddSingleton<HttpClient>()
                )
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })

            ;
    }
}
