using ConquerSite.Models.DTOs;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace ConquerSite
{
    public class GlobalSettingsInitializer : IHostedService
    {
        private readonly GlobalSettings _settings;

        public GlobalSettingsInitializer(GlobalSettings settings)
        {
            _settings = settings;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _settings.Init();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

}
