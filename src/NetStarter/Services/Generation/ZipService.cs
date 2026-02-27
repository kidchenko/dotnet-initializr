using System.IO.Compression;

namespace NetStarter.Services.Generation;

public class ZipService
{
    public MemoryStream CreateZip(Dictionary<string, string> files)
    {
        var ms = new MemoryStream();
        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (path, content) in files)
            {
                var entry = zip.CreateEntry(path, CompressionLevel.Optimal);
                using var writer = new StreamWriter(entry.Open());
                writer.Write(content);
            }
        }
        // ZipArchive is disposed here — central directory written to ms
        ms.Position = 0;
        return ms;
    }
}
