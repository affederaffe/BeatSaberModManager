namespace BeatSaberModManager.Models.Interfaces
{
    public interface ISettings<out T>
    {
        T Value { get; }
    }
}