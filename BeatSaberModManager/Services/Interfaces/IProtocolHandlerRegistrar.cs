namespace BeatSaberModManager.Services.Interfaces
{
    /// <summary>
    /// Registers and unregister the application to handle protocols.
    /// </summary>
    public interface IProtocolHandlerRegistrar
    {
        /// <summary>
        /// Checks if the application is already registered as a protocol handler for the specified protocol.
        /// </summary>
        /// <param name="protocol">The protocol to check for.</param>
        /// <returns>True if the application is registered as a protocol handler, false otherwise.</returns>
        bool IsProtocolHandlerRegistered(string protocol);

        /// <summary>
        /// Registers the application as a handler for the specified protocol.
        /// </summary>
        /// <param name="protocol">The protocol or scheme.</param>
        void RegisterProtocolHandler(string protocol);

        /// <summary>
        /// Unregisters the application as a handler for the specified protocol.
        /// </summary>
        /// <param name="protocol">The protocol or scheme.</param>
        void UnregisterProtocolHandler(string protocol);
    }
}