using System;
using System.Reactive.Subjects;

using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Progress
{
    public class StatusProgress : IStatusProgress, IDisposable
    {
        private readonly Subject<double> _progressValue = new();
        private readonly Subject<string?> _statusText = new();
        private readonly Subject<ProgressBarStatusType> _statusType = new();

        public IObservable<double> ProgressValue => _progressValue;
        public IObservable<string?> StatusText => _statusText;
        public IObservable<ProgressBarStatusType> StatusType => _statusType;

        public void Report(double value) => _progressValue.OnNext(value * 100);
        public void Report(string value) => _statusText.OnNext(value);
        public void Report(ProgressBarStatusType value) => _statusType.OnNext(value);

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _progressValue.Dispose();
            _statusText.Dispose();
            _statusType.Dispose();
        }
    }
}