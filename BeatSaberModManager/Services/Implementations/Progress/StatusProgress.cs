using System;
using System.Reactive.Subjects;

using BeatSaberModManager.Models.Implementations.Progress;
using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.Progress
{
    public sealed class StatusProgress : IStatusProgress, IDisposable
    {
        private readonly Subject<double> _progressValue = new();
        private readonly ISubject<double> _progressValueSync;
        private readonly Subject<ProgressInfo> _progressInfo = new();
        private readonly ISubject<ProgressInfo> _progressInfoSync;

        public StatusProgress()
        {
            _progressValueSync = Subject.Synchronize(_progressValue);
            _progressInfoSync = Subject.Synchronize(_progressInfo);
        }

        public IObservable<double> ProgressValue => _progressValue;

        public IObservable<ProgressInfo> ProgressInfo => _progressInfo;

        public void Report(double value) => _progressValueSync.OnNext(value * 100);

        public void Report(ProgressInfo value) => _progressInfoSync.OnNext(value);

        public void Dispose()
        {
            _progressValue.Dispose();
            _progressInfo.Dispose();
        }
    }
}