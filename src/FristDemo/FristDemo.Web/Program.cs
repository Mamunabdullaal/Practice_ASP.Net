using System;
using Microsoft.Extensions.Configuration;
using Serilog;


class Program
{
    static void Main(string[] args)
    {
        // Load configuration from appsettings.json
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        // Configure Serilog using the JSON configuration
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        Log.Information("This is an informational log entry.");

        Log.CloseAndFlush();
    }
}
