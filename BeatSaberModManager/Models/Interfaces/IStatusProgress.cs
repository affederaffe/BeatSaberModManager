using System;


namespace BeatSaberModManager.Models.Implementations.Interfaces
{
    public interface IStatusProgress : IProgress<double>, IProgress<string> { }
}