using System.Text.Json;
using Confluent.Kafka;
using PlatformService.Dtos;

namespace PlatformService.SyncDataServices.Kafka
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly IConfiguration _configuration;
        private readonly ProducerConfig _kafkaConfig;
        private readonly IProducer<Null, string> _producer;

        public KafkaProducer(IConfiguration configuration)
        {
            _configuration = configuration;
            Enum.TryParse<Acks>(_configuration["KafkaAcks"], out var acks);
            
            _kafkaConfig = new ProducerConfig
            {
                BootstrapServers = _configuration["KafkaHost"],
                Acks = acks
            };

            _producer = new ProducerBuilder<Null, string>(_kafkaConfig).Build();
        }

        public Task PublishNewPlatform(PlatformPublishedDto platformPublishedDto)
        {
            var message = JsonSerializer.Serialize(platformPublishedDto);

            _producer.Produce("platforms", new Message<Null, string> { Value = message }, (deliveryReport) =>
            {
                if (deliveryReport.Error.IsError)
                {
                    Console.WriteLine($"Failed to deliver message: {deliveryReport.Error.Reason}");
                }
                else
                {
                    Console.WriteLine($"Message delivered to {deliveryReport.TopicPartitionOffset}");
                }
            });

            _producer.Flush(TimeSpan.FromSeconds(10));
            return Task.CompletedTask;
        }
    }
}