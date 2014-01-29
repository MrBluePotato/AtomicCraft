﻿using System.IO;
using System.IO.Compression;

namespace fCraft.UpdateBuilder
{
    internal static class Program
    {
        private const string BinariesFileName = "../../UpdateInstaller/Resources/Payload.zip";

        private static readonly string[] FileList =
        {
            "Server (Graphical).exe",
            "Server (Command Line).exe",
            "fCraft.dll",
            "fCraftGUI.dll",
            "Configuration.exe",
            "UpdateBuilder.exe",
            "UpdateInstaller.exe",
            "Changelog.txt",
            "Readme.txt"
        };


        private static void Main()
        {
            FileInfo binaries = new FileInfo(BinariesFileName);
            if (binaries.Exists)
            {
                binaries.Delete();
            }

            using (ZipStorer zs = ZipStorer.Create(binaries.FullName, ""))
            {
                foreach (string file in FileList)
                {
                    FileInfo fi = new FileInfo(file);
                    if (!fi.Exists)
                    {
                        return; // abort if any of the files do not exist
                    }
                    zs.AddFile(ZipStorer.Compression.Deflate, fi.FullName, fi.Name, "");
                }
            }
        }
    }
}