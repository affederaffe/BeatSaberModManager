using System;

using BeatSaberModManager.Models.Implementations.Interfaces;


namespace BeatSaberModManager.Models.Implementations.Implementations.Progress
{
    public class StatusProgress : Progress<(double, string)>, IStatusProgress
    {
        private double _progress;
        private string _status = string.Empty;

        public void Report(double value) => base.OnReport((_progress = value, _status));
        public void Report(string value) => base.OnReport((_progress, _status = value));
    }
}