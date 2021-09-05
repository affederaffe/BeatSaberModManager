using System;

using BeatSaberModManager.Services.Progress;


namespace BeatSaberModManager.Services.Interfaces
{
    public interface IStatusProgress : IProgress<double>, IProgress<string>, IProgress<ProgressBarStatusType> { }
}