﻿using System;
using System.IO;

namespace PackageTool
{
    public sealed class TempDirectory : IDisposable
    {
        public string Path { get; }

        public TempDirectory(string tempDirectoryPath)
        {
            Path = tempDirectoryPath;

            // Clean up the path, in order to make sure the folder is empty.
            DeleteFileSystemEntry(Path);

            Directory.CreateDirectory(Path);
        }

        public bool MoveTo(string targetPath, string subDirectoryName = null)
        {
            try
            {
                var directory = System.IO.Path.Combine(Path, subDirectoryName ?? string.Empty);
                if (!Directory.Exists(directory))
                    throw new DirectoryNotFoundException();

                Directory.Move(directory, targetPath);

                return true;
            }
            catch (Exception e)
            {
                Print.Error(e);

                return false;
            }
        }

        public static void DeleteFileSystemEntry(string path)
        {
            if (Directory.Exists(path)) Directory.Delete(path, true);
            if (File.Exists(path)) File.Delete(path);
        }

        public void Dispose()
        {
            DeleteFileSystemEntry(Path);
        }
    }
}
