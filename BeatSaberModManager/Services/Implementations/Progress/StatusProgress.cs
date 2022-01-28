using System;
using System.Reactive.Subjects;

using BeatSaberModManager.Models.Implementations.Progress;
using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.Progress
{
    public sealed class StatusProgress : IStatusProgress, IDisposable
    {
        private readonly Subject<double> _progressValue = new();
        private readonly Subject<ProgressInfo> _progressInfo = new();

        public IObservable<double> ProgressValue => _progressValue;

        public IObservable<ProgressInfo> ProgressInfo => _progressInfo;

        public void Report(double value) => _progressValue.OnNext(value * 100);

        public void Report(ProgressInfo value) => _progressInfo.OnNext(value);

        public void Dispose()
        {
            _progressValue.Dispose();
            _progressInfo.Dispose();
        }
    }
}