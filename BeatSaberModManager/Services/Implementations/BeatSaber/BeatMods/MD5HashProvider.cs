using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.BeatMods
{
    /// <inheritdoc />
    public class MD5HashProvider : IHashProvider
    {
        /// <inheritdoc />
        public async Task<string?> CalculateHashForFileAsync(string path)
        {
#pragma warning disable CA2007
            await using FileStream? fileStream = IOUtils.TryOpenFile(path, FileMode.Open, FileAccess.Read);
#pragma warning restore CA2007
            return fileStream is null ? null : await CalculateHashForStreamAsync(fileStream).ConfigureAwait(false);
        }

        /// <inheritdoc />
        [SuppressMessage("Security", "CA5351:Do Not Use Broken Cryptographic Algorithms")]
        public async Task<string> CalculateHashForStreamAsync(Stream stream)
        {
            using MD5 md5 = MD5.Create();
            byte[] hash = await md5.ComputeHashAsync(stream).ConfigureAwait(false);
            return Convert.ToHexString(hash);
        }
    }
}
