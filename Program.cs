using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection.PortableExecutable;
using TranscriptsProcessor.Services;

namespace TranscriptsProcessor
{
    public class Program
    {
        static int Main(string[] args)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Set up Dependency Injection
            var serviceProvider = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder.AddConfiguration(configuration.GetSection("Logging"));
                    builder.AddConsole();
                })
                .BuildServiceProvider();

            var host = Host.CreateDefaultBuilder(args)
                 .ConfigureServices((hostContext, services) =>
                 {
                     ConfigureServices(services);
                 })
                 .Build();

            var app = new CommandLineApplication();
            app.HelpOption();

            var pathArg = app.Argument("path", "Path to get user data").IsRequired();

            app.OnExecute(() =>
            {
                try
                {
                    var services = host.Services.CreateScope().ServiceProvider;
                    // var service = services.GetRequiredService<Service>();
                    var logger = serviceProvider.GetService<ILogger<Program>>();
                    var service = new Service(logger);
                    service.Run(pathArg.Value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Something went wrong: " + ex.Message);
                }
            });

            

            //using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            //{
            //    builder
            //        .AddFilter("Microsoft", LogLevel.Warning)  // Set log levels
            //        .AddFilter("System", LogLevel.Warning)
            //        .AddFilter("MyConsoleApp.Program", LogLevel.Debug);
            //       // .AddConsole(); // Add console logger
            //});

            //ILogger logger = loggerFactory.CreateLogger<Program>();
            //logger.LogInformation("This is an information message");
            //logger.LogWarning("This is a warning message");

            // ILogger loggerService = LoggerFactory.CreateLogger<Service>();



            //var logger = serviceProvider.GetService<ILogger<Program>>();
            //var service = new Service(logger);
            //service.Run("");

            return app.Execute(args);

            //var scheduler = new Scheduler(logger);
            //scheduler.Start();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<Service>();
            services.AddSingleton<Scheduler>();
        }
    }
}