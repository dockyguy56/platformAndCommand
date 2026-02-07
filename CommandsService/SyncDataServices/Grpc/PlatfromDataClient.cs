using AutoMapper;
using CommandsService.Models;
using Grpc.Net.Client;
using PlatformService;

namespace CommandsService.SyncDataServices.Grpc
{
    public class PlatfromDataClient : IPlatfromDataCLient
    {
        private readonly IConfiguration _configuration;
        private readonly GrpcChannel _channel;
        private readonly IMapper _mapper;

        public PlatfromDataClient(IConfiguration configuration, IMapper mapper)
        {
            _configuration = configuration;
            _channel = GrpcChannel.ForAddress(_configuration["GrpcPlatform"]);
            _mapper = mapper;
        }

        public IEnumerable<Platform> ReturnAllPlatforms()
        {
            Console.WriteLine($"--> Calling GRPC Service {_configuration["GrpcPlatform"]}");
            var client = new GrpcPlatform.GrpcPlatformClient(_channel);
            var platforms = new List<Platform>();
            var request = new GetAllRequest();

            try
            {
                var response = client.GetAllPlatforms(request);
                return _mapper.Map<IEnumerable<Platform>>(response.Platform);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not get all platforms: {ex.Message}");
                return null;
            }
        }
    }
}