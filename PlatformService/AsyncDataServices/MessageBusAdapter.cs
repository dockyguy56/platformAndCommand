using PlatformService.Dtos;

namespace PlatformService.AsyncDataServices
{
    public class MessageBusAdapter : IMessageBusClient
    {        
        private readonly Lazy<Task<IMessageBusClient>> _messageBusClient;

        public MessageBusAdapter(IConfiguration configuration)
        { 
            _messageBusClient = new Lazy<Task<IMessageBusClient>>(async () =>
            {
                return await MessageBusClient.CreateAsync(configuration);
            });
        }
        

        public async Task PublishNewPlatform(PlatformPublishedDto platformPublishedDto)
        {
            var client = await _messageBusClient.Value;
            await client.PublishNewPlatform(platformPublishedDto);
        }
    }
}