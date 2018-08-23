using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.ServiceBus;

namespace playground_azure_service_bus
{
    class Program
    {
        private static TelemetryClient _logger;
        private static IConfiguration _config;

        static void Main(string[] args)
        {
            LoadConfiguration();
            
            InitializeLogger();

            _logger.TrackTrace("Demo application starting up.");
            _logger.TrackEvent("Logging an event.");
            _logger.TrackException(new Exception("Demo exception."));
            _logger.TrackTrace("Demo application exiting.");
            _logger.Flush();

            Console.WriteLine("Hello World!");
        }

        static void LoadConfiguration() {
            // Adding JSON file into IConfiguration.
            _config =  new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();
        }

        static void InitializeLogger () {
            // Read instrumentation key from IConfiguration.
            string ikey = _config["ApplicationInsights:InstrumentationKey"];
            
            TelemetryConfiguration.Active.InstrumentationKey = ikey;
            _logger = new TelemetryClient();
        }
    }
}
