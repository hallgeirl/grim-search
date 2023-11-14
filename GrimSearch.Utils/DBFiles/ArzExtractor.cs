/*
    ATTRIBUTION
    The work in this file is loosely translated from the work of atom0s [atom0s@live.com]. 
    Original source code: https://github.com/atom0s/grimarc and https://github.com/atom0s/grimarz/

*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using GrimSearch.Utils.CharacterFiles;
using K4os.Compression.LZ4;

namespace GrimSearch.Utils.DBFiles
{
    public static unsafe class ArzExtractor
    {

        private static string GetNullTerminatedString(byte[] buffer, int index)
        {
            string result = "";
            for (int i = index; i < buffer.Length; i++)
            {
                if (buffer[i] == 0)
                {
                    result = Encoding.ASCII.GetString(buffer, index, i - index);
                    break;
                }
            }
            return result;
        }
        public static void ExtractArc(string arcPath, string targetPath)
        {
            // load archive into memory
            var buffer = File.ReadAllBytes(arcPath);
            using var stream = new MemoryStream(buffer);
            var reader = new BinaryReader(stream);

            // Read header
            var magic = reader.ReadUInt32();
            var version = reader.ReadUInt32();
            var numberOfFileEntries = reader.ReadUInt32();
            var numberOfDataRecords = reader.ReadUInt32();
            var recordTableSize = reader.ReadUInt32();
            var stringTableSize = reader.ReadUInt32();
            var recordTableOffset = reader.ReadUInt32();

            if (magic != 0x435241 || version != 3)
            {
                throw new Exception($"File is not in the correct format: Header mismatch -- expected magic number: 4411969, got: {magic}, and expected version: 3 but got: {version}");
            }

            for (var i = 0; i < numberOfFileEntries; i++)
            {
                var tocEntryOffset = recordTableOffset + recordTableSize + stringTableSize + i * 44;
                stream.Seek(tocEntryOffset, SeekOrigin.Begin);

                // Read TOC entry
                var entryType = reader.ReadUInt32();
                var fileOffset = reader.ReadUInt32();
                var compressedSize = reader.ReadUInt32();
                var decompressedSize = reader.ReadUInt32();
                var decompressedHash = reader.ReadUInt32();
                var fileTime = reader.ReadInt64();
                var fileParts = reader.ReadUInt32();
                var firstPartIndex = reader.ReadUInt32();
                var stringEntryLength = reader.ReadUInt32();
                var stringEntryOffset = reader.ReadUInt32();

                // Get the output filename and create the output directory
                var outputFilenameOffset = recordTableOffset + recordTableSize + stringEntryOffset;
                string outputFilename = GetNullTerminatedString(buffer, (int)outputFilenameOffset);
                var fullOutputPath = Path.Combine(targetPath, outputFilename);

                Directory.CreateDirectory(Path.GetDirectoryName(fullOutputPath));

                // If entry is not compressed, just copy it directly to the output file.
                if (entryType == 1 && compressedSize == decompressedSize)
                {
                    stream.Seek(fileOffset, SeekOrigin.Begin);
                    File.WriteAllBytes(fullOutputPath, reader.ReadBytes((int)decompressedSize));
                }
                else
                {
                    // If it is compressed, we have to iterate over each file part, and copy or decompress each of them.
                    using var outputStream = File.OpenWrite(fullOutputPath);
                    var writer = new BinaryWriter(outputStream);
                    for (int j = 0; j < fileParts; j++)
                    {
                        stream.Seek(recordTableOffset + (firstPartIndex + j) * sizeof(uint) * 3, SeekOrigin.Begin);
                        var partOffset = reader.ReadUInt32();
                        var compressedPartSize = reader.ReadUInt32();
                        var decompressedPartSize = reader.ReadUInt32();

                        if (compressedPartSize == decompressedPartSize)
                        {
                            writer.Write(buffer, (int)partOffset, (int)decompressedSize);
                        }
                        else
                        {
                            // Decompress with LZ4
                            var target = new byte[decompressedPartSize];
                            var decoded = LZ4Codec.Decode(buffer, (int)partOffset, (int)compressedPartSize, target, 0, target.Length);
                            writer.Write(target, 0, (int)decompressedPartSize);
                        }
                    }
                }
            }
        }

        public static string ExtractArz(string arzPath, string grimDawnDirectory, string targetPath)
        {
            /*
            struct ARZ_V3_HEADER
            {
                unsigned short  Unknown;
                unsigned short  Version;
                unsigned int    RecordTableStart;
                unsigned int    RecordTableSize;
                unsigned int    RecordTableEntries;
                unsigned int    StringTableStart;
                unsigned int    StringTableSize;
            };
            */
            return null;
        }

        public static string Extract(string arzPath, string grimDawnDirectory, string targetPath)
        {
            var archiveTool = Path.Combine(grimDawnDirectory, "ArchiveTool.exe");
            if (!File.Exists(archiveTool))
            {
                LogHelper.GetLog().Error("ArchiveTool.exe not found in directory: " + grimDawnDirectory + ". Check that you have configured the correct Grim Dawn directory.");
                throw new InvalidOperationException("ArchiveTool.exe not found in directory: " + grimDawnDirectory + ". Check that you have configured the correct Grim Dawn directory.");
            }
            LogHelper.GetLog().Debug("Found ArchiveTool.exe in " + grimDawnDirectory);

            if (Directory.Exists(targetPath))
                Directory.Delete(targetPath, true);

            Directory.CreateDirectory(targetPath);
            LogHelper.GetLog().Debug("Created temp dir at: " + targetPath);

            var extractCommand = "-database";

            if (Path.GetExtension(arzPath).ToLower() == ".arc")
                extractCommand = "-extract";

            var arguments = "\"" + arzPath + "\" " + extractCommand + " \"" + targetPath + "\"";

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

            LogHelper.GetLog().Debug(arzPath + " was successfully extracted to " + targetPath);

            return targetPath;
        }
    }
}
