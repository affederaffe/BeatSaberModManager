using System.Collections.Generic;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Services.Interfaces
{
    public interface IDependencyManager
    {
        bool IsDependency(IMod mod);
        IEnumerable<IMod> ResolveDependencies(IMod mod);
        IEnumerable<IMod> UnresolveDependencies(IMod mod);
    }
}