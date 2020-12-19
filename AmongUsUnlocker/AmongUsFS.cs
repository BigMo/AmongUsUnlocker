using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AmongUsUnlocker
{
    public class AmongUsFS
    {
        public DirectoryInfo InstallDir { get; private set; }
        public FileInfo GameAssembly => new FileInfo(Path.Combine(InstallDir.FullName, "GameAssembly.dll"));
        public FileInfo GlobalMetaData => new FileInfo(Path.Combine(InstallDir.FullName, "Among Us_Data", "il2cpp_data", "Metadata", "global-metadata.dat"));

        public AmongUsFS(DirectoryInfo directory)
        {
            InstallDir = directory;
            if (!InstallDir.Exists)
                throw new DirectoryNotFoundException($"Among Us install directory \"{directory.FullName}\" not found.");
        }

        public FileInfo CreateBackup()
        {
            var backupFile = new FileInfo(Path.Combine(GameAssembly.Directory.FullName, $"GameAssembly_{DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss")}.bak"));
            GameAssembly.CopyTo(backupFile.FullName, true);
            return backupFile;
        }
    }
}
