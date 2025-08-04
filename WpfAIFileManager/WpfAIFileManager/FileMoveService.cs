using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WpfAIFileManager
{
    public static class FileMoveService
    {
        public static string MoveFiles(string baseDir, string? extension, string? createdAfter, string? createdBefore, string? destinationFolder, List<string>? source = null)
        {
            if (string.IsNullOrWhiteSpace(destinationFolder))
                return "Hedef klasör belirtilmedi.";

            if (destinationFolder == "*" || destinationFolder == ".*" || destinationFolder.Contains("*"))
                return "Geçersiz hedef klasör adı.";

            if (!Directory.Exists(baseDir))
                return "BASE klasörü bulunamadı.";

            string fullDestination = Path.Combine(baseDir, destinationFolder);

            if (!Directory.Exists(fullDestination))
                Directory.CreateDirectory(fullDestination);

            var matched = new List<FileInfo>();
            List<string> extensions = new();

            if (!string.IsNullOrWhiteSpace(extension) && extension != "*")
            {
                extensions = extension.Split(',', '.', '|', ';')
                                      .Select(e => e.Trim().ToLower())
                                      .Where(e => !string.IsNullOrWhiteSpace(e))
                                      .ToList();
            }

            if (source != null && source.Any())
            {
                foreach (var folderName in source)
                {
                    string folderPath = Path.Combine(baseDir, folderName);

                    if (Directory.Exists(folderPath))
                    {
                        var files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories)
                                             .Select(f => new FileInfo(f));

                        if (extensions.Any())
                            files = files.Where(f => extensions.Contains(f.Extension.TrimStart('.').ToLower()));

                        if (DateTime.TryParse(createdAfter, out var after))
                            files = files.Where(f => f.CreationTime >= after);

                        if (DateTime.TryParse(createdBefore, out var before))
                            files = files.Where(f => f.CreationTime <= before);

                        matched.AddRange(files);
                    }
                }
            }
            else
            {
                var files = Directory.GetFiles(baseDir, "*", SearchOption.AllDirectories)
                                     .Select(f => new FileInfo(f));

                if (extensions.Any())
                    files = files.Where(f => extensions.Contains(f.Extension.TrimStart('.').ToLower()));

                if (DateTime.TryParse(createdAfter, out var after))
                    files = files.Where(f => f.CreationTime >= after);

                if (DateTime.TryParse(createdBefore, out var before))
                    files = files.Where(f => f.CreationTime <= before);

                matched = files.ToList();
            }

            var errors = new List<string>();

            foreach (var file in matched)
            {
                string destPath = Path.Combine(fullDestination, file.Name);

                try
                {
                    if (File.Exists(destPath))
                        File.Delete(destPath); // Üzerine yazılacaksa eskisini sil

                    File.Move(file.FullName, destPath);
                }
                catch (Exception ex)
                {
                    errors.Add($"'{file.Name}': {ex.Message}");
                }
            }

            if (errors.Any())
                return $"Bazı dosyalar taşınamadı:\n{string.Join("\n", errors)}";

            return matched.Count > 0
                ? $"{matched.Count} dosya '{destinationFolder}' klasörüne taşındı."
                : "Taşınacak dosya bulunamadı.";
        }
    }
}
