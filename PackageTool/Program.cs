using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace PackageTool
{
    public class Program
    {
        public static string BaseDirectory;

        public static void Main(string[] args)
        {
            var filePaths = SelectPackageDefineFiles(args);
            if (!filePaths.Any())
            {
                filePaths = SelectPackageDefineFiles(Directory.GetFiles(Environment.CurrentDirectory));
            }

            if (filePaths.Any())
            {
                filePaths.ForEach(path => TryPack(path));
            }
            else
            {
                Print.Warning("Please provide one or more \"*.package.xml\" file paths.");
            }

            Print.EndLine();
            Console.ReadKey();

            string[] SelectPackageDefineFiles(IEnumerable<string> files)
            {
                return files.Where(path => path.EndsWith(".package.xml")).ToArray();
            }
        }

        private static void TryPack(string path, bool openFolder = true)
        {
            var name = Path.GetFileName(path);
            try
            {
                Print.Divider();
                Print.Info($">>> Start executing {name} file...");

                BaseDirectory = Path.GetDirectoryName(path);
                var element = FolderElement.Create(File.ReadAllText(path));
                Pack(element, openFolder);

                Print.Info($"<<< Completed the {name} file. ");
            }
            catch (FileNotFoundException e)
            {
                Print.Error($"<In {name}> Missing {e.FileName}!");
            }
            catch (Exception e)
            {
                Print.Error($"<In {name}> {e.Message}");
            }
        }

        private static void Pack(FolderElement info, bool openFolder = true)
        {
            string zipTargetPath;
            using (var tempFolder = new TempDirectory(info.Target))
            {
                Print.Info($"Start Copying: {info.Source} --> {info.Target}");
                info.Flatten()
                    .Where(item => !(item is FolderElement folder) ||
                                   !folder.Files.Any() && !folder.Folders.Any())
                    .ForEach(item => item.Source.CopyTo(item.Target));
                Print.Info("Completed Copying. ");

                zipTargetPath = $"{tempFolder.Path}.zip";
                Print.Info($"Start Package: {zipTargetPath}...");
                if (File.Exists(zipTargetPath)) File.Delete(zipTargetPath);
                ZipFile.CreateFromDirectory(
                    tempFolder.Path,
                    zipTargetPath,
                    CompressionLevel.Optimal,
                    true);
            }

            if (openFolder)
            {
                Print.Info($"Opening the file: {zipTargetPath}...");
                Process.Start("explorer.exe", $"/select, {zipTargetPath}");
            }
        }
    }
}
