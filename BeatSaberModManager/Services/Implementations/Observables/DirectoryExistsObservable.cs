using System;
using System.IO;
using System.Reactive.Subjects;


namespace BeatSaberModManager.Services.Implementations.Observables
{
    /// <summary>
    /// An <see cref="IObservable{T}"/> that signals when the directory at the specified <see cref="Path"/> becomes valid or invalid.
    /// </summary>
    public sealed class DirectoryExistsObservable : IObservable<bool>, IDisposable
    {
        private readonly FileSystemWatcher _fileSystemWatcher = new();
        private readonly BehaviorSubject<bool> _subject = new(false);

        private string? _path;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryExistsObservable"/> class.
        /// </summary>
        public DirectoryExistsObservable()
        {
            _fileSystemWatcher.Created += OnCreated;
            _fileSystemWatcher.Renamed += OnRenamed;
            _fileSystemWatcher.Deleted += OnDeleted;
            _fileSystemWatcher.NotifyFilter = NotifyFilters.DirectoryName;
        }

        /// <inheritdoc cref="FileSystemWatcher.Path" />
        public string? Path
        {
            get => _path;
            set
            {
                if (value is null || !Directory.Exists(value))
                {
                    _path = null;
                    _fileSystemWatcher.EnableRaisingEvents = false;
                    _subject.OnNext(false);
                }
                else
                {
                    DirectoryInfo directoryInfo = new(value);
                    _path = System.IO.Path.TrimEndingDirectorySeparator(directoryInfo.FullName);
                    _fileSystemWatcher.Path = directoryInfo.Parent?.FullName ?? throw new InvalidOperationException("Cannot watch root directory.");
                    _fileSystemWatcher.EnableRaisingEvents = true;
                    _subject.OnNext(true);
                }
            }
        }

        /// <inheritdoc />
        public IDisposable Subscribe(IObserver<bool> observer) => _subject.Subscribe(observer);

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath == _path)
                _subject.OnNext(true);
        }

        private void OnRenamed(object sender, RenamedEventArgs e) => _subject.OnNext(e.FullPath == _path);

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath == _path)
                _subject.OnNext(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _fileSystemWatcher.Created -= OnCreated;
            _fileSystemWatcher.Deleted -= OnDeleted;
            _fileSystemWatcher.Dispose();
        }
    }
}
