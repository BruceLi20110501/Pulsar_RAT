using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Pulsar.Server.Utilities
{
    internal static class PluginPackageImporter
    {
        public static void Import(string packagePath, string pluginsDir)
        {
            if (string.IsNullOrWhiteSpace(packagePath) || !File.Exists(packagePath))
                throw new FileNotFoundException("Package not found", packagePath);
            Directory.CreateDirectory(pluginsDir);

            using (var fs = File.OpenRead(packagePath))
            using (var zip = new ZipArchive(fs, ZipArchiveMode.Read))
            {
                var dllEntries = zip.Entries.Where(e => e.FullName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)).ToList();
                foreach (var entry in dllEntries)
                {
                    var outPath = Path.Combine(pluginsDir, Path.GetFileName(entry.FullName));
                    using (var inStream = entry.Open())
                    using (var outStream = File.Create(outPath))
                    {
                        inStream.CopyTo(outStream);
                    }
                }
            }
        }
    }
}
