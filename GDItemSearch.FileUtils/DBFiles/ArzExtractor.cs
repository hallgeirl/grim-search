using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace GDItemSearch.FileUtils.DBFiles
{
    public static class ArzExtractor
    {
        public static string Extract(string arzPath, string grimDawnDirectory)
        {
            var archiveTool = Path.Combine(grimDawnDirectory, "ArchiveTool.exe");
            if (!File.Exists(archiveTool))
            {
                throw new InvalidOperationException("ArchiveTool.exe not found in directory: " + grimDawnDirectory + ". Check that you have configured the correct Grim Dawn directory.");
            }

            var tempDir = Path.Combine(Path.GetTempPath(), "GDArchiveTempPath");

            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);

            Directory.CreateDirectory(tempDir);

            var extractCommand = "-database";

            if (Path.GetExtension(arzPath).ToLower() == ".arc")
                extractCommand = "-extract";

            var arguments = "\"" + arzPath + "\" " + extractCommand + " " + tempDir;
            
            var process = Process.Start(archiveTool, arguments);
            
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new Exception("ArchiveTool.exe exited with exit code " + process.ExitCode);

            return tempDir;
        }
    }
}
