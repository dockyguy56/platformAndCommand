using CommandsService.EventProcessing;
using Confluent.Kafka;

namespace CommandsService.SyncDataServices.Kafka
{
    public class KafkaConsumer : BackgroundService
    {
        const string Topic = "platforms";
        private readonly IEventProcessor _eventProcessor;
        private readonly IConfiguration _configuration;
        private readonly ConsumerConfig _kafkaConsumerConfig;
        private readonly IConsumer<Ignore, string> _kafkaConsumer;

        public KafkaConsumer(IConfiguration configuration, IEventProcessor eventProcessor)
        {
            _eventProcessor = eventProcessor;
            _configuration = configuration;

            Enum.TryParse(_configuration["KafkaAcks"], true, out Acks acks);
            _kafkaConsumerConfig = new ConsumerConfig
            {
                BootstrapServers = _configuration["KafkaHost"],
                GroupId = "commands-service-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                Acks = acks
            };

            _kafkaConsumer = new ConsumerBuilder<Ignore, string>(_kafkaConsumerConfig).Build();
            _kafkaConsumer.Subscribe(Topic);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = _kafkaConsumer.Consume(stoppingToken);
                        Console.WriteLine($"Received message: {consumeResult.Message.Value}");
                        _eventProcessor.ProcessEvent(consumeResult.Message.Value);
                        _kafkaConsumer.Commit(consumeResult);
                    }
                    catch (OperationCanceledException)
                    {
                        // Handle cancellation gracefully
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error consuming message: {ex.Message}");
                    }
                }
            }, stoppingToken);
        }
    }
}