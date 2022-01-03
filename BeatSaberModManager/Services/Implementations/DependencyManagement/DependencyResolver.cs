using System.Collections.Generic;

using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.DependencyManagement
{
    public class DependencyResolver : IDependencyResolver
    {
        private readonly IModProvider _modProvider;
        private readonly Dictionary<IMod, HashSet<IMod>> _dependencyRegistry;

        public DependencyResolver(IModProvider modProvider)
        {
            _modProvider = modProvider;
            _dependencyRegistry = new Dictionary<IMod, HashSet<IMod>>();
        }

        public bool IsDependency(IMod modification) =>
            _dependencyRegistry.TryGetValue(modification, out HashSet<IMod>? dependents) && dependents.Count != 0;

        public IEnumerable<IMod> ResolveDependencies(IMod modification)
        {
            foreach (IMod dependency in _modProvider.GetDependencies(modification))
            {
                if (_dependencyRegistry.TryGetValue(dependency, out HashSet<IMod>? dependents)) dependents.Add(modification);
                else _dependencyRegistry.Add(dependency, new HashSet<IMod> { modification });
                yield return dependency;
            }
        }

        public IEnumerable<IMod> UnresolveDependencies(IMod modification)
        {
            foreach (IMod dependency in _modProvider.GetDependencies(modification))
            {
                if (!_dependencyRegistry.TryGetValue(dependency, out HashSet<IMod>? dependents)) continue;
                dependents.Remove(modification);
                yield return dependency;
            }
        }
    }
}