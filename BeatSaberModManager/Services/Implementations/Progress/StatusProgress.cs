using System;
using System.Reactive.Subjects;

using BeatSaberModManager.Models.Implementations.Progress;
using BeatSaberModManager.Services.Interfaces;


namespace BeatSaberModManager.Services.Implementations.Progress
{
    /// <inheritdoc cref="BeatSaberModManager.Services.Interfaces.IStatusProgress" />
    public sealed class StatusProgress : IStatusProgress, IDisposable
    {
        private readonly Subject<double> _progressValue = new();
        private readonly Subject<ProgressInfo> _progressInfo = new();

        /// <summary>
        /// Signals when the progress value changes.
        /// </summary>
        public IObservable<double> ProgressValue => _progressValue;

        /// <summary>
        /// Signals when the progress info changes.
        /// </summary>
        public IObservable<ProgressInfo> ProgressInfo => _progressInfo;

        /// <inheritdoc />
        public void Report(double value) => _progressValue.OnNext(value * 100);

        /// <inheritdoc />
        public void Report(ProgressInfo value) => _progressInfo.OnNext(value);

        /// <inheritdoc />
        public void Dispose()
        {
            _progressValue.Dispose();
            _progressInfo.Dispose();
        }
    }
}
