using System;


namespace BeatSaberModManager.Models.Interfaces
{
    public interface IObservableVariable<T>
    {
        T? Value { get; set; }
        IObservable<T?> Changed { get; }
    }
}