using System.IO;
using System.IO.Compression;

namespace WpfAIFileManager.Services
{
    public class ZipExtractorService
    {
        public static string ExtractAllZips(string baseFolder)
        {
            var zipFiles = Directory.GetFiles(baseFolder, "*.zip", SearchOption.AllDirectories);

            if (zipFiles.Length == 0)
                return "Hiçbir zip dosyası bulunamadı.";

            int count = 0;

            foreach (var zipFile in zipFiles)
            {
                var destinationDir = Path.Combine(Path.GetDirectoryName(zipFile)!, Path.GetFileNameWithoutExtension(zipFile));

                if (!Directory.Exists(destinationDir))
                    Directory.CreateDirectory(destinationDir);

                ZipFile.ExtractToDirectory(zipFile, destinationDir, overwriteFiles: true);
                count++;
            }

            return $"{count} zip dosyası başarıyla ayıklandı.";
        }
    }
}
