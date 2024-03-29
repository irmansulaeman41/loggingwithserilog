# Serilog Sample Project

Sample Project using Serilog and write to Grafana Loki.

## Project Content

![ProjectStructure1](images/ProjectStructure1.jpg?raw=true)

## Nuget Package Library

    Install-Package Serilog.AspNetCore
    Install-Package Serilog.Exceptions
    Install-Package Serilog.Sinks.Grafana.Loki

## appsettings.json

    {
        "Logging": {
            "LogLevel": {
            "Default": "Information",
            "Microsoft": "Warning",
            "Microsoft.Hosting.Lifetime": "Information"
            }
        },
        "ExternalServices": {
            "Grafana": "http://loki-distributed-distributor.monitoring:3100"
        },
        "AllowedHosts": "*"
    }

## Program.cs

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


## Reference
https://guides.github.com/features/mastering-markdown/  
https://serilog.net  
https://github.com/serilog/serilog/wiki  
https://github.com/serilog/serilog/wiki/Getting-Started  
