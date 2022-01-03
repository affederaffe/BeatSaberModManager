using System.Collections.Generic;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Services.Interfaces
{
    public interface IDependencyResolver
    {
        bool IsDependency(IMod modification);
        IEnumerable<IMod> ResolveDependencies(IMod modification);
        IEnumerable<IMod> UnresolveDependencies(IMod modification);
    }
}