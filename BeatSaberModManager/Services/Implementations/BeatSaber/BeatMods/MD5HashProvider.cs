using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

using BeatSaberModManager.Utils;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.BeatMods
{
    /// <summary>
    /// Provides methods to calculate hashes using MD5.
    /// </summary>
    public static class MD5HashProvider
    {
        /// <summary>
        /// Asynchronously calculates the hash for a file.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <returns>The string representation of the hash, or null when failed to read the file.</returns>
        public static async Task<string?> CalculateHashForFileAsync(string path)
        {
#pragma warning disable CA2007
            await using FileStream? fileStream = IOUtils.TryOpenFile(path, FileMode.Open, FileAccess.Read);
#pragma warning restore CA2007
            return fileStream is null ? null : await CalculateHashForStreamAsync(fileStream).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously calculates the hash for a <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to calculate the hash from.</param>
        /// <returns>The string representation of the hash.</returns>
        [SuppressMessage("Security", "CA5351:Do Not Use Broken Cryptographic Algorithms")]
        public static async Task<string> CalculateHashForStreamAsync(Stream stream)
        {
            using MD5 md5 = MD5.Create();
            byte[] hash = await md5.ComputeHashAsync(stream).ConfigureAwait(false);
            return Convert.ToHexString(hash);
        }
    }
}
