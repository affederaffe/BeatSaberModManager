using System.Collections.Generic;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Services.Interfaces
{
    /// <summary>
    /// Provides methods to manage dependencies of <see cref="IMod"/>s.
    /// </summary>
    public interface IDependencyResolver
    {
        /// <summary>
        /// Checks if other <see cref="IMod"/>s depend on <paramref name="modification"/>.
        /// </summary>
        /// <param name="modification">The <see cref="IMod"/> to check.</param>
        /// <returns>true if <paramref name="modification"/> is a dependency, false otherwise.</returns>
        bool IsDependency(IMod modification);

        /// <summary>
        /// Resolves all dependencies for <paramref name="modification"/> and adds it as an dependent.
        /// </summary>
        /// <param name="modification">The <see cref="IMod"/> to resolve the dependencies for.</param>
        /// <returns>All affected dependencies of <paramref name="modification"/>.</returns>
        IEnumerable<IMod> ResolveDependencies(IMod modification);

        /// <summary>
        /// Resolves all dependencies for <paramref name="modification"/> and removes it as an dependent.
        /// </summary>
        /// <param name="modification">The <see cref="IMod"/> to unresolve the dependencies for.</param>
        /// <returns>All affected dependencies of <paramref name="modification"/>.</returns>
        IEnumerable<IMod> UnresolveDependencies(IMod modification);
    }
}