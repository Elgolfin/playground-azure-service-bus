using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;

namespace playground_azure_service_bus
{
    class Program
    {
        private static TelemetryClient _logger;
        
        static void Main(string[] args)
        {
            InitializeLogger();

            _logger.TrackTrace("Demo application starting up.");
            _logger.TrackEvent("Logging an event.");
            _logger.TrackException(new Exception("Demo exception."));
            _logger.TrackTrace("Demo application exiting.");
            _logger.Flush();

            Console.WriteLine("Hello World!");
        }

        static void InitializeLogger () {
            // Adding JSON file into IConfiguration.
            IConfiguration config =  new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            // Read instrumentation key from IConfiguration.
            string ikey = config["ApplicationInsights:InstrumentationKey"];
            
            TelemetryConfiguration.Active.InstrumentationKey = ikey;
            _logger = new TelemetryClient();
        }
    }
}
