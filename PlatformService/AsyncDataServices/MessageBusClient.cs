using System.Text.Json;
using PlatformService.Dtos;
using RabbitMQ.Client;

namespace PlatformService.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient
    {
        private readonly IConfiguration _configuration;
        private readonly IConnection _connection;
        private readonly IChannel _channel;

        private MessageBusClient(IConfiguration configuration,
            IConnection connection,
            IChannel channel)
        {
            _configuration = configuration;
            _connection = connection;
            _channel = channel;
        }

        async public static Task<MessageBusClient> CreateAsync(IConfiguration configuration)
        {
            
            var factory = new ConnectionFactory() 
            { 
                HostName = configuration["RabbitMQHost"],
                Port = int.Parse(configuration["RabbitMQPort"])
            };

            IConnection connection;
            IChannel channel;
            try
            {
                connection = await factory.CreateConnectionAsync();
                channel = await connection.CreateChannelAsync();

                await channel.ExchangeDeclareAsync(exchange: "trigger", type: ExchangeType.Fanout);
                var client = new MessageBusClient(configuration, connection, channel);

                Console.WriteLine("--> Connected to Message Bus");
                return client;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not connect to the Message Bus: {ex.Message}");
            }

            return null;         
        }
        
        

        public async Task PublishNewPlatform(PlatformPublishedDto platformPublishedDto)
        {
            var message = JsonSerializer.Serialize(platformPublishedDto);

            if (_connection.IsOpen)
            {
                Console.WriteLine("--> RabbitMQ Connection Open, sending message...");
                await SendMessage(message);
            }
            else
            {
                Console.WriteLine("--> RabbitMQ connection is closed, not sending");
            }
        }

        private async Task SendMessage(string message)
        {
            var body = System.Text.Encoding.UTF8.GetBytes(message);

            await _channel.BasicPublishAsync(exchange: "trigger",
                                routingKey: "",
                                body: body);
            
            Console.WriteLine($"--> We have sent {message}");
        }

        public async Task DisposeAsync()
        {
            Console.WriteLine("MessageBus Disposed");
            if (_channel.IsOpen)
            {
                await _channel.CloseAsync();
                await _connection.CloseAsync();
            }
        }
    }
}