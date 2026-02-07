using CommandsService.Models;

namespace CommandsService.SyncDataServices.Grpc
{
    public interface IPlatfromDataCLient
    {
        IEnumerable<Platform> ReturnAllPlatforms();
    }
}