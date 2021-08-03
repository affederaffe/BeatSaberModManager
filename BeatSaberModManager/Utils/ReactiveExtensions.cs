using System;
using System.Runtime.CompilerServices;

using ReactiveUI;


namespace BeatSaberModManager.Utils
{
    public static class ReactiveExtensions
    {
        public static void RaiseAndSetIfChangedConditional<TObj, TRet>(this TObj reactiveObject, ref TRet backingField, TRet newValue, Func<TRet, bool> condition, [CallerMemberName] string? propertyName = null) where TObj : IReactiveObject
        {
            if (!condition(newValue)) return;
            reactiveObject.RaiseAndSetIfChanged(ref backingField, newValue, propertyName);
        }
    }
}