using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;


namespace BeatSaberModManager.Utils
{
    /// <summary>
    /// Utilities for various IO-related operations.
    /// </summary>
    public static class IOUtils
    {
        /// <summary>
        /// Attempts to delete a file.
        /// </summary>
        /// <param name="path">The file to delete.</param>
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

        /// <summary>
        /// Attempts to move a file.
        /// </summary>
        /// <param name="path">The file to move.</param>
        /// <param name="dest">The new path for the file.</param>
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

        /// <summary>
        /// Attempts to create all directories and subdirectories in the specified path unless they already exist.
        /// </summary>
        /// <param name="path">The directory to create.</param>
        /// <returns>true if the operation succeeds, false otherwise.</returns>
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

        /// <summary>
        /// Attempts to delete the specified directory and, if indicated, any subdirectories and files in the directory.
        /// </summary>
        /// <param name="path">The name of the directory to remove.</param>
        /// <param name="recursive">true to remove directories, subdirectories, and files in path, false otherwise.</param>
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

        /// <summary>
        /// Attempts to extract all of the files in the archive to a directory on the file system.
        /// </summary>
        /// <param name="archive">The archive to extract.</param>
        /// <param name="path">The path to the destination directory on the file system.</param>
        /// <param name="overrideFiles">true to overwrite existing files, false otherwise.</param>
        /// <returns>true if the operation succeeds, false otherwise.</returns>
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

        /// <summary>
        /// Attempts to open a <see cref="FileStream"/> on the specified path, having the specified mode with read, write, or read/write access and the specified sharing option.
        /// </summary>
        /// <param name="path">The file to open.</param>
        /// <param name="options">An object that describes optional FileStream parameters to use.</param>
        /// <returns>The <see cref="FileStream"/> if the operation succeeds, null otherwise.</returns>
        public static FileStream? TryOpenFile(string path, FileStreamOptions options)
        {
            try
            {
                return File.Open(path, options);
            }
            catch (ArgumentException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return null;
        }

        /// <summary>
        /// Attempts to open a text file, read all the text in the file, and then close the file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        /// <param name="text">The file's content if the operation succeeds, null otherwise.</param>
        /// <returns>true if the operation succeeds, false otherwise.</returns>
        public static bool TryReadAllText(string path, [MaybeNullWhen(false)] out string text)
        {
            try
            {
                text = File.ReadAllText(path);
                return true;
            }
            catch (ArgumentException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            text = null;
            return false;
        }
    }
}