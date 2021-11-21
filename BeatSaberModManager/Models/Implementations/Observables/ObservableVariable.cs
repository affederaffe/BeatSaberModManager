using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text.Json.Serialization;


namespace BeatSaberModManager.Models.Implementations.Observables
{
    public class ObservableVariable<T>
    {
        private readonly BehaviorSubject<T?> _subject = new(default);

        private T? _value;
        public T? Value
        {
            get => _value;
            set => SetAndRaiseIfChanged(value);
        }

        [JsonIgnore]
        public IObservable<T?> Changed => _subject;

        private void SetAndRaiseIfChanged(T? value)
        {
            if (EqualityComparer<T>.Default.Equals(_value, value)) return;
            _value = value;
            _subject.OnNext(value);
        }
    }
}