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
            // Load archive into memory.
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
                throw new Exception($"Arc file {arcPath} is not in the correct format: Header mismatch -- expected magic number: 4411969, got: {magic}, and expected version: 3 but got: {version}");
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
                    using var outputStream = File.Open(fullOutputPath, FileMode.Create);
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

        public static void ExtractArz(string arzPath, string grimDawnDirectory, string targetPath)
        {
            // Load archive into memory.
            var buffer = File.ReadAllBytes(arzPath);
            using var stream = new MemoryStream(buffer);
            var reader = new BinaryReader(stream);

            var unknown = reader.ReadUInt16();
            var version = reader.ReadUInt16();
            var recordTableStart = reader.ReadUInt32();
            var recordTableSize = reader.ReadUInt32();
            var recordTableEntries = reader.ReadUInt32();
            var stringTableStart = reader.ReadUInt32();
            var stringTableSize = reader.ReadUInt32();

            if (unknown != 2 || version != 3)
            {
                throw new Exception($"Arz file {arzPath} is not in the correct format: Header mismatch -- expected magic number: 2, got: {unknown}, and expected version: 3 but got: {version}");
            }
            var stringTable = ReadStringTable(buffer, stringTableStart, stringTableSize);

            //var recordStart = recordTableStart;
            stream.Seek(recordTableStart, SeekOrigin.Begin);
            for (int i = 0; i < recordTableEntries; i++)
            {

                var recordFileIndex = reader.ReadInt32();
                var recordFile = stringTable[recordFileIndex];
                var fullOutputPath = Path.Combine(targetPath, recordFile);
                var recordTypeLength = reader.ReadInt32();
                var recordTypeBytes = reader.ReadBytes(recordTypeLength);
                var recordType = Encoding.ASCII.GetString(recordTypeBytes);
                var recordDataOffset = reader.ReadInt32();
                var recordDataSizeCompressed = reader.ReadInt32();
                var recordDataSizeDecompressed = reader.ReadInt32();
                reader.ReadInt32(); reader.ReadInt32(); //Skip ahead 8 bytes

                // Decompress with LZ4
                var decompressed = new byte[recordDataSizeDecompressed];
                var decoded = LZ4Codec.Decode(buffer, recordDataOffset + 24, recordDataSizeCompressed, decompressed, 0, decompressed.Length);

                if ((decoded < 0) || (recordDataSizeDecompressed % 4) != 0)
                {
                    throw new Exception("Failed to decompress entry " + recordFile);
                }

                Directory.CreateDirectory(Path.GetDirectoryName(fullOutputPath));
                using var outputWriter = new StreamWriter(File.Open(fullOutputPath, FileMode.Create));
                using var blockStream = new MemoryStream(decompressed);
                using var blockReader = new BinaryReader(blockStream);
                int currentPosition = 0;
                while (currentPosition < recordDataSizeDecompressed / 4)
                {
                    var dataType = blockReader.ReadInt16();
                    var dataCount = blockReader.ReadInt16();
                    var dataStringIndex = blockReader.ReadInt32();

                    outputWriter.Write(stringTable[dataStringIndex] + ",");
                    for (int j = 0; j < dataCount; j++)
                    {
                        switch (dataType)
                        {
                            case 1:
                                var floatValue = blockReader.ReadSingle();
                                outputWriter.Write(floatValue.ToString() + ",");
                                break;
                            case 2:
                                var stringIndex = blockReader.ReadInt32();
                                outputWriter.Write(stringTable[stringIndex] + ",");
                                break;
                            case 0:
                            case 3:
                            default:
                                var intValue = blockReader.ReadInt32();
                                outputWriter.Write(intValue);
                                break;

                        }
                    }
                    outputWriter.WriteLine();
                    currentPosition += 2 + dataCount;

                }
                //File.WriteAllBytes(fullOutputPath, target);
            }

            /*

                        // Set the floating point percision..
                        ofs << std::fixed << std::setprecision(6);

                        
                            ofs << g_StringTable[dataString]->GetString() << ",";

                            for (auto y = 0; y < dataCount; y++)
                            {
                                switch (dataType)
                                {
                                case 0:
                                case 3:
                                default:
                                    ofs << *(unsigned int*)((data_ptr + 8) + (y * 4)) << ",";
                                    break;
                                case 1:
                                    ofs << *(float*)((data_ptr + 8) + (y * 4)) << ",";
                                    break;
                                case 2:
                                    ofs << g_StringTable[*(unsigned int*)((data_ptr + 8) + (y * 4))]->GetString() << ",";
                                    break;
                                }
                            }

                            ofs << std::endl;

                            // Adjust the positions..
                            data_ptr += 8 + (dataCount * 4);
                            current += (2 + dataCount);
                        }
                    }

                    return true;
                }();

                // Cleanup the file buffer..
                printf_s("Finished processing file, status: %s\n", dump_file == true ? "success!" : "failed!");
                delete[] buffer;

                // Cleanup global string table..
                std::for_each(g_StringTable.begin(), g_StringTable.end(), [&](ARZString* str) { delete str; });
                g_StringTable.clear();

                return 0;



            */
        }

        static IList<string> ReadStringTable(byte[] buffer, uint offset, uint size)
        {
            using var stream = new MemoryStream(buffer);
            using BinaryReader reader = new BinaryReader(stream);

            List<string> stringTable = new List<string>();

            uint end = offset + size;
            stream.Seek(offset, SeekOrigin.Begin);

            while (stream.Position < end)
            {
                var count = reader.ReadUInt32();

                for (var i = 0; i < count; i++)
                {
                    var length = reader.ReadInt32();
                    var stringBytes = reader.ReadBytes(length);
                    stringTable.Add(Encoding.ASCII.GetString(stringBytes));
                }
            }
            return stringTable;
        }
        /*
        bool ReadStringTable(unsigned char* buffer, int offset, int size)
        {
            if (buffer == nullptr || size == 0)
                return false;

            auto ptr = &buffer[offset];
            auto end = ptr + size;

            while (ptr < end)
            {
                auto count = *(DWORD*)ptr;
                ptr += 4;

                for (auto x = 0; x < count; x++)
                {
                    auto length = *(DWORD*)ptr;
                    ptr += 4;

                    auto str = new ARZString(ptr, length);
                    g_StringTable.push_back(str);

                    ptr += length;
                }
            }

            return true;
        }
        */
        private static string Extract(string arzPath, string grimDawnDirectory, string targetPath)
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
