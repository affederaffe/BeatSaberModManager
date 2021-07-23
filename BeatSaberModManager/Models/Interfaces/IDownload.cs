namespace BeatSaberModManager.Models.Interfaces
{
    public interface IDownload
    {
        string? Type { get; set; }
        string? Url { get; set; }
        IHash[]? Hashes { get; set; }
    }
}