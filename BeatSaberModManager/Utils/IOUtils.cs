using System;
using System.IO;
using System.IO.Compression;


namespace BeatSaberModManager.Utils
{
    public static class IOUtils
    {
        public static void SafeDeleteFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (ArgumentException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            
        }

        public static void SafeDeleteDirectory(string path, bool recursive)
        {
            try
            {
                Directory.Delete(path, recursive);
            }
            catch (ArgumentException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        public static void SafeExtractArchive(ZipArchive archive, string path, bool overrideFiles)
        {
            try
            {
                archive.ExtractToDirectory(path, overrideFiles);
            }
            catch (ArgumentException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}