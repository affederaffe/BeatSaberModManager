using System.Collections.Generic;

using BeatSaberModManager.Models.Interfaces;
using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.DependencyManagement
{
    /// <inheritdoc />
    public class SimpleDependencyResolver : IDependencyResolver
    {
        private readonly IModProvider _modProvider;
        private readonly Dictionary<IMod, HashSet<IMod>> _dependencyRegistry;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleDependencyResolver"/> class.
        /// </summary>
        public SimpleDependencyResolver(IModProvider modProvider)
        {
            _modProvider = modProvider;
            _dependencyRegistry = new Dictionary<IMod, HashSet<IMod>>();
        }

        /// <inheritdoc />
        public bool IsDependency(IMod modification) =>
            _dependencyRegistry.TryGetValue(modification, out HashSet<IMod>? dependents) && dependents.Count != 0;

        /// <inheritdoc />
        public IEnumerable<IMod> ResolveDependencies(IMod modification)
        {
            foreach (IMod dependency in _modProvider.GetDependencies(modification))
            {
                if (_dependencyRegistry.TryGetValue(dependency, out HashSet<IMod>? dependents)) dependents.Add(modification);
                else _dependencyRegistry.Add(dependency, new HashSet<IMod> { modification });
                yield return dependency;
            }
        }

        /// <inheritdoc />
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
