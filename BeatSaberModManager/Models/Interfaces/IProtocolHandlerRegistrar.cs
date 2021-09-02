namespace BeatSaberModManager.Models.Interfaces
{
    public interface IProtocolHandlerRegistrar
    {
        bool IsProtocolHandlerRegistered(string protocol);
        void RegisterProtocolHandler(string protocol);
        void UnregisterProtocolHandler(string protocol);
    }
}