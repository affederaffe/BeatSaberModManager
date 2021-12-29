namespace BeatSaberModManager.Services.Implementations.Progress
{
    public struct ProgressInfo
    {
        public StatusType StatusType;
        public string? Text;

        public ProgressInfo(StatusType statusType, string? text)
        {
            StatusType = statusType;
            Text = text;
        }
    }
}