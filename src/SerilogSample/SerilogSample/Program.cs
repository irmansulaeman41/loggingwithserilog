using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;
using System;
using System.Collections.Generic;

namespace SerilogSample
{
    public class Program
    {

        private const string OutputTemplate =
            "{Timestamp:dd-MM-yyyy HH:mm:ss} [{Level:u3}] [{ThreadId}] {Message}{NewLine}{Exception}";

        public static int Main(string[] args)
        {
            //configure logging first
            ConfigureLogging();

            try
            {
                Log.Information("Starting web host");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void ConfigureLogging()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("config/appsettings.json", optional: true, reloadOnChange: true)
                .Build();
            var grafanaurl = configuration["ExternalAPI:Grafana"];
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Verbose()
               .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
               .Enrich.FromLogContext()
               .WriteTo.Console()
               .WriteTo.GrafanaLoki(
                   grafanaurl,
                   labels: new List<LokiLabel> { new() { Key = "app", Value = "SerilogSampleApps" } },
                   credentials: null,
                   outputTemplate: OutputTemplate)
               .CreateLogger();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(c => {
                    c.AddJsonFile("config/appsettings.json", optional: true, reloadOnChange: true);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
