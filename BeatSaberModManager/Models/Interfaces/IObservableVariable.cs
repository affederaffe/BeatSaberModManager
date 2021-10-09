using System;


namespace BeatSaberModManager.Models.Interfaces
{
    public interface IObservableVariable<T> : IObservable<T>
    {
        T? Value { get; set; }
    }
}