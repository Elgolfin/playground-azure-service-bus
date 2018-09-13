using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using System.Collections.Generic;

namespace playground_azure_service_bus
{
    class Program
    {
        private static TelemetryClient _logger;
        private static IConfiguration _config;
        private static IQueueClient _queueClient;
        private static IMessageReceiver _queueReceiver;

        static void Main(string[] args)
        {
            LoadConfiguration();
            
            InitializeLogger();

            _logger.TrackTrace("Demo application starting up.");
            Console.Clear();

            var quit = false;

            while (!quit) {
                
                //Console.Clear();
                Console.WriteLine("Choose an action [s] (send a message), [l] (listen and complete), [r] (listen and cancel), [q] (quit): ");
                ConsoleKeyInfo result = Console.ReadKey();
                Console.WriteLine();

                switch (result.KeyChar) {
                    case 's':
                        SendMessage(args);
                        break;
                    case 'l':
                        ListenMessage(args);
                        break;
                    default:
                        quit = true;
                        break;
                }
            }
            
            // _logger.TrackEvent("Logging an event.");

            _logger.TrackTrace("Demo application exiting.");
            _logger.Flush();
        }

        static void SendMessage (string[] args) {
            try {
                    InitializaQueueClient();
                    MainSendMessageAsync(args).GetAwaiter().GetResult();
                } catch (Exception e) {
                    _logger.TrackException(e);
                }
        }

        static void ListenMessage (string[] args) {
            try {
                    InitializaQueueReceiver();
                    MainListenMessageAsync(args).GetAwaiter().GetResult();
                } catch (Exception e) {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{e.Message}");
                    _logger.TrackException(e);
                    Console.ResetColor();
                }
        }

        static async Task MainSendMessageAsync(string[] args) {
            await SendMessageAsync();
            await _queueClient.CloseAsync();
        }

        static async Task MainListenMessageAsync(string[] args) {
            await ListenMessageAsync();
            await _queueReceiver.CloseAsync();
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

        static void InitializaQueueReceiver () {
            var serviceBusConnectionString = _config["ServiceBus:MyServiceBusConnectionString"];
            var queueName = _config["ServiceBus:MyQueueName"];
            _queueReceiver = new MessageReceiver(serviceBusConnectionString, queueName);
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

        static async Task ListenMessageAsync() {
            try
            {
               Console.WriteLine("Receiving message...");
               var message = await _queueReceiver.ReceiveAsync(TimeSpan.FromSeconds(5));
               if (message != null) {
                
                    lock (Console.Out)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine(
                            "\t\t\t\tMessage received: \n\t\t\t\t\t\tMessageId = {0}, \n\t\t\t\t\t\tSequenceNumber = {1}, \n\t\t\t\t\t\tEnqueuedTimeUtc = {2}," +
                            "\n\t\t\t\t\t\tExpiresAtUtc = {5}, \n\t\t\t\t\t\tContentType = \"{3}\", \n\t\t\t\t\t\tSize = {4},  \n\t\t\t\t\t\tContent: [ ], \n\t\t\t\t\t\tDelivery Count: {6}",
                            message.MessageId,
                            message.SystemProperties.SequenceNumber,
                            message.SystemProperties.EnqueuedTimeUtc,
                            message.ContentType,
                            message.Size,
                            message.ExpiresAtUtc,
                            message.SystemProperties.DeliveryCount);
                        Console.ResetColor();
                    }
                    //await _queueReceiver.CompleteAsync(message.SystemProperties.LockToken);
                    var properties = new Dictionary<string, object>();
                    var newDate = DateTime.UtcNow + TimeSpan.FromSeconds(10);
                    //properties.Add("ScheduledEnqueueTimeUtc", newDate);
                    properties.Add("MessageId", message.MessageId + 5);
                    await _queueReceiver.AbandonAsync(message.SystemProperties.LockToken, properties);
               }
               else {
                    Console.WriteLine($"There is no message in the queue");
               }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{e.Message}");
                _logger.TrackException(e);
                Console.ResetColor();
            }
        }
    }
}
