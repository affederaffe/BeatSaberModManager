using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;


namespace BeatSaberModManager.Utils
{
    public static class IOUtils
    {
        public static void TryDeleteFile(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch (ArgumentException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        public static void TryMoveFile(string path, string dest)
        {
            try
            {
                File.Move(path, dest);
            }
            catch (ArgumentException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        public static bool TryCreateDirectory(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
                return true;
            }
            catch (ArgumentException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return false;
        }

        public static void TryDeleteDirectory(string path, bool recursive)
        {
            try
            {
                Directory.Delete(path, recursive);
            }
            catch (ArgumentException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        public static bool TryExtractArchive(ZipArchive archive, string path, bool overrideFiles)
        {
            try
            {
                archive.ExtractToDirectory(path, overrideFiles);
                return true;
            }
            catch (ArgumentException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return false;
        }

        public static FileStream? TryOpenFile(string path, FileMode fileMode, FileAccess fileAccess, FileShare fileShare, FileOptions options)
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

        public static bool TryReadAllText(string path, [MaybeNullWhen(false)] out string text)
        {
            text = null;
            try
            {
                text = File.ReadAllText(path);
                return true;
            }
            catch (ArgumentException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return false;
        }
    }
}