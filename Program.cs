using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;

namespace playground_azure_service_bus
{
    class Program
    {
        static void Main(string[] args)
        {
            // Adding JSON file into IConfiguration.
            IConfiguration config =  new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            // Read instrumentation key from IConfiguration.
            string ikey = config["ApplicationInsights:InstrumentationKey"];
            
            TelemetryConfiguration.Active.InstrumentationKey = ikey;
            TelemetryClient log = new TelemetryClient();
            log.TrackTrace("Demo application starting up.");

            for (int i = 0; i < 10; i++)
            {
                log.TrackEvent("Testing " + i);
            }

            log.TrackException(new Exception("Demo exception."));
            log.TrackTrace("Demo application exiting.");
            log.Flush();

            Console.WriteLine("Hello World!");
        }
    }
}
