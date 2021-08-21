using System;

using BeatSaberModManager.Models.Implementations.Progress;


namespace BeatSaberModManager.Models.Interfaces
{
    public interface IStatusProgress : IProgress<double>, IProgress<string>, IProgress<ProgressBarStatusType> { }
}