using System;
using System.IO;
using System.IO.Compression;


namespace BeatSaberModManager.Utilities
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

        public static void SafeCreateDirectory(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
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

        public static FileStream? SafeOpenFile(string path, FileMode fileMode, FileAccess fileAccess, FileShare fileShare, FileOptions options)
        {
            try
            {
                return new FileStream(path, fileMode, fileAccess, fileShare, 4096, options);
            }
            catch (ArgumentException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return null;
        }

        public static string? SafeReadAllText(string path)
        {
            try
            {
                return File.ReadAllText(path);
            }
            catch (ArgumentException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return null;
        }
    }
}