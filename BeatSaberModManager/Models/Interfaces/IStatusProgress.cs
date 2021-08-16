using System;


namespace BeatSaberModManager.Models.Interfaces
{
    public interface IStatusProgress : IProgress<double>, IProgress<string> { }
}