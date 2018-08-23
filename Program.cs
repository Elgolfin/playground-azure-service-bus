using System;
using System.Text;
using System.Threading.Tasks;
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
        private static IQueueClient _queueClient;

        static void Main(string[] args)
        {
            LoadConfiguration();
            
            InitializeLogger();

            _logger.TrackTrace("Demo application starting up.");

            try {
                InitializaQueueClient();
                MainAsync(args).GetAwaiter().GetResult();
            } catch (Exception e) {
                _logger.TrackException(e);
            }

            
            // _logger.TrackEvent("Logging an event.");

            _logger.TrackTrace("Demo application exiting.");
            _logger.Flush();

            Console.WriteLine("Hello World!");
        }

        static async Task MainAsync(string[] args) {
            await SendMessageAsync();
            await _queueClient.CloseAsync();
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

        static void InitializaQueueClient () {
            var serviceBusConnectionString = _config["ServiceBus:MyServiceBusConnectionString"];
            var queueName = _config["ServiceBus:MyQueueName"];
            _queueClient = new QueueClient(serviceBusConnectionString, queueName);
        }

        static async Task SendMessageAsync() {
            try
            {
                // Create a new message to send to the queue
                string messageBody = $"Message {"{}"}";
                var message = new Message(Encoding.UTF8.GetBytes(messageBody));
                message.UserProperties.Add("MyProrperty", "MyValue");
                message.MessageId = "1"; // TODO: Generate a GUID ; MUST BE set when duplication is enabled

                // Write the body of the message to the console
                _logger.TrackEvent($"Sending message: {messageBody}");

                // Send the message to the queue
                await _queueClient.SendAsync(message);
            }
            catch (Exception e)
            {
                _logger.TrackException(e);
            }
        }
    }
}
