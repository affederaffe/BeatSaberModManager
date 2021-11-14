using System.Collections.Generic;

using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.DependencyManagement
{
    public class DependencyManager : IDependencyManager
    {
        private readonly IModProvider _modProvider;
        private readonly Dictionary<IMod, HashSet<IMod>> _dependencyRegistry;

        public DependencyManager(IModProvider modProvider)
        {
            _modProvider = modProvider;
            _dependencyRegistry = new Dictionary<IMod, HashSet<IMod>>();
        }

        public bool IsDependency(IMod mod) =>
            _dependencyRegistry.TryGetValue(mod, out HashSet<IMod>? dependents) && dependents.Count != 0;

        public IEnumerable<IMod> ResolveDependencies(IMod mod)
        {
            foreach (IMod dependency in _modProvider.GetDependencies(mod))
            {
                if (_dependencyRegistry.TryGetValue(dependency, out HashSet<IMod>? dependents)) dependents.Add(mod);
                else _dependencyRegistry.Add(dependency, new HashSet<IMod> { mod });
                yield return dependency;
            }
        }

        public IEnumerable<IMod> UnresolveDependencies(IMod mod)
        {
            foreach (IMod dependency in _modProvider.GetDependencies(mod))
            {
                if (!_dependencyRegistry.TryGetValue(dependency, out HashSet<IMod>? dependents)) continue;
                dependents.Remove(mod);
                yield return dependency;
            }
        }
    }
}