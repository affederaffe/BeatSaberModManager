using System;

using BeatSaberModManager.Models.Implementations.Progress;


namespace BeatSaberModManager.Services.Interfaces
{
    /// <summary>
    /// Defines a provider for progress updates.
    /// </summary>
    public interface IStatusProgress : IProgress<double>, IProgress<ProgressInfo>;
}
