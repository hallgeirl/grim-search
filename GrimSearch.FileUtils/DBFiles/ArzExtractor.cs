using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace GrimSearch.Utils.DBFiles
{
    public static class ArzExtractor
    {
        public static string Extract(string arzPath, string grimDawnDirectory)
        {
            var archiveTool = Path.Combine(grimDawnDirectory, "ArchiveTool.exe");
            if (!File.Exists(archiveTool))
            {
                LogHelper.GetLog().Error("ArchiveTool.exe not found in directory: " + grimDawnDirectory + ". Check that you have configured the correct Grim Dawn directory.");
                throw new InvalidOperationException("ArchiveTool.exe not found in directory: " + grimDawnDirectory + ". Check that you have configured the correct Grim Dawn directory.");
            }
            LogHelper.GetLog().Debug("Found ArchiveTool.exe in " + grimDawnDirectory);

            var tempDir = Path.Combine(Path.GetTempPath(), "GDArchiveTempPath");

            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);

            Directory.CreateDirectory(tempDir);
            LogHelper.GetLog().Debug("Created temp dir at: " + tempDir);

            var extractCommand = "-database";

            if (Path.GetExtension(arzPath).ToLower() == ".arc")
                extractCommand = "-extract";

            var arguments = "\"" + arzPath + "\" " + extractCommand + " " + tempDir;

            LogHelper.GetLog().Debug("Executing: " + archiveTool + " " + arguments);
            var process = new Process();
            process.StartInfo = new ProcessStartInfo()
            {
                Arguments = arguments,
                FileName = archiveTool
            };

            process.Start();

            process.WaitForExit();
            LogHelper.GetLog().Debug("Execution finished.");

            if (process.ExitCode != 0)
                throw new Exception("ArchiveTool.exe exited with exit code " + process.ExitCode);

            LogHelper.GetLog().Debug(arzPath + " was successfully extracted to " + tempDir);

            return tempDir;
        }
    }
}
