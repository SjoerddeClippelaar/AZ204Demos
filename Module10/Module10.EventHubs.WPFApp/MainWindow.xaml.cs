using System;
using System.Collections.Concurrent;
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
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;

namespace Module10.EventHubs.WPFApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {



        public string EventHubConnectionString
        {
            get { return (string)GetValue(EventHubConnectionStringProperty); }
            set { SetValue(EventHubConnectionStringProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ConnectionString.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EventHubConnectionStringProperty =
            DependencyProperty.Register("EventHubConnectionString", typeof(string), typeof(MainWindow), 
                new PropertyMetadata("Endpoint=sb://ewv-module10-ehns.servicebus.windows.net/;SharedAccessKeyName=phoneanalysis-sap;" +
                                     "SharedAccessKey=gN2kc1H3y6GF3VlUZcT9eA4ABreETgsOqiLcuaMSqmI=;"));





        public string EventHubName
        {
            get { return (string)GetValue(EventHubNameProperty); }
            set { SetValue(EventHubNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EventHubName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EventHubNameProperty =
            DependencyProperty.Register("EventHubName", typeof(string), typeof(MainWindow), new PropertyMetadata("phoneanalysis"));




        public string StorageConnectionString
        {
            get { return (string)GetValue(StorageConnectionStringProperty); }
            set { SetValue(StorageConnectionStringProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ConnectionString.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StorageConnectionStringProperty =
            DependencyProperty.Register("StorageConnectionString", typeof(string), typeof(MainWindow), 
                new PropertyMetadata("DefaultEndpointsProtocol=https;EndpointSuffix=core.windows.net;AccountName=ewvaz204mod10capturestor;" +
                                     "AccountKey=yowiS5HCajJmcObGZmKB/vs3ShCx+00GNsBGCBGcPEgxDGSdikftjQ6cfQa8SBqPFu2wiuWALH3Rg1Qe5i9NKQ=="));



        public string StorageContainerName
        {
            get { return (string)GetValue(StorageContainerNameProperty); }
            set { SetValue(StorageContainerNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EventHubName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StorageContainerNameProperty =
            DependencyProperty.Register("StorageContainerName", typeof(string), typeof(MainWindow), new PropertyMetadata("wpfapp"));

        public static ConcurrentQueue<EventItem> Queue = new ConcurrentQueue<EventItem>();

        public ObservableCollection<EventItem> Events { get; } = new ObservableCollection<EventItem>();

        private DispatcherTimer Timer { get; } = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            Timer.Tick += OnTimerTick;
            Timer.Interval = TimeSpan.FromMilliseconds(10);
            Timer.Start();

        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            while (Queue.TryDequeue(out var result))
            {
                Events.Add(result);
            }
        }

        private async void ConnectButton_OnClick(object sender, RoutedEventArgs args)
        {
            /*
            Parameters:
            hostName: The name of the Microsoft.ServiceBus.Messaging.EventProcessorHost instance. This name must be unique for each instance of the host.
            eventHubPath: The path to the Event Hub from which to start receiving event data.
            consumerGroupName: The name of the Event Hubs consumer group from which to start receiving event data.
            eventHubConnectionString: The connection string for the Event Hub.
            storageConnectionString: The connection string for the Azure Blob storage account to use for partition distribution.
            leaseContainerName: The name of the Azure Blob container in which all lease blobs are created. If this parameter is not supplied, then the Event Hubs path is used as the name of the Azure Blob container.
            leaseBlobPrefix: The prefix of lease blob files.
             */

            var ready = true;

            if (string.IsNullOrWhiteSpace(EventHubName))
                ready = false;
            if (string.IsNullOrWhiteSpace(EventHubConnectionString))
                ready = false;

            if (string.IsNullOrWhiteSpace(StorageConnectionString))
                ready = false;
            if (string.IsNullOrWhiteSpace(StorageContainerName))
                ready = false;

            if (!ready)
            {
                MessageBox.Show("One or more necessary settings missing");
                return;
            }

            try
            {
                var eventProcessorHost = new EventProcessorHost(
                    "host",
                    EventHubName,
                    PartitionReceiver.DefaultConsumerGroupName,
                    EventHubConnectionString,
                    StorageConnectionString,
                    StorageContainerName);

                await eventProcessorHost.RegisterEventProcessorAsync<SimpleEventProcessor>();

                ConnectButton.IsEnabled = false;
                MessageBox.Show("Connection successful");
            }
            catch (Exception e)
            {
                MessageBox.Show($"Exception trying to connect to EventHub: {e.Message}");
            }
        }

    }


    public class SimpleEventProcessor : IEventProcessor
    {
        public async Task OpenAsync(PartitionContext context)
        {
            
        }

        public async Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            
        }

        public Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (var message in messages)
            {
                MainWindow.Queue.Enqueue(new EventItem(context.PartitionId, message));
            }

            return context.CheckpointAsync();
        }

        public async Task ProcessErrorAsync(PartitionContext context, Exception error)
        {
            
        }
    }

    public class EventItem
    {
        public string PartitionId { get; }
        public DateTime Timestamp { get; }
        public string Data { get; }

        public EventItem(string partitionId, EventData data)
        {
            PartitionId = partitionId;
            Data = Encoding.UTF8.GetString(data.Body.Array, data.Body.Offset, data.Body.Count);
            Timestamp = DateTime.Now;

            //Console.WriteLine($"Message received. Partition: '{context.PartitionId}', Data: '{data}'");

        }
    }

}
