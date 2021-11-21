using System.Collections.Generic;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Services.Interfaces
{
    public interface IDependencyResolver
    {
        bool IsDependency(IMod mod);
        IEnumerable<IMod> ResolveDependencies(IMod mod);
        IEnumerable<IMod> UnresolveDependencies(IMod mod);
    }
}