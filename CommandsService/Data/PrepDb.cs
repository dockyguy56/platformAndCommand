using CommandsService.Models;
using CommandsService.SyncDataServices.Grpc;

namespace CommandsService.Data
{
    public static class PrepDb
    {
        public static void PrePopulation(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var grpcClient = serviceScope.ServiceProvider.GetService<IPlatfromDataCLient>();
                var repo = serviceScope.ServiceProvider.GetService<ICommandRepo>();
                var platforms = grpcClient.ReturnAllPlatforms();

                SeedData(repo, platforms);
            }
        }

        private static void SeedData(ICommandRepo repo, IEnumerable<Platform> platforms)
        {
            Console.WriteLine("--> Seeding Data...");

            foreach (var platform in platforms)
            {
                if(!repo.ExternalPlatformExists(platform.ExternalID))
                {
                    repo.CreatePlatform(platform);
                }

                repo.SaveChanges();
            }
        }
    }
}