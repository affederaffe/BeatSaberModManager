namespace BeatSaberModManager.Services.Implementations.Progress
{
    public readonly struct ProgressInfo
    {
        public StatusType StatusType { get; }
        public string? Text { get; }

        public ProgressInfo(StatusType statusType, string? text)
        {
            StatusType = statusType;
            Text = text;
        }
    }
}