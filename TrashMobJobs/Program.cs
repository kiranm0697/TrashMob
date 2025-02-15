
namespace TrashMobJobs
{
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using TrashMob.Shared;
    using TrashMob.Shared.Persistence;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;

    public class Program
    {
        public static async Task Main()
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration(c =>
                {
                    c.AddEnvironmentVariables();
                })
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services =>
                {
                    ServiceBuilder.AddManagers(services);
                    ServiceBuilder.AddRepositories(services);
                    services.AddDbContext<MobDbContext>();
                })
                .Build();

            await host.RunAsync().ConfigureAwait(false);
        }
    }
}