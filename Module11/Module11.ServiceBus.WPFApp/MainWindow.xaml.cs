using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Azure.ServiceBus;

namespace Module11.ServiceBus.WPFApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string ConnectionString
        {
            get { return (string)GetValue(ConnectionStringProperty); }
            set { SetValue(ConnectionStringProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ConnectionString.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ConnectionStringProperty =
            DependencyProperty.Register("ConnectionString", typeof(string), typeof(MainWindow), 
                new PropertyMetadata("Endpoint=sb://ewvaz204svcbus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;" +
                                     "SharedAccessKey=gAy/IjRsDNKw9bRzhjeWTqBo2jq82gMtUD118QpbTJU="));
        


        public string QueueName
        {
            get { return (string)GetValue(QueueNameProperty); }
            set { SetValue(QueueNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for QueueName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty QueueNameProperty =
            DependencyProperty.Register("QueueName", typeof(string), typeof(MainWindow), new PropertyMetadata("az204-queue"));




        public string TopicName
        {
            get { return (string)GetValue(TopicNameProperty); }
            set { SetValue(TopicNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TopicName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TopicNameProperty =
            DependencyProperty.Register("TopicName", typeof(string), typeof(MainWindow), new PropertyMetadata("az-204sbtopic"));



        public int MessageGenerationLevel
        {
            get { return (int)GetValue(MessageGenerationLevelProperty); }
            set { SetValue(MessageGenerationLevelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MessageGenerationLevel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessageGenerationLevelProperty =
            DependencyProperty.Register("MessageGenerationLevel", typeof(int), typeof(MainWindow), new PropertyMetadata(0, OnMessageGenerationLevelChanged));

        private static void OnMessageGenerationLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = (MainWindow)d;
            window.MessagesPerSecond = (long)Math.Pow(2, window.MessageGenerationLevel);
        }


        public long MessagesPerSecond
        {
            get { return (long)GetValue(MessagesPerSecondProperty); }
            set { SetValue(MessagesPerSecondProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MessagesPerSecond.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessagesPerSecondProperty =
            DependencyProperty.Register("MessagesPerSecond", typeof(long), typeof(MainWindow), new PropertyMetadata());





        public double ReceivedMessagesPerSecond
        {
            get { return (double)GetValue(ReceivedMessagesPerSecondProperty); }
            set { SetValue(ReceivedMessagesPerSecondProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ReceivedMessagesPerSecond.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReceivedMessagesPerSecondProperty =
            DependencyProperty.Register("ReceivedMessagesPerSecond", typeof(double), typeof(MainWindow), new PropertyMetadata());




        public string ConnectionStatus
        {
            get { return (string)GetValue(ConnectionStatusProperty); }
            set { SetValue(ConnectionStatusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ConnectionStatus.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ConnectionStatusProperty =
            DependencyProperty.Register("ConnectionStatus", typeof(string), typeof(MainWindow), new PropertyMetadata(""));


        DispatcherTimer _timer = new DispatcherTimer();

        DateTime previousTick;
        double rounded;

        public ObservableCollection<Message> RecentReceivedMessages { get; } = new ObservableCollection<Message>();

        
        private QueueClient _queueClient;
        private TopicClient _topicClient;

        public MainWindow()
        {
            InitializeComponent();

            MessagesPerSecond = (long)Math.Pow(2, MessageGenerationLevel);
            _timer.Interval = TimeSpan.FromMilliseconds(0.0001f);
            _timer.Tick += TimerOnTick;
        }


        private void ConnectButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _queueClient = new QueueClient(ConnectionString, QueueName, ReceiveMode.ReceiveAndDelete);
                //_topicClient = new TopicClient(ConnectionString, TopicName);
                previousTick = DateTime.Now;
                _timer.Start();
                ConnectionStatus = "Connected";
            }
            catch
            {
                if (_timer.IsEnabled)
                    _timer.Stop();
                ConnectionStatus = "Error";
                _queueClient = null;
                _topicClient = null;
            }
        }
        private async void TimerOnTick(object? sender, EventArgs e)
        {
            // calculate time passed since previous tick
            var now = DateTime.Now;
            var dTime = now - previousTick;
            previousTick = now;

            // calculate how many messages we would like to send at this time
            var rawMessageCount = rounded + dTime.TotalSeconds * MessagesPerSecond;
            var messageCount = (int)Math.Floor(rawMessageCount);

            // since we have to round down, store the left over for next round
            rounded = rawMessageCount - messageCount;

            if (messageCount > 0)
            {
                await SendMessagesAsync(_queueClient, messageCount);
            }

            //var messages = await ReceiveMessagesAsync(_queueClient);

        }

        static async Task SendMessagesAsync(QueueClient queueClient, int numberOfMessagesToSend)
        {
            try
            {

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
        }

        
        /*
        static Task<Message> ReceiveMessagesAsync(QueueClient queueClient)
        {
            await using (ServiceBusClient client = new ServiceBusClient(connectionString))
            {
                // create a processor that we can use to process the messages
                ServiceBusProcessor processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());

                // add handler to process messages
                processor.ProcessMessageAsync += MessageHandler;

                // add handler to process any errors
                processor.ProcessErrorAsync += ErrorHandler;

                // start processing 
                await processor.StartProcessingAsync();

                Console.WriteLine("Wait for a minute and then press any key to end the processing");
                Console.ReadKey();

                // stop processing 
                Console.WriteLine("\nStopping the receiver...");
                await processor.StopProcessingAsync();
                Console.WriteLine("Stopped receiving messages");
            }
        }*/

        static async Task SendEventsAsync()
        {
            TopicClient topicClient = null;

            try
            {

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
