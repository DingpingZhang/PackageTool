﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PackageTool
{
    internal static class Extensions
    {
        private const string ConfigurationPlaceholder = "{Configuration}";
#if RELEASE
        private const string ConfigurationName = "release";
#elif DEBUG
        private const string ConfigurationName = "debug";
#endif
        private static readonly Regex VersionPlaceholderRegex = new Regex(@"\{Version:(.+?)\}", RegexOptions.Compiled);


        public static string ReplaceConfigurationPlaceholder(this string path)
        {
            return path.Replace(ConfigurationPlaceholder, ConfigurationName);
        }

        public static string ReplaceVersionPlaceholder(this string path, string sourcePath = null)
        {
            var match = VersionPlaceholderRegex.Match(path);

            return match.Success
                ? path.Replace(match.Value, AssemblyName
                    .GetAssemblyName(sourcePath == null
                        ? match.Groups[1].Value
                        : Path.Combine(sourcePath, match.Groups[1].Value))
                    .Version
                    .ToString(3))
                : path;
        }

        public static string GetFullPath(this string relativePath, string baseDirectory = null)
        {
            baseDirectory = baseDirectory ?? Program.BaseDirectory;
            return Path.GetFullPath(Path.Combine(baseDirectory, relativePath));
        }

        public static IEnumerable<FileElement> Flatten(this FolderElement @this)
        {
            yield return @this;

            foreach (var file in @this.Files)
            {
                yield return file;
            }

            foreach (var folder in @this.Folders)
            {
                foreach (var element in folder.Flatten())
                {
                    yield return element;
                }
            }
        }

        public static void ForEach<T>(this IEnumerable<T> @this, Action<T> callback)
        {
            foreach (var item in @this)
            {
                callback?.Invoke(item);
            }
        }

        // ReSharper disable AssignNullToNotNullAttribute
        public static void CopyTo(this string source, string target)
        {
            if (File.Exists(source))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(target));

                File.Copy(source, target, true);
            }
            else if (Directory.Exists(source))
            {
                Directory.CreateDirectory(target);

                Directory.GetFileSystemEntries(source)
                    .ForEach(item => item.CopyTo(Path.Combine(target, Path.GetFileName(item))));
            }
            else
            {
                Print.Error($"Missing {source}!");
            }
        }
        // ReSharper restore AssignNullToNotNullAttribute
    }
}
