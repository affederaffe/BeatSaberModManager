using System;
using System.Collections.Generic;
using System.Reactive.Subjects;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.Observables
{
    public class ObservableVariable<T> : IObservableVariable<T>
    {
        private readonly Subject<T?> _subject = new();

        private T? _value;
        public T? Value
        {
            get => _value;
            set => SetAndRaiseIfChanged(value);
        }

        private void SetAndRaiseIfChanged(T? value)
        {
            if (EqualityComparer<T>.Default.Equals(_value, value)) return;
            _value = value;
            _subject.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            observer.OnNext(_value!);
            return _subject.Subscribe(observer!);
        }
    }
}