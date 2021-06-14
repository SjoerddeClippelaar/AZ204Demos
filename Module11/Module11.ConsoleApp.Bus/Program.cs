using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyConsoleCore;
using Microsoft.Azure.ServiceBus;

namespace Module11.ConsoleApp
{
    class Program
    {

        const string ServiceBusName = "";
        const string ServiceBusSAKey = "";
        static readonly string ServiceBusConnectionString = $"Endpoint=sb://{ServiceBusName}.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey={ServiceBusSAKey}";
        const string QueueName = "az204-queue";
        const string TopicName = "az-204sbtopic";



        public static async Task Main(string[] args)
        {

            var menu = new Menu();
            
            menu.Add("Send 10 messages to Service Bus Queue", () => SendMessagesAsync(10).Wait());
            menu.Add("Send event to Service Bus Topic", () => SendEventsAsync().Wait());
            menu.Display();

            Console.ReadKey();

        }

        static async Task SendMessagesAsync(int numberOfMessagesToSend)
        {
            QueueClient queueClient = null;
            try
            {
                queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

                for (var i = 0; i < numberOfMessagesToSend; i++)
                {
                    // Create a new message to send to the queue.
                    string messageBody = $"Message {i}";
                    var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                    // Write the body of the message to the console.
                    Console.WriteLine($"Sending message: {messageBody}");

                    // Send the message to the queue.
                    await queueClient.SendAsync(message);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }
            finally
            {
                if (queueClient != null)
                    await queueClient.CloseAsync();
            }
        }

        static async Task SendEventsAsync()
        {
            TopicClient topicClient = null;

            try
            {
                topicClient = new TopicClient(ServiceBusConnectionString, TopicName);

                var messageBody = "Event body";
                var message = new Message(Encoding.UTF8.GetBytes(messageBody));

                Console.WriteLine($"Sending event: {messageBody}");

                await topicClient.SendAsync(message);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                if (topicClient != null)
                    await topicClient.CloseAsync();
            }
        }
    }
}
