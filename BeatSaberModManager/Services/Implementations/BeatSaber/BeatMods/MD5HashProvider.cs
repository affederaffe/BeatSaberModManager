using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

using BeatSaberModManager.Services.Interfaces;
using BeatSaberModManager.Utils;


namespace BeatSaberModManager.Services.Implementations.BeatSaber.BeatMods
{
    public class MD5HashProvider : IHashProvider
    {
        public async Task<string?> CalculateHashForFile(string path)
        {
            await using FileStream? fileStream = IOUtils.TryOpenFile(path, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.Asynchronous);
            return fileStream is null ? null : await CalculateHashForStream(fileStream).ConfigureAwait(false);
        }

        [SuppressMessage("Security", "CA5351:Do Not Use Broken Cryptographic Algorithms")]
        public async Task<string?> CalculateHashForStream(Stream stream)
        {
            using MD5 md5 = MD5.Create();
            byte[] hash = await md5.ComputeHashAsync(stream).ConfigureAwait(false);
            return BitConverter.ToString(hash).Replace("-", null, StringComparison.Ordinal).ToLowerInvariant();
        }
    }
}