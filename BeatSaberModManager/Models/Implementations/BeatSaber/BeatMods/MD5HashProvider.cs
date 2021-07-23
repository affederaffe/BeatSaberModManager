using System;
using System.IO;
using System.Security.Cryptography;

using BeatSaberModManager.Models.Interfaces;


namespace BeatSaberModManager.Models.Implementations.BeatSaber.BeatMods
{
    public class MD5HashProvider : IHashProvider
    {
        public string CalculateHashForFile(string path)
        {
            using Stream stream = File.OpenRead(path);
            return CalculateHashForStream(stream);
        }

        public string CalculateHashForStream(Stream stream)
        {
            using MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}