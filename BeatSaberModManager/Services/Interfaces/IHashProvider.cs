using System.IO;
using System.Threading.Tasks;


namespace BeatSaberModManager.Services.Interfaces
{
    /// <summary>
    /// Provides methods to calculate hashes.
    /// </summary>
    public interface IHashProvider
    {
        /// <summary>
        /// Asynchronously calculates the hash for a file.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <returns>The string representation of the hash, or null when failed to read the file.</returns>
        Task<string?> CalculateHashForFile(string path);

        /// <summary>
        /// Asynchronously calculates the hash for a <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to calculate the hash from.</param>
        /// <returns>The string representation of the hash.</returns>
        Task<string> CalculateHashForStream(Stream stream);
    }
}