using PlatformService.Dtos;

namespace PlatformService.SyncDataServices.Kafka
{
    public interface IKafkaProducer
    {
        Task PublishNewPlatform(PlatformPublishedDto platformPublishedDto);
    }
}