using System.Threading;
using System.Threading.Tasks;

using BeatSaberModManager.Models.Implementations.Settings;
using BeatSaberModManager.Services.Interfaces;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BeatSaberModManager
{
    public class Startup : IHostedService
    {
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IInstallDirValidator _installDirValidator;
        private readonly IInstallDirLocator _installDirLocator;

        public Startup(IOptions<AppSettings> appSettings, IInstallDirValidator installDirValidator, IInstallDirLocator installDirLocator)
        {
            _appSettings = appSettings;
            _installDirValidator = installDirValidator;
            _installDirLocator = installDirLocator;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!_installDirValidator.ValidateInstallDir(_appSettings.Value.InstallDir.Value))
                _appSettings.Value.InstallDir.Value = await _installDirLocator.LocateInstallDirAsync();
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}