using System.Text;
using CommandsService.EventProcessing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CommandsService.AsyncDataServices
{
    public class MessageBusSuscriber : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly IEventProcessor _eventProcessor;
        private IConnection _connection;
        private IChannel _channel;
        private string _queueName;
        

        public MessageBusSuscriber(IConfiguration configuration,
            IEventProcessor eventProcessor)
        {
             _configuration = configuration;
             _eventProcessor = eventProcessor;
        }

        // private MessageBusSuscriber(IConfiguration configuration,
        //  IEventProcessor eventProcessor,
        //  IChannel channel,
        //  IConnection connection,
        //  string queueName)
        //  {
        //      _configuration = configuration;
        //      _eventProcessor = eventProcessor;
        //      _channel = channel;
        //      _connection = connection;
        //      _queueName = queueName;
        //  }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            { 
                (IChannel channel, string queueName, IConnection connection) = (await CreateAsync(_configuration, _eventProcessor)).GetValueOrDefault();
                _channel = channel;
                _queueName = queueName;
                _connection = connection;

                Console.WriteLine($"--> Service Bus sucriber quename: {_queueName}");
            }).Wait();
            return base.StartAsync(cancellationToken);
        }

        async public static Task<(IChannel, string, IConnection)?> CreateAsync(IConfiguration configuration, IEventProcessor eventProcessor)
        {
            var factory = new ConnectionFactory()
                { 
                    HostName = configuration["RabbitMQHost"],
                    Port = int.Parse(configuration["RabbitMQPort"]) 
                };

            try
            {
                IConnection connection = await factory.CreateConnectionAsync();
                IChannel channel = await connection.CreateChannelAsync();
                await channel.ExchangeDeclareAsync(exchange: "trigger", type: ExchangeType.Fanout);
                string queueName = (await channel.QueueDeclareAsync()).QueueName;
                await channel.QueueBindAsync(queue: queueName, exchange: "trigger", routingKey: "");
                Console.WriteLine("--> Listening on the Message Bus...");

                // var subscriber = new MessageBusSuscriber(configuration, eventProcessor, channel, connection, queueName);
                // return subscriber;
                return (channel, queueName, connection);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not connect to the Message Bus: {ex.Message}");
            }

            return null;
        }

        public async Task DisposeAsync()
        {
            Console.WriteLine("MessageBusSuscriber Disposed");
            if (_channel.IsOpen)
            {
                await _channel.CloseAsync();
                await _connection.CloseAsync();
            }

            base.Dispose();
        }

        async protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            Console.WriteLine("--> Executing Message Bus Subscriber");
            try
            {
                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += (ModuleHandle, ea) =>
                {
                    Console.WriteLine("--> Event Received!");

                    var body = ea.Body.ToArray();
                    var notificationMessage = Encoding.UTF8.GetString(body);
                    Console.WriteLine($" [x] {notificationMessage}");
                    _eventProcessor.ProcessEvent(notificationMessage);

                    return Task.CompletedTask;
                };

                await _channel.BasicConsumeAsync(queue: _queueName, autoAck: true, consumer: consumer);
            } 
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not subscribe to the Message Bus: {ex.Message}");
            }
        }
    }
}