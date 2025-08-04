using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WpfAIFileManager.Services
{
    public static class FileListerService
    {
        public static List<string> ListFiles(string baseDir, string? extension, string? createdAfter, string? createdBefore)
        {
            if (!Directory.Exists(baseDir))
                return new List<string> { "Klasör bulunamadı." };

            var result = new List<string>();

            // Klasörleri listele (sadece genel listelemede göster)
            if (string.IsNullOrWhiteSpace(extension) || extension == "*")
            {
                var directories = Directory.GetDirectories(baseDir, "*", SearchOption.TopDirectoryOnly);
                foreach (var dir in directories)
                {
                    var dirName = Path.GetFileName(dir);
                    result.Add($"[KLASÖR] {dirName}");
                }
            }

            // Dosyaları listele (filtre uygula)
            var files = Directory.EnumerateFiles(baseDir, "*", SearchOption.TopDirectoryOnly);

            // Uzantı filtresi sadece dosyalara uygula
            if (!string.IsNullOrWhiteSpace(extension) && extension != "*")
            {
                files = files.Where(f =>
                    Path.GetExtension(f).Equals($".{extension}", StringComparison.OrdinalIgnoreCase));
            }

            if (DateTime.TryParse(createdAfter, out var after))
            {
                files = files.Where(f => File.GetLastWriteTime(f) >= after);
            }

            if (DateTime.TryParse(createdBefore, out var before))
            {
                files = files.Where(f => File.GetLastWriteTime(f) <= before);
            }

            // Dosyaları ekle
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var fileInfo = new FileInfo(file);
                var size = fileInfo.Length;
                var sizeText = size < 1024 ? $"{size} B" :
                              size < 1024 * 1024 ? $"{size / 1024:F1} KB" :
                              $"{size / (1024 * 1024):F1} MB";

                result.Add($"[DOSYA] {fileName} ({sizeText})");
            }

            return result;
        }

    }
}
