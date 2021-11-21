using System;

using BeatSaberModManager.Services.Implementations.Progress;


namespace BeatSaberModManager.Services.Interfaces
{
    public interface IStatusProgress : IProgress<double>, IProgress<string>, IProgress<ProgressBarStatusType>
    {
        
    }
}