using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;


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
        public static bool TryMoveFile(string path, string dest)
        {
            try
            {
                File.Move(path, dest);
                return true;
            }
            catch (ArgumentException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return false;
        }

        /// <summary>
        /// Attempts to create all directories and subdirectories in the specified path unless they already exist.
        /// </summary>
        /// <param name="path">The directory to create.</param>
        /// <returns>True if the operation succeeds, false otherwise.</returns>
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
        /// <param name="recursive">True to remove directories, subdirectories, and files in path, false otherwise.</param>
        public static bool TryDeleteDirectory(string path, bool recursive)
        {
            try
            {
                Directory.Delete(path, recursive);
                return true;
            }
            catch (ArgumentException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return false;
        }

        /// <summary>
        /// Attempts to extract all of the files in the archive to a directory on the file system.
        /// </summary>
        /// <param name="archive">The archive to extract.</param>
        /// <param name="path">The path to the destination directory on the file system.</param>
        /// <param name="overrideFiles">True to overwrite existing files, false otherwise.</param>
        /// <returns>True if the operation succeeds, false otherwise.</returns>
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
        /// Attempts to open a <see cref="FileStream"/> o the specified path, having the specified mode with read, write, or read/write access and the specified sharing option.
        /// </summary>
        /// <param name="path">The file to open.</param>
        /// <param name="mode">A <see cref="FileMode"/>value that specifies whether a file is created if one does not exist, and determines whether the contents of existing files are retained or overwritten.</param>
        /// <param name="access">A <see cref="FileAccess"/> value that specifies the operations that can be performed on the file.</param>
        /// <returns>The <see cref="FileStream"/> if the operation succeeds, null otherwise.</returns>
        public static FileStream? TryOpenFile(string path, FileMode mode, FileAccess access)
        {
            try
            {
                return File.Open(path, mode, access);
            }
            catch (ArgumentException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return null;
        }

        /// <summary>
        /// Attempts to read the lines of a file.
        /// </summary>
        /// <param name="path">The file to read.</param>
        /// <returns>All the lines of the file if the operation succeeds, null otherwise.</returns>
        public static async Task<string[]?> TryReadAllLinesAsync(string path)
        {
            try
            {
                return await File.ReadAllLinesAsync(path).ConfigureAwait(false);
            }
            catch (ArgumentException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            return null;
        }
    }
}
