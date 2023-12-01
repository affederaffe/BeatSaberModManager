using System.Collections.Generic;

using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.DependencyManagement
{
    /// <inheritdoc />
    public class SimpleDependencyResolver(IModProvider modProvider) : IDependencyResolver
    {
        private readonly Dictionary<IMod, HashSet<IMod>> _dependencyRegistry = new();

        /// <inheritdoc />
        public bool IsDependency(IMod modification) => _dependencyRegistry.TryGetValue(modification, out HashSet<IMod>? dependents) && dependents.Count != 0;

        /// <inheritdoc />
        public IEnumerable<IMod> ResolveDependencies(IMod modification)
        {
            HashSet<IMod> dependencies = new();
            ResolveDependencies(modification, dependencies);
            return dependencies;
        }

        /// <inheritdoc />
        public IEnumerable<IMod> UnresolveDependencies(IMod modification)
        {
            HashSet<IMod> dependencies = new();
            UnresolveDependencies(modification, dependencies);
            return dependencies;
        }

        private void ResolveDependencies(IMod modification, HashSet<IMod> dependencies)
        {
            foreach (IMod dependency in modProvider.GetDependencies(modification))
            {
                if (_dependencyRegistry.TryGetValue(dependency, out HashSet<IMod>? dependents))
                    dependents.Add(modification);
                else
                    _dependencyRegistry.Add(dependency, new HashSet<IMod> { modification });
                dependencies.Add(dependency);
                ResolveDependencies(dependency, dependencies);
            }
        }

        private void UnresolveDependencies(IMod modification, HashSet<IMod> dependencies)
        {
            foreach (IMod dependency in modProvider.GetDependencies(modification))
            {
                if (!_dependencyRegistry.TryGetValue(dependency, out HashSet<IMod>? dependents))
                    continue;
                dependents.Remove(modification);
                dependencies.Add(dependency);
                if (dependencies.Count == 0)
                    UnresolveDependencies(dependency, dependencies);
            }
        }
    }
}
