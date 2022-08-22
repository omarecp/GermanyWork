using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestfullService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // set the base path (logging, ef, ...)
            Environment.CurrentDirectory = AppContext.BaseDirectory;

            try
            {
                CreateHostBuilder(args)
                    .Build()
                    .Run();
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>

            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostContext, builder) =>
            {
                var environment = hostContext.HostingEnvironment;

                builder
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", true, false) // overwrites previous values
                    .AddEnvironmentVariables(); // overwrites previous values
                if (args != null)
                {
                    builder.AddCommandLine(args);
                }
            })

                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
