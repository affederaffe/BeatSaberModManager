using System;

using BeatSaberModManager.Models.Implementations.Progress;


namespace BeatSaberModManager.Services.Interfaces
{
    public interface IStatusProgress : IProgress<double>, IProgress<ProgressInfo> { }
}